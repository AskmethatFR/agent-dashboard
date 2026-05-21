using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Boards;
using AgentDashboard.TicketTracking.Domain.Tickets;
using AgentDashboard.TicketTracking.Domain.UnitTests.Agents;
using AgentDashboard.TicketTracking.Domain.UnitTests.Tickets;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Boards;

public sealed class BoardSnapshotTests
{
    private static Agent DevA() => new AgentBuilder().WithId("DA").WithName("DevA").WithGlyph("Da").WithRole("Developer A").Build();
    private static Agent DevB() => new AgentBuilder().WithId("DB").WithName("DevB").WithGlyph("Db").WithRole("Developer B").Build();
    private static Agent Pm() => new AgentBuilder().WithId("PM").WithName("PM").WithGlyph("PM").WithRole("Project Manager").Build();

    private static BoardColumn CreatedColumn() => new(new BoardColumnId("CREATED"), new BoardColumnLabel("Created"));
    private static BoardColumn DevColumn() => new(new BoardColumnId("IN_DEVELOPMENT"), new BoardColumnLabel("In Development"));
    private static BoardColumn QaColumn() => new(new BoardColumnId("IN_QA"), new BoardColumnLabel("In QA"));

    [Fact]
    public void Should_Throw_ArgumentNullException_When_ColumnsIsNull()
    {
        var act = () => new BoardSnapshot(null!, Array.Empty<Ticket>(), Array.Empty<Agent>());

        act.Should().ThrowExactly<ArgumentNullException>()
            .WithParameterName("columns");
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_TicketsIsNull()
    {
        var act = () => new BoardSnapshot(Array.Empty<BoardColumn>(), null!, Array.Empty<Agent>());

        act.Should().ThrowExactly<ArgumentNullException>()
            .WithParameterName("tickets");
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_AgentsIsNull()
    {
        var act = () => new BoardSnapshot(Array.Empty<BoardColumn>(), Array.Empty<Ticket>(), null!);

        act.Should().ThrowExactly<ArgumentNullException>()
            .WithParameterName("agents");
    }

    [Fact]
    public void Should_BuildEmptySnapshot_When_AllCollectionsAreEmpty()
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
    public void Should_Throw_ArgumentException_When_TicketReferencesUnknownColumn()
    {
        var ticket = new TicketBuilder()
            .WithId(1)
            .WithColumn("MISSING")
            .WithAgent("DA")
            .AsOpen()
            .Build();

        var act = () => new BoardSnapshot(
            new[] { CreatedColumn() },
            new[] { ticket },
            new[] { DevA() });

        act.Should().ThrowExactly<ArgumentException>()
            .WithParameterName("tickets")
            .WithMessage("*unknown column*");
    }

    [Fact]
    public void Should_Throw_ArgumentException_When_TicketReferencesUnknownAgent()
    {
        var ticket = new TicketBuilder()
            .WithId(1)
            .WithColumn("CREATED")
            .WithAgent("GHOST")
            .AsOpen()
            .Build();

        var act = () => new BoardSnapshot(
            new[] { CreatedColumn() },
            new[] { ticket },
            new[] { DevA() });

        act.Should().ThrowExactly<ArgumentException>()
            .WithParameterName("tickets")
            .WithMessage("*unknown agent*");
    }

    [Fact]
    public void Should_Throw_ArgumentException_When_TicketReferencesUnknownCoAgent()
    {
        var ticket = new TicketBuilder()
            .WithId(1)
            .WithColumn("CREATED")
            .WithAgent("DA")
            .WithCoAgent("GHOST")
            .AsInCrossReview()
            .Build();

        var act = () => new BoardSnapshot(
            new[] { CreatedColumn() },
            new[] { ticket },
            new[] { DevA() });

        act.Should().ThrowExactly<ArgumentException>()
            .WithParameterName("tickets")
            .WithMessage("*unknown co-agent*");
    }

    [Fact]
    public void Should_Throw_ArgumentException_When_TicketReferencesUnknownEscalationTarget()
    {
        var ticket = new TicketBuilder()
            .WithId(1)
            .WithColumn("CREATED")
            .WithAgent("DA")
            .WithEscalationTarget("GHOST")
            .WithRetry(3)
            .AsEscalated()
            .Build();

        var act = () => new BoardSnapshot(
            new[] { CreatedColumn() },
            new[] { ticket },
            new[] { DevA() });

        act.Should().ThrowExactly<ArgumentException>()
            .WithParameterName("tickets")
            .WithMessage("*unknown escalation target*");
    }

    [Fact]
    public void Should_Throw_ArgumentException_When_TwoTicketsShareSameId()
    {
        var ticketOne = new TicketBuilder().WithId(1).WithColumn("CREATED").WithAgent("DA").AsOpen().Build();
        var ticketTwo = new TicketBuilder().WithId(1).WithColumn("CREATED").WithAgent("DA").AsOpen().Build();

        var act = () => new BoardSnapshot(
            new[] { CreatedColumn() },
            new[] { ticketOne, ticketTwo },
            new[] { DevA() });

        act.Should().ThrowExactly<ArgumentException>()
            .WithParameterName("tickets")
            .WithMessage("*Duplicate ticket id*");
    }

    [Fact]
    public void Should_BuildSnapshot_When_AllReferencesAreCoherent()
    {
        var ticket = new TicketBuilder()
            .WithId(1)
            .WithColumn("CREATED")
            .WithAgent("DA")
            .WithCoAgent("DB")
            .WithEscalationTarget("PM")
            .AsInCrossReview()
            .Build();

        var snapshot = new BoardSnapshot(
            new[] { CreatedColumn() },
            new[] { ticket },
            new[] { DevA(), DevB(), Pm() });

        snapshot.Tickets.Should().HaveCount(1);
        snapshot.Columns.Should().HaveCount(1);
        snapshot.Agents.Should().HaveCount(3);
    }

    [Fact]
    public void Should_BuildSnapshot_When_MultipleTicketsAcrossColumnsAndAgents()
    {
        var ticketOne = new TicketBuilder().WithId(1).WithColumn("CREATED").WithAgent("DA").AsOpen().Build();
        var ticketTwo = new TicketBuilder().WithId(2).WithColumn("IN_DEVELOPMENT").WithAgent("DB").AsOpen().Build();
        var ticketThree = new TicketBuilder().WithId(3).WithColumn("IN_QA").WithAgent("PM").AsOpen().Build();

        var snapshot = new BoardSnapshot(
            new[] { CreatedColumn(), DevColumn(), QaColumn() },
            new[] { ticketOne, ticketTwo, ticketThree },
            new[] { DevA(), DevB(), Pm() });

        int[] expectedTicketIds = [1, 2, 3];
        string[] expectedColumnIds = ["CREATED", "IN_DEVELOPMENT", "IN_QA"];
        string[] expectedAgentIds = ["DA", "DB", "PM"];

        snapshot.Tickets.Select(t => t.Id.Value).Should().BeEquivalentTo(expectedTicketIds);
        snapshot.Columns.Select(c => c.Id.Value).Should().BeEquivalentTo(expectedColumnIds);
        snapshot.Agents.Select(a => a.Id.Value).Should().BeEquivalentTo(expectedAgentIds);
    }
}
