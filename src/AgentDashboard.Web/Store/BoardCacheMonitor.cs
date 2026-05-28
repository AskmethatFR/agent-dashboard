using AgentDashboard.TicketTracking.Infrastructure.Boards;
using Blazor.Redux.Interfaces;

namespace AgentDashboard.Web.Store;

/// <summary>
/// Service that monitors the BoardSnapshotCache and dispatches LoadBoardAction
/// when the cache is updated, enabling live refresh of the board data.
/// </summary>
public sealed class BoardCacheMonitor : IDisposable
{
    private readonly BoardSnapshotCache _cache;
    private readonly IAsyncDispatcher _dispatcher;

    /// <summary>
    /// Initializes a new instance of the BoardCacheMonitor.
    /// </summary>
    /// <param name="cache">The board snapshot cache to monitor.</param>
    /// <param name="dispatcher">The async dispatcher for Redux actions.</param>
    /// <exception cref="ArgumentNullException">Thrown when cache or dispatcher is null.</exception>
    public BoardCacheMonitor(BoardSnapshotCache cache, IAsyncDispatcher dispatcher)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _cache.OnUpdated += HandleCacheUpdated;
    }

    /// <summary>
    /// Handles the cache updated event by dispatching a LoadBoardAction.
    /// </summary>
    private async void HandleCacheUpdated()
    {
        await _dispatcher.DispatchAsync<BoardSlice, LoadBoardAction>(new LoadBoardAction());
    }

    /// <summary>
    /// Disposes the monitor by unsubscribing from the cache event.
    /// </summary>
    public void Dispose()
    {
        _cache.OnUpdated -= HandleCacheUpdated;
    }
}
