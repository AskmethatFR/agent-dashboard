using AgentDashboard.TicketTracking.Application.Ports;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AgentDashboard.TicketTracking.Infrastructure.GitHub;

internal sealed partial class GitHubIssuesPoller : BackgroundService
{
    private readonly IGitHubIssuesClient _client;
    private readonly BoardRefreshTrigger _trigger;
    private readonly GitHubPollingOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<GitHubIssuesPoller> _logger;

    public GitHubIssuesPoller(
        IGitHubIssuesClient client,
        BoardRefreshTrigger trigger,
        GitHubPollingOptions options,
        TimeProvider timeProvider,
        ILogger<GitHubIssuesPoller> logger)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(trigger);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(logger);
        _client = client;
        _trigger = trigger;
        _options = options;
        _timeProvider = timeProvider;
        _logger = logger;
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
            var issues = await _client.GetOpenIssuesAsync(cancellationToken).ConfigureAwait(false);
            var elapsed = _timeProvider.GetElapsedTime(startTimestamp);
            var nextPollInSeconds = (int)Math.Max(0, (nextScheduledDeadline - _timeProvider.GetUtcNow()).TotalSeconds);

            GitHubIssuesPollerLog.PollSucceeded(
                _logger,
                $"{_options.RepositoryOwner}/{_options.RepositoryName}",
                issues.Count,
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
