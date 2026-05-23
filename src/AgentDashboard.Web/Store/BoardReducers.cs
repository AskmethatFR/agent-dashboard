using AgentDashboard.TicketTracking.Application.Queries.GetBoard;
using AgentDashboard.TicketTracking.Application.Queries.GetBoard.Dtos;
using Blazor.Redux.Interfaces;
using Cortex.Mediator;

namespace AgentDashboard.Web.Store;

public sealed class LoadBoardReducer : IReducer<BoardSlice, LoadBoardAction>
{
    public BoardSlice Reduce(BoardSlice slice, LoadBoardAction action)
    {
        return slice with { IsLoading = true, Error = null };
    }
}

public sealed class LoadBoardSuccessReducer : IReducer<BoardSlice, LoadBoardSuccessAction>
{
    public BoardSlice Reduce(BoardSlice slice, LoadBoardSuccessAction action)
    {
        return slice with { Board = action.Board, IsLoading = false };
    }
}

public sealed class LoadBoardFailureReducer : IReducer<BoardSlice, LoadBoardFailureAction>
{
    public BoardSlice Reduce(BoardSlice slice, LoadBoardFailureAction action)
    {
        return slice with { Error = action.Error, IsLoading = false };
    }
}

public sealed class LoadBoardAsyncReducer : IAsyncReducer<BoardSlice, LoadBoardAction>
{
    private readonly IMediator _mediator;

    public LoadBoardAsyncReducer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<BoardSlice> ReduceAsync(BoardSlice slice, LoadBoardAction action)
    {
        try
        {
            var board = await _mediator
                .SendQueryAsync<GetBoardQuery, BoardDto>(
                    new GetBoardQuery(),
                    CancellationToken.None)
                .ConfigureAwait(false);

            return slice with { Board = board, IsLoading = false, Error = null };
        }
        catch (Exception ex)
        {
            return slice with { Error = ex.Message, IsLoading = false };
        }
    }
}
