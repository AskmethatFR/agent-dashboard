using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Boards;
using AgentDashboard.TicketTracking.Domain.Tickets;
using AgentDashboard.TicketTracking.TestShared.Agents;
using AgentDashboard.TicketTracking.TestShared.Boards;
using AgentDashboard.TicketTracking.TestShared.Tickets;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Boards;

public sealed class BoardSnapshotTests
{
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
        var snapshot = BoardSnapshotFixtures.Empty;

        snapshot.Columns.Should().BeEmpty();
        snapshot.Tickets.Should().BeEmpty();
        snapshot.Agents.Should().BeEmpty();
    }

    [Fact]
    public void Should_Throw_ArgumentException_When_TicketReferencesUnknownColumn()
    {
        var ticket = TicketFixtures.Open(id: 1, columnId: "MISSING", agentId: "DA");

        var act = () => BoardSnapshotFixtures.Build(
            columns: new[] { BoardColumnFixtures.Created },
            tickets: new[] { ticket },
            agents: new[] { AgentFixtures.DevA });

        act.Should().ThrowExactly<ArgumentException>()
            .WithParameterName("tickets")
            .WithMessage("*unknown column*");
    }

    [Fact]
    public void Should_Throw_ArgumentException_When_TicketReferencesUnknownAgent()
    {
        var ticket = TicketFixtures.Open(id: 1, columnId: "CREATED", agentId: "GHOST");

        var act = () => BoardSnapshotFixtures.Build(
            columns: new[] { BoardColumnFixtures.Created },
            tickets: new[] { ticket },
            agents: new[] { AgentFixtures.DevA });

        act.Should().ThrowExactly<ArgumentException>()
            .WithParameterName("tickets")
            .WithMessage("*unknown agent*");
    }

    [Fact]
    public void Should_Throw_ArgumentException_When_TicketReferencesUnknownCoAgent()
    {
        var ticket = TicketFixtures.InCrossReview(
            id: 1, columnId: "CREATED", agentId: "DA", coAgentId: "GHOST");

        var act = () => BoardSnapshotFixtures.Build(
            columns: new[] { BoardColumnFixtures.Created },
            tickets: new[] { ticket },
            agents: new[] { AgentFixtures.DevA });

        act.Should().ThrowExactly<ArgumentException>()
            .WithParameterName("tickets")
            .WithMessage("*unknown co-agent*");
    }

    [Fact]
    public void Should_Throw_ArgumentException_When_TicketReferencesUnknownEscalationTarget()
    {
        var ticket = TicketFixtures.Escalated(
            id: 1, columnId: "CREATED", agentId: "DA", escalationTarget: "GHOST", retry: 3);

        var act = () => BoardSnapshotFixtures.Build(
            columns: new[] { BoardColumnFixtures.Created },
            tickets: new[] { ticket },
            agents: new[] { AgentFixtures.DevA });

        act.Should().ThrowExactly<ArgumentException>()
            .WithParameterName("tickets")
            .WithMessage("*unknown escalation target*");
    }

    [Fact]
    public void Should_Throw_ArgumentException_When_TwoTicketsShareSameId()
    {
        var ticketOne = TicketFixtures.Open(id: 1, columnId: "CREATED", agentId: "DA");
        var ticketTwo = TicketFixtures.Open(id: 1, columnId: "CREATED", agentId: "DA");

        var act = () => BoardSnapshotFixtures.Build(
            columns: new[] { BoardColumnFixtures.Created },
            tickets: new[] { ticketOne, ticketTwo },
            agents: new[] { AgentFixtures.DevA });

        act.Should().ThrowExactly<ArgumentException>()
            .WithParameterName("tickets")
            .WithMessage("*Duplicate ticket id*");
    }

    [Fact]
    public void Should_BuildSnapshot_When_AllReferencesAreCoherent()
    {
        var ticket = TicketFixtures.InCrossReview(
            id: 1,
            columnId: "CREATED",
            agentId: "DA",
            coAgentId: "DB",
            escalationTarget: "PM");

        var snapshot = BoardSnapshotFixtures.Build(
            columns: new[] { BoardColumnFixtures.Created },
            tickets: new[] { ticket },
            agents: new[] { AgentFixtures.DevA, AgentFixtures.DevB, AgentFixtures.Pm });

        snapshot.Tickets.Should().HaveCount(1);
        snapshot.Columns.Should().HaveCount(1);
        snapshot.Agents.Should().HaveCount(3);
    }

    [Fact]
    public void Should_BuildSnapshot_When_MultipleTicketsAcrossColumnsAndAgents()
    {
        var ticketOne = TicketFixtures.Open(id: 1, columnId: "CREATED", agentId: "DA");
        var ticketTwo = TicketFixtures.Open(id: 2, columnId: "IN_DEVELOPMENT", agentId: "DB");
        var ticketThree = TicketFixtures.Open(id: 3, columnId: "IN_QA", agentId: "PM");

        var snapshot = BoardSnapshotFixtures.Build(
            columns: new[]
            {
                BoardColumnFixtures.Created,
                BoardColumnFixtures.InDevelopment,
                BoardColumnFixtures.InQa,
            },
            tickets: new[] { ticketOne, ticketTwo, ticketThree },
            agents: new[] { AgentFixtures.DevA, AgentFixtures.DevB, AgentFixtures.Pm });

        int[] expectedTicketIds = [1, 2, 3];
        string[] expectedColumnIds = ["CREATED", "IN_DEVELOPMENT", "IN_QA"];
        string[] expectedAgentIds = ["DA", "DB", "PM"];

        snapshot.Tickets.Select(t => t.Id.Value).Should().BeEquivalentTo(expectedTicketIds);
        snapshot.Columns.Select(c => c.Id.Value).Should().BeEquivalentTo(expectedColumnIds);
        snapshot.Agents.Select(a => a.Id.Value).Should().BeEquivalentTo(expectedAgentIds);
    }
}
