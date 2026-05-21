using AgentDashboard.TicketTracking.Application.Queries.Dtos;
using Cortex.Mediator.Queries;

namespace AgentDashboard.TicketTracking.Application.Queries;

public sealed record GetBoardQuery : IQuery<BoardDto>;
