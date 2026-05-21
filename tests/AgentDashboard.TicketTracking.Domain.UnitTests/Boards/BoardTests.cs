using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Boards;
using AgentDashboard.TicketTracking.Domain.Tickets;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Boards;

public sealed class BoardTests
{
    private static Agent DevA() => new(new AgentId("DA"), "DevA", "Da", "Developer A");
    private static Agent DevB() => new(new AgentId("DB"), "DevB", "Db", "Developer B");
    private static Agent Pm() => new(new AgentId("PM"), "PM", "PM", "Project Manager");

    private static BoardColumn CreatedCol() => new(new BoardColumnId("CREATED"), "Created");

    private static Ticket SimpleTicket(AgentId owner) => new(
        new TicketId(1), new BoardColumnId("CREATED"), "t",
        owner, null, false,
        new Retry(0), new Age(TimeSpan.Zero),
        false, false, false, false, null);

    [Fact]
    public void RejectsTicketReferencingUnknownColumn()
    {
        var ticket = new Ticket(
            new TicketId(1), new BoardColumnId("MISSING"), "t",
            new AgentId("DA"), null, false,
            new Retry(0), new Age(TimeSpan.Zero),
            false, false, false, false, null);
        var act = () => new Board(
            new[] { CreatedCol() },
            new[] { ticket },
            new[] { DevA() });
        act.Should().Throw<ArgumentException>().WithMessage("*unknown column*");
    }

    [Fact]
    public void RejectsTicketReferencingUnknownAgent()
    {
        var act = () => new Board(
            new[] { CreatedCol() },
            new[] { SimpleTicket(new AgentId("GHOST")) },
            new[] { DevA() });
        act.Should().Throw<ArgumentException>().WithMessage("*unknown agent*");
    }

    [Fact]
    public void RejectsTicketReferencingUnknownCoAgent()
    {
        var ticket = new Ticket(
            new TicketId(1), new BoardColumnId("CREATED"), "t",
            new AgentId("DA"), coAgentId: new AgentId("GHOST"), crossReview: true,
            new Retry(0), new Age(TimeSpan.Zero),
            false, false, false, false, null);
        var act = () => new Board(
            new[] { CreatedCol() },
            new[] { ticket },
            new[] { DevA() });
        act.Should().Throw<ArgumentException>().WithMessage("*unknown co-agent*");
    }

    [Fact]
    public void RejectsTicketReferencingUnknownEscalationTarget()
    {
        var ticket = new Ticket(
            new TicketId(1), new BoardColumnId("CREATED"), "t",
            new AgentId("DA"), null, false,
            new Retry(0), new Age(TimeSpan.Zero),
            false, false, false, escalated: true, escalationTarget: new AgentId("GHOST"));
        var act = () => new Board(
            new[] { CreatedCol() },
            new[] { ticket },
            new[] { DevA() });
        act.Should().Throw<ArgumentException>().WithMessage("*unknown escalation target*");
    }

    [Fact]
    public void AcceptsCoherentBoard()
    {
        var ticket = new Ticket(
            new TicketId(1), new BoardColumnId("CREATED"), "t",
            new AgentId("DA"), coAgentId: new AgentId("DB"), crossReview: true,
            new Retry(0), new Age(TimeSpan.Zero),
            false, false, false, escalated: true, escalationTarget: new AgentId("PM"));
        var board = new Board(
            new[] { CreatedCol() },
            new[] { ticket },
            new[] { DevA(), DevB(), Pm() });
        board.Tickets.Should().HaveCount(1);
        board.Columns.Should().HaveCount(1);
        board.Agents.Should().HaveCount(3);
    }
}
