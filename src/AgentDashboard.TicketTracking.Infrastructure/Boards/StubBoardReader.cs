using AgentDashboard.TicketTracking.Application.Ports;
using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Boards;
using AgentDashboard.TicketTracking.Domain.Tickets;

namespace AgentDashboard.TicketTracking.Infrastructure.Boards;

public sealed class StubBoardReader : IBoardReader
{
    public const string SeedTitle = "First ticket";

    private static readonly BoardSnapshot Snapshot = BuildSeedSnapshot();

    public Task<BoardSnapshot> GetCurrentAsync(CancellationToken cancellationToken) =>
        Task.FromResult(Snapshot);

    private static BoardSnapshot BuildSeedSnapshot()
    {
        var createdColumn = new BoardColumn(
            new BoardColumnId("CREATED"),
            new BoardColumnLabel("Created"));

        var pm = new Agent(
            new AgentId("pm"),
            new AgentName("Project Manager"),
            new AgentGlyph("PM"),
            new AgentRole("project-manager"));

        var ticket = Ticket.Open(
            new TicketId(1),
            createdColumn.Id,
            new TicketTitle(SeedTitle),
            pm.Id,
            new Retry(0),
            new Age(TimeSpan.Zero),
            thinking: false,
            freshness: TicketFreshness.Neutral);

        return new BoardSnapshot(
            columns: [createdColumn],
            tickets: [ticket],
            agents: [pm]);
    }
}
