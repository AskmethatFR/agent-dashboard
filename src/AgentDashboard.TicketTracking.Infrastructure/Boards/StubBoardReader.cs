using AgentDashboard.TicketTracking.Application.Ports;
using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Boards;
using AgentDashboard.TicketTracking.Domain.Tickets;

namespace AgentDashboard.TicketTracking.Infrastructure.Boards;

public sealed class StubBoardReader : IBoardReader
{
    private static readonly BoardSnapshot Snapshot = BuildSeedSnapshot();

    public Task<BoardSnapshot> GetCurrentAsync(CancellationToken cancellationToken) =>
        Task.FromResult(Snapshot);

    private static BoardSnapshot BuildSeedSnapshot()
    {
        var created = new BoardColumn(new BoardColumnId("CREATED"), new BoardColumnLabel("Created"));
        var specified = new BoardColumn(new BoardColumnId("SPECIFIED"), new BoardColumnLabel("Specified"));
        var inDevelopment = new BoardColumn(new BoardColumnId("IN_DEVELOPMENT"), new BoardColumnLabel("In Development"));
        var inReview = new BoardColumn(new BoardColumnId("IN_REVIEW"), new BoardColumnLabel("In Review"));
        var inQa = new BoardColumn(new BoardColumnId("IN_QA"), new BoardColumnLabel("In Qa"));
        var awaitingValidation = new BoardColumn(new BoardColumnId("AWAITING_VALIDATION"), new BoardColumnLabel("Awaiting Validation"));
        var done = new BoardColumn(new BoardColumnId("DONE"), new BoardColumnLabel("Done"));

        var pm = new Agent(
            new AgentId("pm"),
            new AgentName("Project Manager"),
            new AgentGlyph("PM"),
            new AgentRole("project-manager"));
        var devA = new Agent(
            new AgentId("dev-a"),
            new AgentName("Developer A"),
            new AgentGlyph("DA"),
            new AgentRole("developer"));
        var devB = new Agent(
            new AgentId("dev-b"),
            new AgentName("Developer B"),
            new AgentGlyph("DB"),
            new AgentRole("developer"));

        var tickets = new List<TicketSnapshot>
        {
            // Created (2)
            SeedTicket(1, created.Id, "wire github polling", pm.Id, retrySteps: 0, ageMinutes: 4),
            SeedTicket(2, created.Id, "add dark mode toggle", pm.Id, retrySteps: 0, ageMinutes: 12),

            // Specified (1)
            SeedTicket(3, specified.Id, "design board read model", pm.Id, retrySteps: 0, ageMinutes: 22),

            // In Development (2)
            SeedTicket(4, inDevelopment.Id, "implement health check", devA.Id, retrySteps: 1, ageMinutes: 35),
            SeedTicket(5, inDevelopment.Id, "wire sqlite migrations", devB.Id, retrySteps: 0, ageMinutes: 18),

            // In Review (1)
            SeedTicket(6, inReview.Id, "review api error mapping", devA.Id, retrySteps: 2, ageMinutes: 47),

            // In Qa (1)
            SeedTicket(7, inQa.Id, "qa pass on board snapshot", devB.Id, retrySteps: 1, ageMinutes: 28),

            // Awaiting Validation (0) — intentionally empty

            // Done (2)
            SeedTicket(8, done.Id, "scaffold solution layout", pm.Id, retrySteps: 0, ageMinutes: 240),
            SeedTicket(9, done.Id, "publish first walking skeleton", devA.Id, retrySteps: 0, ageMinutes: 180),
        };

        return new BoardSnapshot(
            columns: [created, specified, inDevelopment, inReview, inQa, awaitingValidation, done],
            tickets: tickets,
            agents: [pm, devA, devB]);
    }

    private static TicketSnapshot SeedTicket(
        int id,
        BoardColumnId columnId,
        string title,
        AgentId agentId,
        int retrySteps,
        int ageMinutes) =>
        TicketSnapshot.Open(
            new TicketId(id),
            columnId,
            new TicketTitle(title),
            agentId,
            new Retry(retrySteps),
            new Age(TimeSpan.FromMinutes(ageMinutes)),
            thinking: false,
            freshness: TicketFreshness.Neutral);
}
