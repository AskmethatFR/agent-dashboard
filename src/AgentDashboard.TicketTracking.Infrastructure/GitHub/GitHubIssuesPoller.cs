using AgentDashboard.TicketTracking.Application.GitHub;
using AgentDashboard.TicketTracking.Application.Ports;
using AgentDashboard.TicketTracking.Infrastructure.Boards;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AgentDashboard.TicketTracking.Infrastructure.GitHub;

internal sealed partial class GitHubIssuesPoller : BackgroundService
{
    private readonly IGitHubIssuesClient _client;
    private readonly BoardSnapshotCache _cache;
    private readonly BoardRefreshTrigger _trigger;
    private readonly GitHubPollingOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<GitHubIssuesPoller> _logger;
    private readonly string _repoLabel;

    public GitHubIssuesPoller(
        IGitHubIssuesClient client,
        BoardSnapshotCache cache,
        BoardRefreshTrigger trigger,
        GitHubPollingOptions options,
        TimeProvider timeProvider,
        ILogger<GitHubIssuesPoller> logger)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(trigger);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(logger);
        _client = client;
        _cache = cache;
        _trigger = trigger;
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
            
            // Map the GitHub issues to a board snapshot and update the cache
            var snapshot = GitHubBoardMapper.MapToBoardSnapshot(records, now);
            _cache.Update(snapshot, now);

            var elapsed = _timeProvider.GetElapsedTime(startTimestamp);
            var nextPollInSeconds = (int)Math.Max(0, (nextScheduledDeadline - now).TotalSeconds);

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
#pragma warning disable CA1031 // intentional: a single failing poll must not crash the host
        catch (Exception ex)
#pragma warning restore CA1031
        {
            GitHubIssuesPollerLog.PollFailed(_logger, ex);
        }
    }
}

internal static partial class GitHubIssuesPollerLog
{
    [LoggerMessage(
        EventId = 200,
        Level = LogLevel.Information,
        Message = "GitHub poll completed for {repo} - {issue_count} open issue(s) in {duration_ms} ms; next poll in {next_poll_in_seconds}s.")]
    public static partial void PollSucceeded(ILogger logger, string repo, int issue_count, long duration_ms, int next_poll_in_seconds);

    [LoggerMessage(
        EventId = 201,
        Level = LogLevel.Error,
        Message = "GitHub poll failed.")]
    public static partial void PollFailed(ILogger logger, Exception exception);
}
