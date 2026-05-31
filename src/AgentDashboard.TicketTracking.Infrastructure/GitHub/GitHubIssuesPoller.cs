using AgentDashboard.TicketTracking.Application.GitHub;
using AgentDashboard.TicketTracking.Application.Ports;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AgentDashboard.TicketTracking.Infrastructure.GitHub;

public sealed partial class GitHubIssuesPoller : BackgroundService
{
    private const int MinimumNextPollSeconds = 0;

    private readonly IGitHubIssuesClient _client;
    private readonly IBoardSnapshotUpdater _snapshotUpdater;
    private readonly BoardRefreshTrigger _trigger;
    private readonly ITicketWriteRepository _ticketWriteRepository;
    private readonly GitHubPollingOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<GitHubIssuesPoller> _logger;
    private readonly string _repoLabel;

    internal GitHubIssuesPoller(
        IGitHubIssuesClient client,
        IBoardSnapshotUpdater snapshotUpdater,
        BoardRefreshTrigger trigger,
        ITicketWriteRepository ticketWriteRepository,
        GitHubPollingOptions options,
        TimeProvider timeProvider,
        ILogger<GitHubIssuesPoller> logger)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(snapshotUpdater);
        ArgumentNullException.ThrowIfNull(trigger);
        ArgumentNullException.ThrowIfNull(ticketWriteRepository);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(logger);
        _client = client;
        _snapshotUpdater = snapshotUpdater;
        _trigger = trigger;
        _ticketWriteRepository = ticketWriteRepository;
        _options = options;
        _timeProvider = timeProvider;
        _logger = logger;
        _repoLabel = $"{options.RepositoryOwner}/{options.RepositoryName}";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var nextScheduledDeadline = _timeProvider.GetUtcNow() + _options.PollInterval;
        await PollOnceAsync(nextScheduledDeadline, stoppingToken).ConfigureAwait(false);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = _timeProvider.GetUtcNow();
            var delayUntilDeadline = nextScheduledDeadline - now;
            if (delayUntilDeadline < TimeSpan.Zero)
            {
                delayUntilDeadline = TimeSpan.Zero;
            }

            var timerTask = Task.Delay(delayUntilDeadline, _timeProvider, stoppingToken);
            var triggerTask = _trigger.Reader.WaitToReadAsync(stoppingToken).AsTask();

            var completed = await Task.WhenAny(timerTask, triggerTask).ConfigureAwait(false);
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            if (completed == timerTask)
            {
                nextScheduledDeadline = _timeProvider.GetUtcNow() + _options.PollInterval;
                await PollOnceAsync(nextScheduledDeadline, stoppingToken).ConfigureAwait(false);
            }
            else
            {
                _trigger.Reader.TryRead(out _);
                await PollOnceAsync(nextScheduledDeadline, stoppingToken).ConfigureAwait(false);
            }
        }
    }

    private async Task PollOnceAsync(DateTimeOffset nextScheduledDeadline, CancellationToken cancellationToken)
    {
        var startTimestamp = _timeProvider.GetTimestamp();
        try
        {
            var records = await _client.GetOpenIssuesAsync(cancellationToken).ConfigureAwait(false);
            var now = _timeProvider.GetUtcNow();
            
            // Save tickets to SQLite
            foreach (var record in records)
            {
                var mappingResult = GitHubIssueToTicketMapper.Map(record);
                await _ticketWriteRepository.SaveAsync(mappingResult.Ticket, cancellationToken).ConfigureAwait(false);

                foreach (var warning in mappingResult.Warnings)
                {
                    GitHubIssuesPollerLog.LabelMappingWarning(_logger, GitHubLogSanitizer.Sanitize(FormatWarning(warning)));
                }
            }

            // Update the board snapshot via the port
            _snapshotUpdater.Update(records, now);

            var elapsed = _timeProvider.GetElapsedTime(startTimestamp);
            var nextPollInSeconds = (int)Math.Max(MinimumNextPollSeconds, (nextScheduledDeadline - now).TotalSeconds);

            GitHubIssuesPollerLog.PollSucceeded(
                _logger,
                _repoLabel,
                records.Count,
                (long)elapsed.TotalMilliseconds,
                nextPollInSeconds);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            GitHubIssuesPollerLog.PollFailed(_logger, ex.GetType().Name, SanitizeExceptionMessage(ex));
        }
    }

    private static string SanitizeExceptionMessage(Exception ex) => GitHubLogSanitizer.Sanitize(ex.ToString());

    private static string FormatWarning(MappingWarning warning) => warning.Kind switch
    {
        MappingWarningKind.MultipleStatusLabels =>
            $"Issue #{warning.IssueNumber}: multiple status labels {string.Join(", ", warning.ConflictingStatusLabels)} — selected '{warning.SelectedStatusLabel}' (latest in state machine).",
        MappingWarningKind.MissingStatusLabel =>
            $"Issue #{warning.IssueNumber}: no status label — defaulted to 'status:created'.",
        _ => $"Issue #{warning.IssueNumber}: label mapping anomaly."
    };
}

public static partial class GitHubIssuesPollerLog
{
    private const int PollSucceededEventId = 200;
    private const int PollFailedEventId = 201;
    private const int LabelMappingWarningEventId = 202;

    [LoggerMessage(
        EventId = PollSucceededEventId,
        Level = LogLevel.Information,
        Message = "GitHub poll completed for {repo} - {issue_count} open issue(s) in {duration_ms} ms; next poll in {next_poll_in_seconds}s.")]
    public static partial void PollSucceeded(ILogger logger, string repo, int issue_count, long duration_ms, int next_poll_in_seconds);

    [LoggerMessage(
        EventId = PollFailedEventId,
        Level = LogLevel.Error,
        Message = "GitHub poll failed: {exception_type} - {exception_message}")]
    public static partial void PollFailed(ILogger logger, string exception_type, string exception_message);

    [LoggerMessage(
        EventId = LabelMappingWarningEventId,
        Level = LogLevel.Warning,
        Message = "{label_mapping_warning}")]
    public static partial void LabelMappingWarning(ILogger logger, string label_mapping_warning);
}
