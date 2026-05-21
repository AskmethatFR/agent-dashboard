using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Boards;

namespace AgentDashboard.TicketTracking.Domain.Tickets;

public sealed record Ticket
{
    public const int MaxTitleLength = 512;

    public TicketId Id { get; }
    public BoardColumnId ColumnId { get; }
    public string Title { get; }
    public AgentId AgentId { get; }
    public AgentId? CoAgentId { get; }
    public bool IsInCrossReview { get; }
    public Retry Retry { get; }
    public Age Age { get; }
    public bool IsThinking { get; }
    public TicketFreshness Freshness { get; }
    public bool IsEscalated { get; }
    public AgentId? EscalationTarget { get; }

    private Ticket(
        TicketId id,
        BoardColumnId columnId,
        string title,
        AgentId agentId,
        AgentId? coAgentId,
        bool crossReview,
        Retry retry,
        Age age,
        bool thinking,
        TicketFreshness freshness,
        bool escalated,
        AgentId? escalationTarget)
    {
        Id = id;
        ColumnId = columnId;
        Title = title;
        AgentId = agentId;
        CoAgentId = coAgentId;
        IsInCrossReview = crossReview;
        Retry = retry;
        Age = age;
        IsThinking = thinking;
        Freshness = freshness;
        IsEscalated = escalated;
        EscalationTarget = escalationTarget;
    }

    public static Ticket Open(
        TicketId id,
        BoardColumnId columnId,
        string title,
        AgentId agentId,
        Retry retry,
        Age age,
        bool thinking,
        TicketFreshness freshness)
    {
        GuardCommonInputs(id, columnId, title, agentId, retry, age);
        return new Ticket(
            id, columnId, title, agentId,
            coAgentId: null, crossReview: false,
            retry, age, thinking, freshness,
            escalated: false, escalationTarget: null);
    }

    public static Ticket InCrossReview(
        TicketId id,
        BoardColumnId columnId,
        string title,
        AgentId agentId,
        AgentId coAgentId,
        Retry retry,
        Age age,
        bool thinking,
        TicketFreshness freshness,
        AgentId? escalationTarget = null)
    {
        GuardCommonInputs(id, columnId, title, agentId, retry, age);
        ArgumentNullException.ThrowIfNull(coAgentId);
        return new Ticket(
            id, columnId, title, agentId,
            coAgentId, crossReview: true,
            retry, age, thinking, freshness,
            escalated: escalationTarget is not null,
            escalationTarget);
    }

    public static Ticket Escalated(
        TicketId id,
        BoardColumnId columnId,
        string title,
        AgentId agentId,
        AgentId escalationTarget,
        Retry retry,
        Age age,
        bool thinking,
        TicketFreshness freshness,
        AgentId? coAgentId = null)
    {
        GuardCommonInputs(id, columnId, title, agentId, retry, age);
        ArgumentNullException.ThrowIfNull(escalationTarget);
        return new Ticket(
            id, columnId, title, agentId,
            coAgentId, crossReview: coAgentId is not null,
            retry, age, thinking, freshness,
            escalated: true, escalationTarget);
    }

    public TicketSeverity Severity =>
        IsEscalated ? TicketSeverity.Escalated
        : Retry.IsAtDangerThreshold ? TicketSeverity.Danger
        : Retry.IsAtWarnThreshold ? TicketSeverity.Warn
        : TicketSeverity.Normal;

    private static void GuardCommonInputs(
        TicketId id,
        BoardColumnId columnId,
        string title,
        AgentId agentId,
        Retry retry,
        Age age)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(columnId);
        ArgumentNullException.ThrowIfNull(agentId);
        ArgumentNullException.ThrowIfNull(retry);
        ArgumentNullException.ThrowIfNull(age);
        GuardTitle(title);
    }

    private static void GuardTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Ticket title cannot be empty.", nameof(title));
        if (title.Length > MaxTitleLength)
            throw new ArgumentException($"Ticket title cannot exceed {MaxTitleLength} characters.", nameof(title));
        foreach (var c in title)
        {
            if (char.IsControl(c) && c != '\t')
                throw new ArgumentException("Ticket title cannot contain control characters.", nameof(title));
        }
    }
}
