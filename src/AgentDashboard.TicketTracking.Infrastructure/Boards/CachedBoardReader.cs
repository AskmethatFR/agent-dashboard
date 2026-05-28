using AgentDashboard.TicketTracking.Application.Ports;
using AgentDashboard.TicketTracking.Domain.Boards;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgentDashboard.TicketTracking.Infrastructure.Boards;

/// <summary>
/// Adapter that implements IBoardReader to provide board snapshots from the cache.
/// </summary>
public sealed class CachedBoardReader : IBoardReader
{
    private readonly BoardSnapshotCache _cache;
    private readonly ILogger<CachedBoardReader> _logger;

    /// <summary>
    /// Initializes a new instance of the CachedBoardReader.
    /// </summary>
    /// <param name="cache">The cache for storing board snapshots.</param>
    /// <param name="logger">The logger for debug and warning messages.</param>
    public CachedBoardReader(BoardSnapshotCache cache, ILogger<CachedBoardReader>? logger = null)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? NullLogger<CachedBoardReader>.Instance;
    }

    /// <summary>
    /// Gets the current board snapshot from the cache.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current board snapshot.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the cache is empty.</exception>
    public Task<BoardSnapshot> GetCurrentAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var cached = _cache.GetLatest();
        
        if (cached is null)
        {
            CachedBoardReaderLog.CacheMiss(_logger);
            throw new InvalidOperationException(
                "Board snapshot cache is empty. The cache has not been populated yet.");
        }
        
        CachedBoardReaderLog.CacheHit(_logger);
        return Task.FromResult(cached);
    }
}

internal static partial class CachedBoardReaderLog
{
    [LoggerMessage(
        EventId = 400,
        Level = LogLevel.Debug,
        Message = "CachedBoardReader: Cache hit - returning cached board snapshot.")]
    public static partial void CacheHit(ILogger logger);

    [LoggerMessage(
        EventId = 401,
        Level = LogLevel.Warning,
        Message = "CachedBoardReader: Cache miss - cache is empty.")]
    public static partial void CacheMiss(ILogger logger);
}
