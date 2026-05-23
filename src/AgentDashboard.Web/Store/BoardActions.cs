using AgentDashboard.TicketTracking.Application.Queries.GetBoard.Dtos;
using Blazor.Redux.Interfaces;

namespace AgentDashboard.Web.Store;

/// <summary>
/// Board actions for Blazor.Redux.
/// </summary>
public sealed record LoadBoardAction : IAction;

public sealed record LoadBoardSuccessAction(BoardDto Board) : IAction;

public sealed record LoadBoardFailureAction(string Error) : IAction;
