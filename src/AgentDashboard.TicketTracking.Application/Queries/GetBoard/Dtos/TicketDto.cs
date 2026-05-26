using System.Globalization;

namespace AgentDashboard.TicketTracking.Application.Queries.GetBoard.Dtos;

public sealed record TicketDto(
    string Title,
    string AgentId = "",
    bool IsThinking = false,
    int RetryCount = 0,
    int Id = 0,
    TimeSpan Age = default,
    string Freshness = "Neutral",
    bool IsEscalated = false,
    string? EscalationTargetId = null,
    bool IsInCrossReview = false,
    string? CoAgentId = null)
{
    public string AgeFormatted => Age.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
}
