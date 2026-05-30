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
