using AgentDashboard.TicketTracking.Application.Queries.Dtos;
using MediatR;

namespace AgentDashboard.TicketTracking.Application.Queries;

public sealed record GetBoardQuery : IRequest<BoardDto>;
