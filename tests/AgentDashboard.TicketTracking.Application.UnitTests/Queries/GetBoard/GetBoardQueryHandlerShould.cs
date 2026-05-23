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

        dto.Columns.Single().Tickets.Single().Should().BeEquivalentTo(
            new TicketDto("hello", "DA", false));
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
