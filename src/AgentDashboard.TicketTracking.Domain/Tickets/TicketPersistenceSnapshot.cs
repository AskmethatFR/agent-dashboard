namespace AgentDashboard.TicketTracking.Domain.Tickets;

/// <summary>
/// A frozen, primitive-only projection of a <see cref="Ticket"/>'s state for the
/// Domain↔persistence boundary (Snapshot pattern). Carries no identity, no lifecycle,
/// and no transactional boundary — it is pure data, never an entity or aggregate.
/// </summary>
public sealed record TicketPersistenceSnapshot(
    string Repo,
    long GitHubIssueNumber,
    string Title,
    string Status,
    string? Agent,
    int RetryCount,
    string GitHubUrl,
    string CreatedAtUtc,
    string UpdatedAtUtc,
    string? ClosedAtUtc);
