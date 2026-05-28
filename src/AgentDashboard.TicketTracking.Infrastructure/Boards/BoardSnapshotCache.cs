using AgentDashboard.TicketTracking.Domain.Boards;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgentDashboard.TicketTracking.Infrastructure.Boards;

public sealed class BoardSnapshotCache : IDisposable
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly ILogger<BoardSnapshotCache> _logger;
    private BoardSnapshot? _snapshot;
    private DateTimeOffset _lastUpdated;

    public BoardSnapshotCache(ILogger<BoardSnapshotCache>? logger = null)
    {
        _logger = logger ?? NullLogger<BoardSnapshotCache>.Instance;
        _snapshot = null;
        _lastUpdated = DateTimeOffset.MinValue;
    }

    public BoardSnapshot? GetLatest()
    {
        _lock.EnterReadLock();
        try
        {
            BoardSnapshotCacheLog.GetLatestCalled(_logger);
            return _snapshot;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Update(BoardSnapshot snapshot, DateTimeOffset asOf)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        _lock.EnterWriteLock();
        try
        {
            _snapshot = snapshot;
            _lastUpdated = asOf;
            BoardSnapshotCacheLog.UpdateCalled(_logger, asOf);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public DateTimeOffset LastUpdated
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _lastUpdated;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public void Dispose()
    {
        _lock.Dispose();
    }
}

internal static partial class BoardSnapshotCacheLog
{
    [LoggerMessage(
        EventId = 300,
        Level = LogLevel.Debug,
        Message = "BoardSnapshotCache.GetLatest() called.")]
    public static partial void GetLatestCalled(ILogger logger);

    [LoggerMessage(
        EventId = 301,
        Level = LogLevel.Debug,
        Message = "BoardSnapshotCache.Update() called with asOf={asOf}.")]
    public static partial void UpdateCalled(ILogger logger, DateTimeOffset asOf);
}
