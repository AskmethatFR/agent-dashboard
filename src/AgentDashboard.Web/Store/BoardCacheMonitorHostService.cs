
namespace AgentDashboard.Web.Store;

internal sealed class BoardCacheMonitorHostService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private IDisposable? _monitorScope;

    public BoardCacheMonitorHostService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var scope = _serviceProvider.CreateScope();
        _monitorScope = scope;
        _ = scope.ServiceProvider.GetRequiredService<BoardCacheMonitor>();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _monitorScope?.Dispose();
        return Task.CompletedTask;
    }
}
