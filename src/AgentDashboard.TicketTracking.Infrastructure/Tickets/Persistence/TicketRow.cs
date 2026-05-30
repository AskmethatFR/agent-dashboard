namespace AgentDashboard.TicketTracking.Infrastructure.Tickets.Persistence;

/// <summary>
/// EF Core persistence POCO for a row in the <c>tickets</c> table.
/// Mutable by design so the EF change tracker can update existing rows in place.
/// Not a domain type — it carries no behavior and no invariants.
/// </summary>
internal sealed class TicketRow
{
    public string Repo { get; set; } = string.Empty;
    public long GitHubIssueNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Agent { get; set; }
    public int RetryCount { get; set; }
    public string GitHubUrl { get; set; } = string.Empty;
    public string CreatedAtUtc { get; set; } = string.Empty;
    public string UpdatedAtUtc { get; set; } = string.Empty;
    public string? ClosedAtUtc { get; set; }
}
