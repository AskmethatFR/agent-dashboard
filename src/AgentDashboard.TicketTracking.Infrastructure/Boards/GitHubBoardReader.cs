using AgentDashboard.TicketTracking.Application.Ports;
using AgentDashboard.TicketTracking.Domain.Boards;
using Microsoft.Extensions.Logging;

namespace AgentDashboard.TicketTracking.Infrastructure.Boards;

/// <summary>
/// Adapter that implements IBoardReader to provide real data from GitHub.
/// Uses a cache to avoid unnecessary API calls and implements freshness logic.
/// </summary>
public sealed partial class GitHubBoardReader : IBoardReader
{
    private readonly BoardSnapshotCache _cache;
    private readonly IGitHubIssuesClient _client;
    private readonly IBoardSnapshotUpdater _snapshotUpdater;
    private const int CacheFreshnessRatio = 2; // Cache valid for half the poll interval
    private readonly TimeSpan _pollInterval;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<GitHubBoardReader> _logger;

    /// <summary>
    /// Initializes a new instance of the GitHubBoardReader.
    /// </summary>
    /// <param name="cache">The cache for storing board snapshots.</param>
    /// <param name="client">The GitHub issues client for fetching data.</param>
    /// <param name="snapshotUpdater">The updater for board snapshots.</param>
    /// <param name="pollInterval">The polling interval for freshness calculation.</param>
    /// <param name="timeProvider">The time provider for getting current time.</param>
    /// <param name="logger">The logger for error logging.</param>
    public GitHubBoardReader(
        BoardSnapshotCache cache,
        IGitHubIssuesClient client,
        IBoardSnapshotUpdater snapshotUpdater,
        TimeSpan pollInterval,
        TimeProvider timeProvider,
        ILogger<GitHubBoardReader> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _snapshotUpdater = snapshotUpdater ?? throw new ArgumentNullException(nameof(snapshotUpdater));
        _pollInterval = pollInterval;
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the current board snapshot, either from cache or by polling GitHub.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current board snapshot.</returns>
    /// <exception cref="InvalidOperationException">Thrown when polling fails and cache is empty.</exception>
    public async Task<BoardSnapshot> GetCurrentAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Check if we have recent data in cache
        var cached = _cache.GetLatest();
        if (cached is not null && IsCacheFresh())
        {
            return cached;
        }

        // Cache miss or stale - poll GitHub
        try
        {
            var snapshot = await PollAndMapAsync(cancellationToken).ConfigureAwait(false);
            return snapshot;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (cached is not null)
        {
            // Don't break the cache if polling fails but we have cached data
            LogFailedToPollGitHubReturningCached(_logger, ex);
            return cached;
        }
        // If no cached data and polling fails, let the exception propagate
    }

    /// <summary>
    /// Checks if the cache is fresh based on the poll interval.
    /// Cache is considered fresh if LastUpdated is within PollInterval/2.
    /// </summary>
    private bool IsCacheFresh()
    {
        if (_pollInterval <= TimeSpan.Zero)
        {
            // Zero or negative interval means always poll
            return false;
        }

        var lastUpdated = _cache.LastUpdated;
        if (lastUpdated == DateTimeOffset.MinValue)
        {
            // Cache has never been updated
            return false;
        }

        var now = _timeProvider.GetUtcNow();
        var cacheAge = now - lastUpdated;
        var freshnessThreshold = _pollInterval / CacheFreshnessRatio;

        return cacheAge < freshnessThreshold;
    }

    /// <summary>
    /// Polls GitHub for issues, maps them to a board snapshot, and updates the cache.
    /// </summary>
    private async Task<BoardSnapshot> PollAndMapAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var records = await _client.GetOpenIssuesAsync(cancellationToken).ConfigureAwait(false);
        var now = _timeProvider.GetUtcNow();
        
        // Use the updater to map and cache the snapshot
        _snapshotUpdater.Update(records, now);
        
        // Return the cached snapshot
        var cached = _cache.GetLatest();
        if (cached is null)
        {
            throw new InvalidOperationException("Snapshot was not updated in cache");
        }
        return cached;
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Failed to poll GitHub, returning cached data.")]
    private static partial void LogFailedToPollGitHubReturningCached(ILogger logger, Exception exception);
}
