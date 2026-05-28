using AgentDashboard.TicketTracking.Infrastructure.Boards;
using Blazor.Redux.Interfaces;
using Microsoft.Extensions.Logging;

namespace AgentDashboard.Web.Store;

/// <summary>
/// Service that monitors the BoardSnapshotCache and dispatches LoadBoardAction
/// when the cache is updated, enabling live refresh of the board data.
/// </summary>
public sealed class BoardCacheMonitor : IDisposable
{
    private readonly BoardSnapshotCache _cache;
    private readonly IAsyncDispatcher _dispatcher;
    private readonly ILogger<BoardCacheMonitor> _logger;

    /// <summary>
    /// Initializes a new instance of the BoardCacheMonitor.
    /// </summary>
    /// <param name="cache">The board snapshot cache to monitor.</param>
    /// <param name="dispatcher">The async dispatcher for Redux actions.</param>
    /// <param name="logger">The logger for error handling.</param>
    /// <exception cref="ArgumentNullException">Thrown when cache, dispatcher, or logger is null.</exception>
    public BoardCacheMonitor(
        BoardSnapshotCache cache,
        IAsyncDispatcher dispatcher,
        ILogger<BoardCacheMonitor> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache.OnUpdated += HandleCacheUpdated;
    }

    /// <summary>
    /// Handles the cache updated event by dispatching a LoadBoardAction.
    /// </summary>
    private async void HandleCacheUpdated()
    {
        try
        {
            await HandleCacheUpdatedAsync();
        }
        catch (Exception ex)
        {
            BoardCacheMonitorLog.HandleCacheUpdatedFailed(_logger, ex.GetType().Name, ex.Message);
        }
    }

    private async Task HandleCacheUpdatedAsync()
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

// Logger messages for BoardCacheMonitor
internal static partial class BoardCacheMonitorLog
{
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Error,
        Message = "Failed to handle cache update: {exception_type} - {exception_message}")]
    public static partial void HandleCacheUpdatedFailed(ILogger logger, string exception_type, string exception_message);
}
