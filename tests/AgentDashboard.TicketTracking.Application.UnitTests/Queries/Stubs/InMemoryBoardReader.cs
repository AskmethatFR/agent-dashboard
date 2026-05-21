using AgentDashboard.TicketTracking.Application.Ports;
using AgentDashboard.TicketTracking.Domain.Boards;

namespace AgentDashboard.TicketTracking.Application.UnitTests.Queries.Stubs;

internal sealed class InMemoryBoardReader : IBoardReader
{
    private readonly BoardSnapshot _snapshot;

    public InMemoryBoardReader(BoardSnapshot snapshot) => _snapshot = snapshot;

    public Task<BoardSnapshot> GetCurrentAsync(CancellationToken cancellationToken)
        => Task.FromResult(_snapshot);
}
