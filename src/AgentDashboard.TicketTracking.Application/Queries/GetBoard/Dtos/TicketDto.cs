namespace AgentDashboard.TicketTracking.Application.Queries.GetBoard.Dtos;

public sealed record TicketDto(
    string Title,
    string AgentId = "",
    bool IsThinking = false,
    int RetryCount = 0);
