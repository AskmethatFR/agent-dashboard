namespace AgentDashboard.TicketTracking.Domain.Tickets;

using AgentDashboard.TicketTracking.Domain.Agents;

/// <summary>
/// A projection entity representing a Ticket in the TicketTracking bounded context.
/// This is a downstream Conformist read model of GitHub Issues.
/// Identity is the composite key (GitHubRepository, GitHubIssueNumber).
/// </summary>
public sealed class Ticket
{
    /// <summary>
    /// The repository source where this ticket originates from.
    /// </summary>
    public GitHubRepository GitHubRepository { get; }

    /// <summary>
    /// The GitHub issue number.
    /// </summary>
    public GitHubIssueNumber GitHubIssueNumber { get; }

    /// <summary>
    /// The title of the ticket.
    /// </summary>
    public TicketTitle TicketTitle { get; }

    /// <summary>
    /// The current status of the ticket.
    /// </summary>
    public TicketStatus TicketStatus { get; }

    /// <summary>
    /// The agent ID currently assigned to this ticket, or null if not assigned.
    /// </summary>
    public AgentId? AgentId { get; }

    /// <summary>
    /// The retry count (0-3) for this ticket.
    /// </summary>
    public Retry RetryCount { get; }

    /// <summary>
    /// The GitHub URL for this ticket.
    /// </summary>
    public GitHubUrl GitHubUrl { get; }

    /// <summary>
    /// The timestamp when the ticket was created in UTC.
    /// </summary>
    public TimestampUtc CreatedAtUtc { get; }

    /// <summary>
    /// The timestamp when the ticket was last updated in UTC.
    /// </summary>
    public TimestampUtc UpdatedAtUtc { get; }

    /// <summary>
    /// The timestamp when the ticket was closed in UTC, or null if still open.
    /// </summary>
    public TimestampUtc? ClosedAtUtc { get; }

    /// <summary>
    /// Creates a new <see cref="Ticket"/> with the specified properties.
    /// </summary>
    public Ticket(
        GitHubRepository repositorySource,
        GitHubIssueNumber gitHubIssueNumber,
        TicketTitle ticketTitle,
        TicketStatus ticketStatus,
        AgentId? agentId,
        Retry retryCount,
        GitHubUrl gitHubUrl,
        TimestampUtc createdAtUtc,
        TimestampUtc updatedAtUtc,
        TimestampUtc? closedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(repositorySource);
        ArgumentNullException.ThrowIfNull(gitHubIssueNumber);
        ArgumentNullException.ThrowIfNull(ticketTitle);
        ArgumentNullException.ThrowIfNull(ticketStatus);
        ArgumentNullException.ThrowIfNull(retryCount);
        ArgumentNullException.ThrowIfNull(gitHubUrl);
        ArgumentNullException.ThrowIfNull(createdAtUtc);
        ArgumentNullException.ThrowIfNull(updatedAtUtc);

        GitHubRepository = repositorySource;
        GitHubIssueNumber = gitHubIssueNumber;
        TicketTitle = ticketTitle;
        TicketStatus = ticketStatus;
        AgentId = agentId;
        RetryCount = retryCount;
        GitHubUrl = gitHubUrl;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        ClosedAtUtc = closedAtUtc;
    }

    /// <summary>
    /// Externalizes this ticket's state into an immutable, primitive-only snapshot
    /// for the Domain↔persistence boundary.
    /// </summary>
    public TicketPersistenceSnapshot ToSnapshot() => new(
        Repo: GitHubRepository.Value,
        GitHubIssueNumber: GitHubIssueNumber.Value,
        Title: TicketTitle.Value,
        Status: TicketStatus.Value.ToString(),
        Agent: AgentId?.Value,
        RetryCount: RetryCount.Value,
        GitHubUrl: GitHubUrl.Value,
        CreatedAtUtc: CreatedAtUtc.ToString(),
        UpdatedAtUtc: UpdatedAtUtc.ToString(),
        ClosedAtUtc: ClosedAtUtc?.ToString());

    /// <summary>
    /// Rehydrates a <see cref="Ticket"/> from a <see cref="TicketPersistenceSnapshot"/>.
    /// Inverse of <see cref="ToSnapshot"/>.
    /// </summary>
    public static Ticket FromSnapshot(TicketPersistenceSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return new Ticket(
            new GitHubRepository(snapshot.Repo),
            new GitHubIssueNumber(snapshot.GitHubIssueNumber),
            new TicketTitle(snapshot.Title),
            TicketStatus.Parse(snapshot.Status),
            snapshot.Agent is null ? null : new AgentId(snapshot.Agent),
            new Retry(snapshot.RetryCount),
            new GitHubUrl(snapshot.GitHubUrl),
            new TimestampUtc(ParseTimestamp(snapshot.CreatedAtUtc)),
            new TimestampUtc(ParseTimestamp(snapshot.UpdatedAtUtc)),
            snapshot.ClosedAtUtc is null ? null : new TimestampUtc(ParseTimestamp(snapshot.ClosedAtUtc)));
    }

    private static DateTimeOffset ParseTimestamp(string value) =>
        DateTimeOffset.Parse(
            value,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.RoundtripKind);

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// Equality is based on the composite key (GitHubRepository, GitHubIssueNumber).
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is Ticket other &&
               EqualityComparer<GitHubRepository>.Default.Equals(GitHubRepository, other.GitHubRepository) &&
               EqualityComparer<GitHubIssueNumber>.Default.Equals(GitHubIssueNumber, other.GitHubIssueNumber);
    }

    public static bool operator ==(Ticket left, Ticket right)
    {
        if (left is null ^ right is null)
        {
            return false;
        }
        return left?.Equals(right) != false;
    }

    public static bool operator !=(Ticket left, Ticket right) => !(left == right);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    public override int GetHashCode()
    {
        unchecked
        {
            return (EqualityComparer<GitHubRepository>.Default.GetHashCode(GitHubRepository) * 397) ^
                   EqualityComparer<GitHubIssueNumber>.Default.GetHashCode(GitHubIssueNumber);
        }
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    public override string ToString() => $"Ticket({GitHubRepository.Value}/{GitHubIssueNumber.Value})";
}
