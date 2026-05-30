namespace AgentDashboard.TicketTracking.Infrastructure.Tickets.Persistence;

using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Tickets;

/// <summary>
/// EF Core persistence POCO for a row in the <c>tickets</c> table, and the single
/// Infrastructure artifact that maps a domain <see cref="Ticket"/> to and from its
/// persisted shape. Mutable by design so the EF change tracker can update existing
/// rows in place. Carries no domain invariants.
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

    internal static TicketRow FromTicket(Ticket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);

        return new TicketRow
        {
            Repo = ticket.GitHubRepository.Value,
            GitHubIssueNumber = ticket.GitHubIssueNumber.Value,
            Title = ticket.TicketTitle.Value,
            Status = ticket.TicketStatus.Value.ToString(),
            Agent = ticket.AgentId?.Value,
            RetryCount = ticket.RetryCount.Value,
            GitHubUrl = ticket.GitHubUrl.Value,
            CreatedAtUtc = ticket.CreatedAtUtc.ToString(),
            UpdatedAtUtc = ticket.UpdatedAtUtc.ToString(),
            ClosedAtUtc = ticket.ClosedAtUtc?.ToString(),
        };
    }

    internal Ticket ToTicket() => new(
        new GitHubRepository(Repo),
        new GitHubIssueNumber(GitHubIssueNumber),
        new TicketTitle(Title),
        TicketStatus.Parse(Status),
        Agent is null ? null : new AgentId(Agent),
        new Retry(RetryCount),
        new GitHubUrl(GitHubUrl),
        new TimestampUtc(ParseTimestamp(CreatedAtUtc)),
        new TimestampUtc(ParseTimestamp(UpdatedAtUtc)),
        ClosedAtUtc is null ? null : new TimestampUtc(ParseTimestamp(ClosedAtUtc)));

    private static DateTimeOffset ParseTimestamp(string value) =>
        DateTimeOffset.Parse(
            value,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.RoundtripKind);
}
