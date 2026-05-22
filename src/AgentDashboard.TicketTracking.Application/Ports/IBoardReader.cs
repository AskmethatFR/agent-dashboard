using AgentDashboard.TicketTracking.Domain.Boards;

namespace AgentDashboard.TicketTracking.Application.Ports;

public interface IBoardReader
{
    Task<BoardSnapshot> GetCurrentAsync(CancellationToken cancellationToken);
}
