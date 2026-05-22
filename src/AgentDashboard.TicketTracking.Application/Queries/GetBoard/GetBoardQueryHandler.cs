using AgentDashboard.TicketTracking.Application.Ports;
using AgentDashboard.TicketTracking.Application.Queries.GetBoard.Dtos;
using AgentDashboard.TicketTracking.Domain.Boards;
using Cortex.Mediator.Queries;

namespace AgentDashboard.TicketTracking.Application.Queries.GetBoard;

public sealed class GetBoardQueryHandler : IQueryHandler<GetBoardQuery, BoardDto>
{
    private readonly IBoardReader _boardReader;

    public GetBoardQueryHandler(IBoardReader boardReader)
    {
        ArgumentNullException.ThrowIfNull(boardReader);
        _boardReader = boardReader;
    }

    public async Task<BoardDto> Handle(GetBoardQuery query, CancellationToken cancellationToken)
    {
        var snapshot = await _boardReader.GetCurrentAsync(cancellationToken).ConfigureAwait(false);
        return MapToDto(snapshot);
    }

    private static BoardDto MapToDto(BoardSnapshot snapshot)
    {
        var ticketsByColumn = snapshot.Tickets
            .GroupBy(t => t.ColumnId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var columns = snapshot.Columns
            .Select(column =>
            {
                var tickets = ticketsByColumn.TryGetValue(column.Id, out var list)
                    ? list.Select(t => new TicketDto(t.Title.Value)).ToList()
                    : new List<TicketDto>();
                return new BoardColumnDto(column.Label.Value, tickets);
            })
            .ToList();

        return new BoardDto(columns);
    }
}
