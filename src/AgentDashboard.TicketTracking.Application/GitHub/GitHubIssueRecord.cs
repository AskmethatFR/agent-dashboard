namespace AgentDashboard.TicketTracking.Application.GitHub;

/// <summary>
/// A record representing a GitHub issue fetched from the API.
/// </summary>
/// <param name="Number">The issue number.</param>
/// <param name="Title">The issue title.</param>
/// <param name="Labels">The list of labels on the issue.</param>
/// <param name="CreatedAt">The timestamp when the issue was created.</param>
/// <param name="HtmlUrl">The HTML URL of the issue.</param>
/// <param name="UpdatedAt">The timestamp when the issue was last updated.</param>
/// <param name="ClosedAt">The timestamp when the issue was closed, or null if still open.</param>
public sealed record GitHubIssueRecord(
    long Number,
    string Title,
    IReadOnlyList<string> Labels,
    DateTimeOffset CreatedAt,
    string HtmlUrl,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ClosedAt);
