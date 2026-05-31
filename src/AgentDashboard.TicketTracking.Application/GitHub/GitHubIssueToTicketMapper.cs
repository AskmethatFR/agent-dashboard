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
    /// <returns>The mapped ticket plus any label-mapping warnings.</returns>
    public static MappingResult Map(GitHubIssueRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var recognizedStatusLabels = FindRecognizedStatusLabels(record.Labels);
        var selectedStatusLabel = SelectLatestStatusLabel(recognizedStatusLabels);
        var agentLabel = FindFirstAgentLabel(record.Labels);
        var retryCount = FindHighestRetryCount(record.Labels);

        var warnings = new List<MappingWarning>();
        if (recognizedStatusLabels.Count > 1)
        {
            warnings.Add(MappingWarning.MultipleStatusLabels(
                record.Number, recognizedStatusLabels, selectedStatusLabel!));
        }
        else if (recognizedStatusLabels.Count == 0)
        {
            warnings.Add(MappingWarning.MissingStatusLabel(record.Number));
        }

        var ticketTitle = new TicketTitle(record.Title);
        var ticketStatus = selectedStatusLabel is not null && TryParseStatusValue(selectedStatusLabel, out var selectedValue)
            ? new TicketStatus(selectedValue)
            : new TicketStatus(TicketStatusValue.Created);

        var agentId = agentLabel is not null ? new AgentId(agentLabel) : null;
        var retry = new Retry(retryCount);
        var gitHubUrl = new GitHubUrl(record.HtmlUrl);
        var createdAt = new TimestampUtc(record.CreatedAt);
        var updatedAt = new TimestampUtc(record.UpdatedAt);
        var closedAt = record.ClosedAt is not null ? new TimestampUtc(record.ClosedAt.Value) : null;

        var ticket = new Ticket(
            new GitHubIssueNumber(record.Number),
            ticketTitle,
            ticketStatus,
            agentId,
            retry,
            gitHubUrl,
            createdAt,
            updatedAt,
            closedAt);

        return new MappingResult(ticket, warnings);
    }

    private static List<string> FindRecognizedStatusLabels(IReadOnlyList<string> labels) =>
        labels
            .Where(l => l.StartsWith("status:", StringComparison.OrdinalIgnoreCase))
            .Where(IsRecognizedStatusLabel)
            .ToList();

    private static bool IsRecognizedStatusLabel(string label) =>
        TryParseStatusValue(label, out _);

    private static bool TryParseStatusValue(string label, out TicketStatusValue value)
    {
        var cleanValue = label
            .Replace("status:", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("-", string.Empty, StringComparison.Ordinal);
        return Enum.TryParse(cleanValue, true, out value);
    }

    private static string? SelectLatestStatusLabel(IReadOnlyList<string> recognizedStatusLabels)
    {
        string? latest = null;
        int latestIndex = -1;

        foreach (var label in recognizedStatusLabels)
        {
            if (TryParseStatusValue(label, out var statusValue))
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
