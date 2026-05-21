namespace AgentDashboard.TicketTracking.Application.Queries.Dtos;

public sealed record BoardDto(IReadOnlyList<BoardColumnDto> Columns);
