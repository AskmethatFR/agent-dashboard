namespace AgentDashboard.TicketTracking.Application.Queries.Dtos;

public sealed record BoardColumnDto(string Label, IReadOnlyList<TicketDto> Tickets);
