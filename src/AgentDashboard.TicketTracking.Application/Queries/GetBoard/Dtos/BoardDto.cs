namespace AgentDashboard.TicketTracking.Application.Queries.GetBoard.Dtos;

public sealed record BoardDto(IReadOnlyList<BoardColumnDto> Columns);
