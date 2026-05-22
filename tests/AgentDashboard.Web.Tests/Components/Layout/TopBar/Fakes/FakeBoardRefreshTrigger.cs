using AgentDashboard.TicketTracking.Application.Ports;

namespace AgentDashboard.Web.Tests.Components.Layout.TopBar.Fakes;

internal sealed class FakeBoardRefreshTrigger : IBoardRefreshTrigger
{
    public int CallCount { get; private set; }

    public TaskCompletionSource? Gate { get; set; }

    public Exception? ToThrow { get; set; }

    public async ValueTask TriggerNowAsync(CancellationToken cancellationToken)
    {
        CallCount++;

        if (Gate is not null)
        {
            await Gate.Task.ConfigureAwait(false);
        }

        if (ToThrow is not null)
        {
            throw ToThrow;
        }
    }
}
