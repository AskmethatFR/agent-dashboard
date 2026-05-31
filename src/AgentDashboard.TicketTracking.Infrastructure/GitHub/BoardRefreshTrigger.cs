using System.Threading.Channels;
using AgentDashboard.TicketTracking.Application.Ports;

namespace AgentDashboard.TicketTracking.Infrastructure.GitHub;

public readonly record struct RefreshSignal;

public sealed class BoardRefreshTrigger : IBoardRefreshTrigger
{
    private readonly Channel<RefreshSignal> _channel = Channel.CreateBounded<RefreshSignal>(
        new BoundedChannelOptions(capacity: 1)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true,
            SingleWriter = false,
        });

    public ChannelReader<RefreshSignal> Reader => _channel.Reader;

    public ValueTask TriggerNowAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _channel.Writer.TryWrite(default);
        return ValueTask.CompletedTask;
    }
}
