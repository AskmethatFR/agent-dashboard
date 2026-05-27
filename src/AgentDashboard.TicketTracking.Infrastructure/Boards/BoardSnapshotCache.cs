using AgentDashboard.TicketTracking.Domain.Boards;

namespace AgentDashboard.TicketTracking.Infrastructure.Boards;

public sealed class BoardSnapshotCache : IDisposable
{
    private readonly ReaderWriterLockSlim _lock = new();
    private BoardSnapshot? _snapshot;
    private DateTimeOffset _lastUpdated;

    public BoardSnapshotCache()
    {
        _snapshot = null;
        _lastUpdated = DateTimeOffset.MinValue;
    }

    public BoardSnapshot? GetLatest()
    {
        _lock.EnterReadLock();
        try
        {
            return _snapshot;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Update(BoardSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        
        _lock.EnterWriteLock();
        try
        {
            _snapshot = snapshot;
            _lastUpdated = DateTimeOffset.UtcNow;
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
