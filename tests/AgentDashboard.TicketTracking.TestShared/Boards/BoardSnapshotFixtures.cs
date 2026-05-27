using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Boards;
using AgentDashboard.TicketTracking.Domain.Tickets;

namespace AgentDashboard.TicketTracking.TestShared.Boards;

public static class BoardSnapshotFixtures
{
    public static BoardSnapshot Empty { get; } = new(
        Array.Empty<BoardColumn>(),
        Array.Empty<TicketSnapshot>(),
        Array.Empty<Agent>());

    public static BoardSnapshot Build(
        IEnumerable<BoardColumn>? columns = null,
        IEnumerable<TicketSnapshot>? tickets = null,
        IEnumerable<Agent>? agents = null) =>
        new(
            (columns ?? Array.Empty<BoardColumn>()).ToArray(),
            (tickets ?? Array.Empty<TicketSnapshot>()).ToArray(),
            (agents ?? Array.Empty<Agent>()).ToArray());
}
