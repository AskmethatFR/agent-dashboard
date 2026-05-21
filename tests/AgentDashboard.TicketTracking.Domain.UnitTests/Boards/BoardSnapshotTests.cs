using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Boards;
using AgentDashboard.TicketTracking.Domain.Tickets;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Boards;

public sealed class BoardSnapshotTests
{
    private static Agent DevA() => new(new AgentId("DA"), "DevA", "Da", "Developer A");
    private static Agent DevB() => new(new AgentId("DB"), "DevB", "Db", "Developer B");
    private static Agent Pm() => new(new AgentId("PM"), "PM", "PM", "Project Manager");

    private static BoardColumn CreatedCol() => new(new BoardColumnId("CREATED"), "Created");
    private static BoardColumn DevCol() => new(new BoardColumnId("IN_DEVELOPMENT"), "In Development");
    private static BoardColumn QaCol() => new(new BoardColumnId("IN_QA"), "In QA");

    private static Ticket SimpleTicket(int id, BoardColumnId column, AgentId owner) =>
        Ticket.Open(
            new TicketId(id), column, "t", owner,
            new Retry(0), new Age(TimeSpan.Zero),
            thinking: false, TicketFreshness.Neutral);

    [Fact]
    public void RejectsNullColumns()
    {
        var act = () => new BoardSnapshot(null!, Array.Empty<Ticket>(), Array.Empty<Agent>());
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RejectsNullTickets()
    {
        var act = () => new BoardSnapshot(Array.Empty<BoardColumn>(), null!, Array.Empty<Agent>());
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RejectsNullAgents()
    {
        var act = () => new BoardSnapshot(Array.Empty<BoardColumn>(), Array.Empty<Ticket>(), null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AcceptsEmptySnapshot()
    {
        var snapshot = new BoardSnapshot(
            Array.Empty<BoardColumn>(),
            Array.Empty<Ticket>(),
            Array.Empty<Agent>());
        snapshot.Columns.Should().BeEmpty();
        snapshot.Tickets.Should().BeEmpty();
        snapshot.Agents.Should().BeEmpty();
    }

    [Fact]
    public void RejectsTicketReferencingUnknownColumn()
    {
        var ticket = Ticket.Open(
            new TicketId(1), new BoardColumnId("MISSING"), "t", new AgentId("DA"),
            new Retry(0), new Age(TimeSpan.Zero), thinking: false, TicketFreshness.Neutral);
        var act = () => new BoardSnapshot(
            new[] { CreatedCol() },
            new[] { ticket },
            new[] { DevA() });
        act.Should().Throw<ArgumentException>().WithMessage("*unknown column*");
    }

    [Fact]
    public void RejectsTicketReferencingUnknownAgent()
    {
        var act = () => new BoardSnapshot(
            new[] { CreatedCol() },
            new[] { SimpleTicket(1, new BoardColumnId("CREATED"), new AgentId("GHOST")) },
            new[] { DevA() });
        act.Should().Throw<ArgumentException>().WithMessage("*unknown agent*");
    }

    [Fact]
    public void RejectsTicketReferencingUnknownCoAgent()
    {
        var ticket = Ticket.InCrossReview(
            new TicketId(1), new BoardColumnId("CREATED"), "t",
            new AgentId("DA"), new AgentId("GHOST"),
            new Retry(0), new Age(TimeSpan.Zero), thinking: false, TicketFreshness.Neutral);
        var act = () => new BoardSnapshot(
            new[] { CreatedCol() },
            new[] { ticket },
            new[] { DevA() });
        act.Should().Throw<ArgumentException>().WithMessage("*unknown co-agent*");
    }

    [Fact]
    public void RejectsTicketReferencingUnknownEscalationTarget()
    {
        var ticket = Ticket.Escalated(
            new TicketId(1), new BoardColumnId("CREATED"), "t",
            new AgentId("DA"), new AgentId("GHOST"),
            new Retry(3), new Age(TimeSpan.Zero), thinking: false, TicketFreshness.Neutral);
        var act = () => new BoardSnapshot(
            new[] { CreatedCol() },
            new[] { ticket },
            new[] { DevA() });
        act.Should().Throw<ArgumentException>().WithMessage("*unknown escalation target*");
    }

    [Fact]
    public void RejectsDuplicateTicketIds()
    {
        var t1 = SimpleTicket(1, new BoardColumnId("CREATED"), new AgentId("DA"));
        var t2 = SimpleTicket(1, new BoardColumnId("CREATED"), new AgentId("DA"));
        var act = () => new BoardSnapshot(
            new[] { CreatedCol() },
            new[] { t1, t2 },
            new[] { DevA() });
        act.Should().Throw<ArgumentException>().WithMessage("*Duplicate ticket id*");
    }

    [Fact]
    public void AcceptsCoherentSnapshot()
    {
        var ticket = Ticket.InCrossReview(
            new TicketId(1), new BoardColumnId("CREATED"), "t",
            new AgentId("DA"), new AgentId("DB"),
            new Retry(0), new Age(TimeSpan.Zero), thinking: false, TicketFreshness.Neutral,
            escalationTarget: new AgentId("PM"));
        var snapshot = new BoardSnapshot(
            new[] { CreatedCol() },
            new[] { ticket },
            new[] { DevA(), DevB(), Pm() });
        snapshot.Tickets.Should().HaveCount(1);
        snapshot.Columns.Should().HaveCount(1);
        snapshot.Agents.Should().HaveCount(3);
    }

    [Fact]
    public void AcceptsMultipleTicketsAcrossColumnsAndAgents()
    {
        var t1 = SimpleTicket(1, new BoardColumnId("CREATED"), new AgentId("DA"));
        var t2 = SimpleTicket(2, new BoardColumnId("IN_DEVELOPMENT"), new AgentId("DB"));
        var t3 = SimpleTicket(3, new BoardColumnId("IN_QA"), new AgentId("PM"));

        var snapshot = new BoardSnapshot(
            new[] { CreatedCol(), DevCol(), QaCol() },
            new[] { t1, t2, t3 },
            new[] { DevA(), DevB(), Pm() });

        int[] expectedTicketIds = [1, 2, 3];
        string[] expectedColumnIds = ["CREATED", "IN_DEVELOPMENT", "IN_QA"];
        string[] expectedAgentIds = ["DA", "DB", "PM"];

        snapshot.Tickets.Select(t => t.Id.Value).Should().BeEquivalentTo(expectedTicketIds);
        snapshot.Columns.Select(c => c.Id.Value).Should().BeEquivalentTo(expectedColumnIds);
        snapshot.Agents.Select(a => a.Id.Value).Should().BeEquivalentTo(expectedAgentIds);
    }
}
