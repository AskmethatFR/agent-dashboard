namespace AgentDashboard.TicketTracking.Application.GitHub;

public sealed record GitHubIssueRecord(
    long Number,
    string Title,
    IReadOnlyList<string> Labels,
    DateTimeOffset CreatedAt);
