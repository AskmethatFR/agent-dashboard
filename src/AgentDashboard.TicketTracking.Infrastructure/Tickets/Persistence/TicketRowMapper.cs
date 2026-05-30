namespace AgentDashboard.TicketTracking.Infrastructure.Tickets.Persistence;

using AgentDashboard.TicketTracking.Domain.Tickets;

/// <summary>
/// Trivial flat-to-flat copy between the Domain <see cref="TicketPersistenceSnapshot"/>
/// and the EF <see cref="TicketRow"/> POCO. Never touches a value object.
/// </summary>
internal static class TicketRowMapper
{
    public static TicketRow ToRow(TicketPersistenceSnapshot snapshot) => new()
    {
        Repo = snapshot.Repo,
        GitHubIssueNumber = snapshot.GitHubIssueNumber,
        Title = snapshot.Title,
        Status = snapshot.Status,
        Agent = snapshot.Agent,
        RetryCount = snapshot.RetryCount,
        GitHubUrl = snapshot.GitHubUrl,
        CreatedAtUtc = snapshot.CreatedAtUtc,
        UpdatedAtUtc = snapshot.UpdatedAtUtc,
        ClosedAtUtc = snapshot.ClosedAtUtc,
    };
}
