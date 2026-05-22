namespace AgentDashboard.TicketTracking.Application.Queries.GetBoard.Dtos;

public sealed record BoardColumnDto(string Label, IReadOnlyList<TicketDto> Tickets);
