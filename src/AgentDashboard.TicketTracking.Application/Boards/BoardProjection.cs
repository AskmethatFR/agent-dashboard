using AgentDashboard.TicketTracking.Application.GitHub;
using AgentDashboard.TicketTracking.Application.Ports;
using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Boards;
using AgentDashboard.TicketTracking.Domain.Tickets;

namespace AgentDashboard.TicketTracking.Application.Boards;

public sealed class BoardProjection : IBoardProjection
{
    private const string StatusPrefix = "status:";
    private const string AgentPrefix = "agent:";
    private const string RetryPrefix = "retry:";
    private const string CoAgentPrefix = "co-agent:";
    private const string EscalationTargetPrefix = "escalation-target:";

    private const string ColumnCreated = "CREATED";
    private const string ColumnSpecified = "SPECIFIED";
    private const string ColumnInDevelopment = "IN_DEVELOPMENT";
    private const string ColumnInReview = "IN_REVIEW";
    private const string ColumnInQa = "IN_QA";
    private const string ColumnAwaitingValidation = "AWAITING_VALIDATION";
    private const string ColumnDone = "DONE";

    private const string StatusCreated = "status:created";
    private const string StatusSpecified = "status:specified";
    private const string StatusInDevelopment = "status:in-development";
    private const string StatusInReview = "status:in-review";
    private const string StatusInQa = "status:in-qa";
    private const string StatusAwaitingValidation = "status:awaiting-validation";
    private const string StatusDone = "status:done";
    private const string StatusEscalated = "status:escalated";

    private const string DefaultAgentId = "pm";

    private static readonly IReadOnlyList<BoardColumn> Columns = new List<BoardColumn>
    {
        new BoardColumn(new BoardColumnId(ColumnCreated), new BoardColumnLabel("Created")),
        new BoardColumn(new BoardColumnId(ColumnSpecified), new BoardColumnLabel("Specified")),
        new BoardColumn(new BoardColumnId(ColumnInDevelopment), new BoardColumnLabel("In Development")),
        new BoardColumn(new BoardColumnId(ColumnInReview), new BoardColumnLabel("In Review")),
        new BoardColumn(new BoardColumnId(ColumnInQa), new BoardColumnLabel("In Qa")),
        new BoardColumn(new BoardColumnId(ColumnAwaitingValidation), new BoardColumnLabel("Awaiting Validation")),
        new BoardColumn(new BoardColumnId(ColumnDone), new BoardColumnLabel("Done"))
    };

    private static readonly IReadOnlyList<Agent> Agents = new List<Agent>
    {
        new Agent(new AgentId("pm"), new AgentName("Project Manager"), new AgentGlyph("PM"), new AgentRole("project-manager")),
        new Agent(new AgentId("architect"), new AgentName("Project Architect"), new AgentGlyph("AR"), new AgentRole("architect")),
        new Agent(new AgentId("dev-a"), new AgentName("Developer A"), new AgentGlyph("DA"), new AgentRole("developer")),
        new Agent(new AgentId("dev-b"), new AgentName("Developer B"), new AgentGlyph("DB"), new AgentRole("developer")),
        new Agent(new AgentId("qa"), new AgentName("QA"), new AgentGlyph("QA"), new AgentRole("qa")),
        new Agent(new AgentId("security"), new AgentName("Security"), new AgentGlyph("SC"), new AgentRole("security"))
    };

    private static readonly HashSet<string> ValidLabelPrefixes = new(
        StringComparer.Ordinal)
    {
        StatusPrefix,
        AgentPrefix,
        RetryPrefix,
        CoAgentPrefix,
        EscalationTargetPrefix
    };

    private static readonly HashSet<string> ValidStatusValues = new(
        StringComparer.Ordinal)
    {
        "created",
        "specified",
        "in-development",
        "in-review",
        "in-qa",
        "awaiting-validation",
        "done",
        "escalated"
    };

    private static readonly HashSet<string> ValidAgentValues = new(
        StringComparer.Ordinal)
    {
        "pm",
        "architect",
        "dev-a",
        "dev-b",
        "qa",
        "security"
    };

    public BoardSnapshot Project(
        IReadOnlyList<GitHubIssueRecord> records,
        DateTimeOffset asOf)
    {
        // Validate all labels in all records before processing
        foreach (var record in records)
        {
            ValidateLabels(record.Labels);
        }

        var tickets = records.Select(r => MapToTicket(r, asOf)).ToList();
        return new BoardSnapshot(Columns, tickets, Agents);
    }

    private static void ValidateLabels(IReadOnlyList<string> labels)
    {
        foreach (var label in labels)
        {
            ValidateLabelFormat(label);
        }
    }

    private static void ValidateLabelFormat(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new InvalidOperationException(
                $"GitHub label is null, empty, or whitespace. All labels must have a valid format.");
        }

        // Check if the label has a valid prefix
        var hasValidPrefix = false;
        foreach (var prefix in ValidLabelPrefixes)
        {
            if (label.StartsWith(prefix, StringComparison.Ordinal))
            {
                hasValidPrefix = true;
                var value = label[prefix.Length..];

                // Validate the value based on the prefix
                ValidateLabelValue(prefix, value);
                break;
            }
        }

        if (!hasValidPrefix)
        {
            // Label doesn't have a recognized prefix - this might be okay (e.g., "status:in-review")
            // but we should at least check it's not malicious
            if (label.Contains(':'))
            {
                // Has a colon but not a recognized prefix - validate the format
                var parts = label.Split(':', 2);
                if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
                {
                    throw new InvalidOperationException(
                        $"GitHub label '{label}' has invalid format. Labels must be in the format 'prefix:value'.");
                }
            }
        }
    }

    private static void ValidateLabelValue(string prefix, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"GitHub label with prefix '{prefix}' has empty or whitespace value.");
        }

        // Validate based on prefix
        switch (prefix)
        {
            case StatusPrefix:
                if (!ValidStatusValues.Contains(value))
                {
                    throw new InvalidOperationException(
                        $"Invalid status value '{value}'. Valid values are: {string.Join(", ", ValidStatusValues)}.");
                }
                break;

            case AgentPrefix:
            case CoAgentPrefix:
            case EscalationTargetPrefix:
                if (!ValidAgentValues.Contains(value))
                {
                    throw new InvalidOperationException(
                        $"Invalid agent value '{value}'. Valid values are: {string.Join(", ", ValidAgentValues)}.");
                }
                break;

            case RetryPrefix:
                if (!int.TryParse(value, out _))
                {
                    throw new InvalidOperationException(
                        $"Invalid retry value '{value}'. Retry must be a valid integer.");
                }
                break;
        }
    }

    private static TicketSnapshot MapToTicket(GitHubIssueRecord record, DateTimeOffset asOf)
    {
        var columnId = MapStatusLabel(record.Labels);
        var agentId = MapAgentLabel(record.Labels);
        var retry = MapRetryLabel(record.Labels);
        var coAgentId = MapCoAgentLabel(record.Labels);
        var escalationTarget = MapEscalationTargetLabel(record.Labels);
        var hasInReviewStatus = record.Labels.Contains(StatusInReview);
        var hasEscalatedStatus = record.Labels.Contains(StatusEscalated);
        var isEscalated = hasEscalatedStatus && retry.Value >= 3;

        var age = new Age(asOf - record.CreatedAt);
        var freshness = CalculateFreshness(record.Labels, columnId.Value, age);

        if (hasInReviewStatus)
        {
            // InCrossReview requires non-null coAgentId, so use agentId if coAgentId is null
            return TicketSnapshot.InCrossReview(
                id: new TicketId((int)record.Number),
                columnId: columnId,
                title: new TicketTitle(record.Title),
                agentId: agentId,
                coAgentId: coAgentId ?? agentId,
                retry: retry,
                age: age,
                thinking: false,
                freshness: freshness,
                escalationTarget: isEscalated ? escalationTarget : null);
        }
        else if (isEscalated)
        {
            return TicketSnapshot.Escalated(
                id: new TicketId((int)record.Number),
                columnId: columnId,
                title: new TicketTitle(record.Title),
                agentId: agentId,
                escalationTarget: escalationTarget ?? agentId,
                retry: retry,
                age: age,
                thinking: false,
                freshness: freshness,
                coAgentId: coAgentId);
        }
        else
        {
            return TicketSnapshot.Open(
                id: new TicketId((int)record.Number),
                columnId: columnId,
                title: new TicketTitle(record.Title),
                agentId: agentId,
                retry: retry,
                age: age,
                thinking: false,
                freshness: freshness);
        }
    }

    private static BoardColumnId MapStatusLabel(IReadOnlyList<string> labels)
    {
        foreach (var label in labels)
        {
            if (label.StartsWith(StatusPrefix, StringComparison.Ordinal))
            {
                return label switch
                {
                    StatusCreated => new BoardColumnId(ColumnCreated),
                    StatusSpecified => new BoardColumnId(ColumnSpecified),
                    StatusInDevelopment => new BoardColumnId(ColumnInDevelopment),
                    StatusInReview => new BoardColumnId(ColumnInReview),
                    StatusInQa => new BoardColumnId(ColumnInQa),
                    StatusAwaitingValidation => new BoardColumnId(ColumnAwaitingValidation),
                    StatusDone => new BoardColumnId(ColumnDone),
                    StatusEscalated => new BoardColumnId(ColumnCreated),
                    _ => new BoardColumnId(ColumnCreated)
                };
            }
        }
        return new BoardColumnId(ColumnCreated);
    }

    private static AgentId MapAgentLabel(IReadOnlyList<string> labels)
    {
        foreach (var label in labels)
        {
            if (label.StartsWith(AgentPrefix, StringComparison.Ordinal))
            {
                var agentValue = label[AgentPrefix.Length..];
                return new AgentId(agentValue);
            }
        }
        return new AgentId(DefaultAgentId);
    }

    private static Retry MapRetryLabel(IReadOnlyList<string> labels)
    {
        foreach (var label in labels)
        {
            if (label.StartsWith(RetryPrefix, StringComparison.Ordinal))
            {
                var retryValue = label[RetryPrefix.Length..];
                if (int.TryParse(retryValue, out var value))
                {
                    return new Retry(value);
                }
            }
        }
        return new Retry(0);
    }

    private static AgentId? MapCoAgentLabel(IReadOnlyList<string> labels)
    {
        foreach (var label in labels)
        {
            if (label.StartsWith(CoAgentPrefix, StringComparison.Ordinal))
            {
                var coAgentValue = label[CoAgentPrefix.Length..];
                return new AgentId(coAgentValue);
            }
        }
        return null;
    }

    private static AgentId? MapEscalationTargetLabel(IReadOnlyList<string> labels)
    {
        foreach (var label in labels)
        {
            if (label.StartsWith(EscalationTargetPrefix, StringComparison.Ordinal))
            {
                var targetValue = label[EscalationTargetPrefix.Length..];
                return new AgentId(targetValue);
            }
        }
        return null;
    }

    private static TicketFreshness CalculateFreshness(
        IReadOnlyList<string> labels,
        string columnIdValue,
        Age age)
    {
        if (labels.Contains(StatusDone) || columnIdValue == ColumnDone)
        {
            if (age.Value < TimeSpan.FromHours(24))
            {
                return TicketFreshness.Fresh;
            }
        }

        if (age.Value >= Age.WarningThreshold)
        {
            return TicketFreshness.Stale;
        }

        return TicketFreshness.Neutral;
    }
}
