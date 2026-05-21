using AgentDashboard.TicketTracking.Application.Queries;
using AgentDashboard.TicketTracking.Application.Queries.Dtos;
using AgentDashboard.TicketTracking.Application.UnitTests.Queries.Stubs;
using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Boards;
using AgentDashboard.TicketTracking.Domain.Tickets;

namespace AgentDashboard.TicketTracking.Application.UnitTests.Queries;

public sealed class GetBoardQueryHandlerShould
{
    private static BoardColumn CreatedColumn() =>
        new(new BoardColumnId("CREATED"), new BoardColumnLabel("Created"));

    private static BoardColumn DevelopmentColumn() =>
        new(new BoardColumnId("IN_DEVELOPMENT"), new BoardColumnLabel("In Development"));

    private static BoardColumn QaColumn() =>
        new(new BoardColumnId("IN_QA"), new BoardColumnLabel("In QA"));

    private static Agent DevA() => new(
        new AgentId("DA"),
        new AgentName("DevA"),
        new AgentGlyph("Da"),
        new AgentRole("Developer A"));

    private static Ticket OpenTicketOn(string columnId, int id, string title) =>
        Ticket.Open(
            new TicketId(id),
            new BoardColumnId(columnId),
            new TicketTitle(title),
            new AgentId("DA"),
            new Retry(0),
            new Age(TimeSpan.Zero),
            thinking: false,
            TicketFreshness.Neutral);

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
        var snapshot = new BoardSnapshot(
            Array.Empty<BoardColumn>(),
            Array.Empty<Ticket>(),
            Array.Empty<Agent>());
        var handler = new GetBoardQueryHandler(new InMemoryBoardReader(snapshot));

        var dto = await handler.Handle(new GetBoardQuery(), CancellationToken.None);

        dto.Columns.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_PreserveColumnOrder_When_MappingMultipleColumns()
    {
        var snapshot = new BoardSnapshot(
            new[] { CreatedColumn(), DevelopmentColumn(), QaColumn() },
            Array.Empty<Ticket>(),
            Array.Empty<Agent>());
        var handler = new GetBoardQueryHandler(new InMemoryBoardReader(snapshot));

        var dto = await handler.Handle(new GetBoardQuery(), CancellationToken.None);

        dto.Columns.Select(c => c.Label).Should().Equal("Created", "In Development", "In QA");
    }

    [Fact]
    public async Task Should_EmitEmptyTicketList_When_ColumnHasNoTickets()
    {
        var snapshot = new BoardSnapshot(
            new[] { CreatedColumn(), DevelopmentColumn() },
            new[] { OpenTicketOn("CREATED", 1, "alpha") },
            new[] { DevA() });
        var handler = new GetBoardQueryHandler(new InMemoryBoardReader(snapshot));

        var dto = await handler.Handle(new GetBoardQuery(), CancellationToken.None);

        dto.Columns[0].Tickets.Should().HaveCount(1);
        dto.Columns[1].Tickets.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Should_GroupTicketsByColumnId_When_MultipleColumnsAndTickets()
    {
        var snapshot = new BoardSnapshot(
            new[] { CreatedColumn(), DevelopmentColumn() },
            new[]
            {
                OpenTicketOn("CREATED", 1, "alpha"),
                OpenTicketOn("IN_DEVELOPMENT", 2, "beta"),
                OpenTicketOn("CREATED", 3, "gamma"),
            },
            new[] { DevA() });
        var handler = new GetBoardQueryHandler(new InMemoryBoardReader(snapshot));

        var dto = await handler.Handle(new GetBoardQuery(), CancellationToken.None);

        string[] expectedCreatedTitles = ["alpha", "gamma"];
        dto.Columns[0].Tickets.Select(t => t.Title).Should().BeEquivalentTo(expectedCreatedTitles);
        dto.Columns[1].Tickets.Select(t => t.Title).Should().Equal("beta");
    }

    [Fact]
    public async Task Should_ProjectOnlyTitleValue_When_MappingTicket()
    {
        var snapshot = new BoardSnapshot(
            new[] { CreatedColumn() },
            new[] { OpenTicketOn("CREATED", 1, "hello") },
            new[] { DevA() });
        var handler = new GetBoardQueryHandler(new InMemoryBoardReader(snapshot));

        var dto = await handler.Handle(new GetBoardQuery(), CancellationToken.None);

        dto.Columns.Single().Tickets.Single().Should().BeEquivalentTo(new TicketDto("hello"));
    }

    [Fact]
    public async Task Should_ProjectOnlyLabelValue_When_MappingColumn()
    {
        var snapshot = new BoardSnapshot(
            new[] { CreatedColumn() },
            Array.Empty<Ticket>(),
            Array.Empty<Agent>());
        var handler = new GetBoardQueryHandler(new InMemoryBoardReader(snapshot));

        var dto = await handler.Handle(new GetBoardQuery(), CancellationToken.None);

        dto.Columns.Single().Should().BeEquivalentTo(
            new BoardColumnDto("Created", Array.Empty<TicketDto>()));
    }
}
