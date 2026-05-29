namespace AgentDashboard.TicketTracking.Application.GitHub;

using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Tickets;

/// <summary>
/// Maps GitHub issue records to Ticket domain entities.
/// Handles label conflict resolution according to the label taxonomy.
/// </summary>
public static class GitHubIssueToTicketMapper
{
    // State machine order from docs/labels.md
    private static readonly TicketStatusValue[] StateMachineOrder = 
    {
        TicketStatusValue.Created,
        TicketStatusValue.Specified,
        TicketStatusValue.InDevelopment,
        TicketStatusValue.InReview,
        TicketStatusValue.InQa,
        TicketStatusValue.AwaitingValidation,
        TicketStatusValue.Done,
        TicketStatusValue.Escalated
    };

    /// <summary>
    /// Maps a GitHub issue record to a Ticket entity.
    /// </summary>
    /// <param name="record">The GitHub issue record.</param>
    /// <param name="repositorySource">The repository source.</param>
    /// <returns>A Ticket entity.</returns>
    public static Ticket Map(GitHubIssueRecord record, RepositorySource repositorySource)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(repositorySource);

        // Extract labels
        var statusLabel = FindLatestStatusLabel(record.Labels);
        var agentLabel = FindFirstAgentLabel(record.Labels);
        var retryCount = FindHighestRetryCount(record.Labels);

        // Map to domain types
        var ticketTitle = new TicketTitle(record.Title);
        var ticketStatus = statusLabel is not null 
            ? TicketStatus.Parse(statusLabel)
            : new TicketStatus(TicketStatusValue.Created);
        
        var agentId = agentLabel is not null ? new AgentId(agentLabel) : null;
        var retry = new Retry(retryCount);
        var gitHubUrl = new GitHubUrl(record.HtmlUrl);
        var createdAt = new TimestampUtc(record.CreatedAt);
        var updatedAt = new TimestampUtc(record.UpdatedAt);
        var closedAt = record.ClosedAt is not null ? new TimestampUtc(record.ClosedAt.Value) : null;

        return new Ticket(
            repositorySource,
            new GitHubIssueNumber(record.Number),
            ticketTitle,
            ticketStatus,
            agentId,
            retry,
            gitHubUrl,
            createdAt,
            updatedAt,
            closedAt);
    }

    private static string? FindLatestStatusLabel(IReadOnlyList<string> labels)
    {
        var statusLabels = labels.Where(l => l.StartsWith("status:", StringComparison.OrdinalIgnoreCase)).ToList();
        
        if (statusLabels.Count == 0)
        {
            return null;
        }

        // Return the label with the highest state machine order
        string? latest = null;
        int latestIndex = -1;
        
        foreach (var label in statusLabels)
        {
            var cleanValue = label.Replace("status:", string.Empty, StringComparison.OrdinalIgnoreCase);
            if (Enum.TryParse<TicketStatusValue>(cleanValue, true, out var statusValue))
            {
                var index = Array.IndexOf(StateMachineOrder, statusValue);
                if (index > latestIndex)
                {
                    latestIndex = index;
                    latest = label;
                }
            }
        }

        return latest;
    }

    private static string? FindFirstAgentLabel(IReadOnlyList<string> labels)
    {
        var agentLabels = labels.Where(l => l.StartsWith("agent:", StringComparison.OrdinalIgnoreCase)).ToList();
        return agentLabels.FirstOrDefault();
    }

    private static int FindHighestRetryCount(IReadOnlyList<string> labels)
    {
        var retryLabels = labels.Where(l => l.StartsWith("retry:", StringComparison.OrdinalIgnoreCase)).ToList();
        
        if (retryLabels.Count == 0)
        {
            return 0;
        }

        int highest = 0;
        foreach (var label in retryLabels)
        {
            var cleanValue = label.Replace("retry:", string.Empty, StringComparison.OrdinalIgnoreCase);
            if (int.TryParse(cleanValue, out var value) && value > highest)
            {
                highest = value;
            }
        }

        return highest;
    }
}
