using AgentDashboard.TicketTracking.Application.Queries.GetBoard.Dtos;
using Cortex.Mediator.Queries;

namespace AgentDashboard.TicketTracking.Application.Queries.GetBoard;

public sealed record GetBoardQuery : IQuery<BoardDto>;
