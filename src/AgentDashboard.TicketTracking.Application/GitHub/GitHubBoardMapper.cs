using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Boards;
using AgentDashboard.TicketTracking.Domain.Tickets;

namespace AgentDashboard.TicketTracking.Application.GitHub;

public static class GitHubBoardMapper
{
    private static readonly IReadOnlyList<BoardColumn> Columns = new List<BoardColumn>
    {
        new BoardColumn(new BoardColumnId("CREATED"), new BoardColumnLabel("Created")),
        new BoardColumn(new BoardColumnId("SPECIFIED"), new BoardColumnLabel("Specified")),
        new BoardColumn(new BoardColumnId("IN_DEVELOPMENT"), new BoardColumnLabel("In Development")),
        new BoardColumn(new BoardColumnId("IN_REVIEW"), new BoardColumnLabel("In Review")),
        new BoardColumn(new BoardColumnId("IN_QA"), new BoardColumnLabel("In Qa")),
        new BoardColumn(new BoardColumnId("AWAITING_VALIDATION"), new BoardColumnLabel("Awaiting Validation")),
        new BoardColumn(new BoardColumnId("DONE"), new BoardColumnLabel("Done"))
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

    public static BoardSnapshot MapToBoardSnapshot(
        IReadOnlyList<GitHubIssueRecord> records,
        DateTimeOffset now)
    {
        var tickets = records.Select(r => MapToTicket(r, now)).ToList();
        return new BoardSnapshot(Columns, tickets, Agents);
    }

    private static Ticket MapToTicket(GitHubIssueRecord record, DateTimeOffset now)
    {
        var columnId = MapStatusLabel(record.Labels);
        var agentId = MapAgentLabel(record.Labels);
        var retry = MapRetryLabel(record.Labels);
        var coAgentId = MapCoAgentLabel(record.Labels);
        var escalationTarget = MapEscalationTargetLabel(record.Labels);
        var hasInReviewStatus = record.Labels.Contains("status:in-review");
        var hasEscalatedStatus = record.Labels.Contains("status:escalated");
        var isEscalated = hasEscalatedStatus && retry.Value >= 3;

        var age = new Age(now - record.CreatedAt);
        var freshness = CalculateFreshness(record.Labels, columnId.Value, age);

        if (hasInReviewStatus)
        {
            // InCrossReview requires non-null coAgentId, so use agentId if coAgentId is null
            return Ticket.InCrossReview(
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
            return Ticket.Escalated(
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
            return Ticket.Open(
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
            if (label.StartsWith("status:", StringComparison.Ordinal))
            {
                return label switch
                {
                    "status:created" => new BoardColumnId("CREATED"),
                    "status:specified" => new BoardColumnId("SPECIFIED"),
                    "status:in-development" => new BoardColumnId("IN_DEVELOPMENT"),
                    "status:in-review" => new BoardColumnId("IN_REVIEW"),
                    "status:in-qa" => new BoardColumnId("IN_QA"),
                    "status:awaiting-validation" => new BoardColumnId("AWAITING_VALIDATION"),
                    "status:done" => new BoardColumnId("DONE"),
                    "status:escalated" => new BoardColumnId("CREATED"),
                    _ => new BoardColumnId("CREATED")
                };
            }
        }
        return new BoardColumnId("CREATED");
    }

    private static AgentId MapAgentLabel(IReadOnlyList<string> labels)
    {
        foreach (var label in labels)
        {
            if (label.StartsWith("agent:", StringComparison.Ordinal))
            {
                var agentValue = label["agent:".Length..];
                return new AgentId(agentValue);
            }
        }
        return new AgentId("pm");
    }

    private static Retry MapRetryLabel(IReadOnlyList<string> labels)
    {
        foreach (var label in labels)
        {
            if (label.StartsWith("retry:", StringComparison.Ordinal))
            {
                var retryValue = label["retry:".Length..];
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
            if (label.StartsWith("co-agent:", StringComparison.Ordinal))
            {
                var coAgentValue = label["co-agent:".Length..];
                return new AgentId(coAgentValue);
            }
        }
        return null;
    }

    private static AgentId? MapEscalationTargetLabel(IReadOnlyList<string> labels)
    {
        foreach (var label in labels)
        {
            if (label.StartsWith("escalation-target:", StringComparison.Ordinal))
            {
                var targetValue = label["escalation-target:".Length..];
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
        if (labels.Contains("status:done") || columnIdValue == "DONE")
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
