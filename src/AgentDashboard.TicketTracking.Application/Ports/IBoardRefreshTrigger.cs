namespace AgentDashboard.TicketTracking.Application.Ports;

public interface IBoardRefreshTrigger
{
    ValueTask TriggerNowAsync(CancellationToken cancellationToken);
}
