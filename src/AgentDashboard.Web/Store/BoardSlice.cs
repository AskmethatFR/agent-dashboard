using AgentDashboard.TicketTracking.Application.Queries.GetBoard.Dtos;
using Blazor.Redux.Interfaces;

namespace AgentDashboard.Web.Store;

/// <summary>
/// Board state slice for Blazor.Redux.
/// </summary>
public sealed record BoardSlice(BoardDto? Board, bool IsLoading, string? Error) : ISlice
{
    public static BoardSlice Initial => new(null, false, null);
}
