using AgentDashboard.TicketTracking.Application.Queries.GetBoard;
using AgentDashboard.TicketTracking.Application.Queries.GetBoard.Dtos;
using AgentDashboard.TicketTracking.Application.UnitTests.Stubs;
using AgentDashboard.TicketTracking.Domain.Tickets;
using AgentDashboard.TicketTracking.TestShared.Agents;
using AgentDashboard.TicketTracking.TestShared.Boards;
using AgentDashboard.TicketTracking.TestShared.Tickets;

namespace AgentDashboard.TicketTracking.Application.UnitTests.Queries.GetBoard;

public sealed class GetBoardQueryHandlerShould
{
    [Fact]
    public void Should_Throw_ArgumentNullException_When_BoardReaderIsNull()
    {
        var act = () => new GetBoardQueryHandler(null!);

        act.Should().ThrowExactly<ArgumentNullException>()
            .WithParameterName("boardReader");
    }

    [Fact]
    public async Task Should_ReturnEmptyDto_When_SnapshotHasNoColumns()
    {
        var handler = new GetBoardQueryHandler(new InMemoryBoardReader(BoardSnapshotFixtures.Empty));

        var dto = await handler.Handle(new GetBoardQuery(), CancellationToken.None);

        dto.Columns.Should().BeEmpty();
        dto.Agents.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_PreserveColumnOrder_When_MappingMultipleColumns()
    {
        var snapshot = BoardSnapshotFixtures.Build(
            columns: new[]
            {
                BoardColumnFixtures.Created,
                BoardColumnFixtures.InDevelopment,
                BoardColumnFixtures.InQa,
            });
        var handler = new GetBoardQueryHandler(new InMemoryBoardReader(snapshot));

        var dto = await handler.Handle(new GetBoardQuery(), CancellationToken.None);

        dto.Columns.Select(c => c.Label).Should().Equal("Created", "In Development", "In QA");
    }

    [Fact]
    public async Task Should_EmitEmptyTicketList_When_ColumnHasNoTickets()
    {
        var snapshot = BoardSnapshotFixtures.Build(
            columns: new[] { BoardColumnFixtures.Created, BoardColumnFixtures.InDevelopment },
            tickets: new[] { TicketFixtures.Open(id: 1, columnId: "CREATED", title: "alpha") },
            agents: new[] { AgentFixtures.DevA });
        var handler = new GetBoardQueryHandler(new InMemoryBoardReader(snapshot));

        var dto = await handler.Handle(new GetBoardQuery(), CancellationToken.None);

        dto.Columns[0].Tickets.Should().HaveCount(1);
        dto.Columns[1].Tickets.Should().NotBeNull().And.BeEmpty();
        dto.Agents.Should().ContainSingle();
    }

    [Fact]
    public async Task Should_GroupTicketsByColumnId_When_MultipleColumnsAndTickets()
    {
        var snapshot = BoardSnapshotFixtures.Build(
            columns: new[] { BoardColumnFixtures.Created, BoardColumnFixtures.InDevelopment },
            tickets: new[]
            {
                TicketFixtures.Open(id: 1, columnId: "CREATED", title: "alpha"),
                TicketFixtures.Open(id: 2, columnId: "IN_DEVELOPMENT", title: "beta"),
                TicketFixtures.Open(id: 3, columnId: "CREATED", title: "gamma"),
            },
            agents: new[] { AgentFixtures.DevA });
        var handler = new GetBoardQueryHandler(new InMemoryBoardReader(snapshot));

        var dto = await handler.Handle(new GetBoardQuery(), CancellationToken.None);

        string[] expectedCreatedTitles = ["alpha", "gamma"];
        dto.Columns[0].Tickets.Select(t => t.Title).Should().Equal(expectedCreatedTitles);
        dto.Columns[1].Tickets.Select(t => t.Title).Should().Equal("beta");
        dto.Agents.Should().ContainSingle();
    }

    [Fact]
    public async Task Should_ProjectTitleAndAgentIdAndIsThinking_When_MappingTicket()
    {
        var snapshot = BoardSnapshotFixtures.Build(
            columns: new[] { BoardColumnFixtures.Created },
            tickets: new[] { TicketFixtures.Open(id: 1, columnId: "CREATED", title: "hello") },
            agents: new[] { AgentFixtures.DevA });
        var handler = new GetBoardQueryHandler(new InMemoryBoardReader(snapshot));

        var dto = await handler.Handle(new GetBoardQuery(), CancellationToken.None);

        var ticketDto = dto.Columns.Single().Tickets.Single();
        ticketDto.Title.Should().Be("hello");
        ticketDto.AgentId.Should().Be("DA");
        ticketDto.IsThinking.Should().Be(false);
    }

    [Fact]
    public async Task Should_MapAllTicketFieldsToDto_When_TicketHasAllProperties()
    {
        // Test List:
        // Happy path: all fields mapped correctly
        // Boundaries: default values (0, false, null), non-default values
        // Invalid: N/A for mapping
        var snapshot = BoardSnapshotFixtures.Build(
            columns: new[] { BoardColumnFixtures.Created },
            tickets: new[] { TicketFixtures.Open(id: 42, columnId: "CREATED", title: "test ticket", agentId: "AGENT1", retry: 2, age: TimeSpan.FromHours(2), thinking: true, freshness: TicketFreshness.Fresh) },
            agents: new[] { AgentFixtures.Build("AGENT1", "Agent 1", "A1", "Dev") });
        var handler = new GetBoardQueryHandler(new InMemoryBoardReader(snapshot));

        var dto = await handler.Handle(new GetBoardQuery(), CancellationToken.None);

        var ticketDto = dto.Columns.Single().Tickets.Single();
        ticketDto.Id.Should().Be(42);
        ticketDto.Title.Should().Be("test ticket");
        ticketDto.AgentId.Should().Be("AGENT1");
        ticketDto.RetryCount.Should().Be(2);
        ticketDto.Age.Should().Be(TimeSpan.FromHours(2));
        ticketDto.Freshness.Should().Be("Fresh");
        ticketDto.IsThinking.Should().Be(true);
        ticketDto.IsEscalated.Should().Be(false);
        ticketDto.EscalationTargetId.Should().BeNull();
        ticketDto.IsInCrossReview.Should().Be(false);
        ticketDto.CoAgentId.Should().BeNull();
    }

    [Fact]
    public async Task Should_MapEscalatedTicketFields_When_TicketIsEscalated()
    {
        // Test List:
        // Happy path: escalated ticket with co-agent
        // Boundaries: non-null EscalationTarget, non-null CoAgentId
        var snapshot = BoardSnapshotFixtures.Build(
            columns: new[] { BoardColumnFixtures.Created },
            tickets: new[] { TicketFixtures.Escalated(id: 99, columnId: "CREATED", title: "escalated", agentId: "DA", escalationTarget: "PM", retry: 3, age: TimeSpan.FromHours(4), thinking: false, freshness: TicketFreshness.Stale, coAgentId: "DB") },
            agents: new[] { AgentFixtures.DevA, AgentFixtures.DevB, AgentFixtures.Pm });
        var handler = new GetBoardQueryHandler(new InMemoryBoardReader(snapshot));

        var dto = await handler.Handle(new GetBoardQuery(), CancellationToken.None);

        var ticketDto = dto.Columns.Single().Tickets.Single();
        ticketDto.Id.Should().Be(99);
        ticketDto.Title.Should().Be("escalated");
        ticketDto.AgentId.Should().Be("DA");
        ticketDto.RetryCount.Should().Be(3);
        ticketDto.Age.Should().Be(TimeSpan.FromHours(4));
        ticketDto.Freshness.Should().Be("Stale");
        ticketDto.IsThinking.Should().Be(false);
        ticketDto.IsEscalated.Should().Be(true);
        ticketDto.EscalationTargetId.Should().Be("PM");
        ticketDto.IsInCrossReview.Should().Be(true);
        ticketDto.CoAgentId.Should().Be("DB");
    }

    [Fact]
    public async Task Should_MapCrossReviewTicketFields_When_TicketIsInCrossReview()
    {
        // Test List:
        // Happy path: cross-review ticket without escalation
        var snapshot = BoardSnapshotFixtures.Build(
            columns: new[] { BoardColumnFixtures.Created },
            tickets: new[] { TicketFixtures.InCrossReview(id: 77, columnId: "CREATED", title: "review me", agentId: "DA", coAgentId: "DB", retry: 1, age: TimeSpan.FromMinutes(30), thinking: true, freshness: TicketFreshness.Fresh) },
            agents: new[] { AgentFixtures.DevA, AgentFixtures.DevB });
        var handler = new GetBoardQueryHandler(new InMemoryBoardReader(snapshot));

        var dto = await handler.Handle(new GetBoardQuery(), CancellationToken.None);

        var ticketDto = dto.Columns.Single().Tickets.Single();
        ticketDto.Id.Should().Be(77);
        ticketDto.Title.Should().Be("review me");
        ticketDto.AgentId.Should().Be("DA");
        ticketDto.RetryCount.Should().Be(1);
        ticketDto.Age.Should().Be(TimeSpan.FromMinutes(30));
        ticketDto.Freshness.Should().Be("Fresh");
        ticketDto.IsThinking.Should().Be(true);
        ticketDto.IsEscalated.Should().Be(false);
        ticketDto.EscalationTargetId.Should().BeNull();
        ticketDto.IsInCrossReview.Should().Be(true);
        ticketDto.CoAgentId.Should().Be("DB");
    }

    [Fact]
    public async Task Should_MapAgents_When_SnapshotContainsAgents()
    {
        var snapshot = BoardSnapshotFixtures.Build(
            columns: new[] { BoardColumnFixtures.Created },
            tickets: Array.Empty<Ticket>(),
            agents: new[] { AgentFixtures.DevA, AgentFixtures.DevB });
        var handler = new GetBoardQueryHandler(new InMemoryBoardReader(snapshot));

        var dto = await handler.Handle(new GetBoardQuery(), CancellationToken.None);

        dto.Agents.Should().BeEquivalentTo(new[]
        {
            new AgentDto("DA", "DevA", "Da", "Developer A"),
            new AgentDto("DB", "DevB", "Db", "Developer B"),
        });
    }

    [Fact]
    public async Task Should_ProjectOnlyLabelValue_When_MappingColumn()
    {
        var snapshot = BoardSnapshotFixtures.Build(
            columns: new[] { BoardColumnFixtures.Created });
        var handler = new GetBoardQueryHandler(new InMemoryBoardReader(snapshot));

        var dto = await handler.Handle(new GetBoardQuery(), CancellationToken.None);

        dto.Columns.Single().Should().BeEquivalentTo(
            new BoardColumnDto("Created", Array.Empty<TicketDto>()));
    }
}
