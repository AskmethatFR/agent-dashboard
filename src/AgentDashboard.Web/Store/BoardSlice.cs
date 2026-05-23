using AgentDashboard.TicketTracking.Application.Queries.GetBoard.Dtos;
using Blazor.Redux.Interfaces;

namespace AgentDashboard.Web.Store;

public sealed record BoardSlice(BoardDto? Board, bool IsLoading, string? Error) : ISlice
{
    public static BoardSlice Initial => new(null, false, null);
}
