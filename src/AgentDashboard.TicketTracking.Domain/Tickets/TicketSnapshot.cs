using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Boards;

namespace AgentDashboard.TicketTracking.Domain.Tickets;

public sealed record TicketSnapshot
{
    public TicketId Id { get; }
    public BoardColumnId ColumnId { get; }
    public TicketTitle Title { get; }
    public AgentId AgentId { get; }
    public AgentId? CoAgentId { get; }
    public bool IsInCrossReview { get; }
    public Retry Retry { get; }
    public Age Age { get; }
    public bool IsThinking { get; }
    public TicketFreshness Freshness { get; }
    public bool IsEscalated { get; }
    public AgentId? EscalationTarget { get; }

    private TicketSnapshot(
        TicketId id,
        BoardColumnId columnId,
        TicketTitle title,
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

    public static TicketSnapshot Open(
        TicketId id,
        BoardColumnId columnId,
        TicketTitle title,
        AgentId agentId,
        Retry retry,
        Age age,
        bool thinking,
        TicketFreshness freshness)
    {
        GuardCommonInputs(id, columnId, title, agentId, retry, age);
        return new TicketSnapshot(
            id, columnId, title, agentId,
            coAgentId: null, crossReview: false,
            retry, age, thinking, freshness,
            escalated: false, escalationTarget: null);
    }

    public static TicketSnapshot InCrossReview(
        TicketId id,
        BoardColumnId columnId,
        TicketTitle title,
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
        return new TicketSnapshot(
            id, columnId, title, agentId,
            coAgentId, crossReview: true,
            retry, age, thinking, freshness,
            escalated: escalationTarget is not null,
            escalationTarget);
    }

    public static TicketSnapshot Escalated(
        TicketId id,
        BoardColumnId columnId,
        TicketTitle title,
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
        return new TicketSnapshot(
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
        TicketTitle title,
        AgentId agentId,
        Retry retry,
        Age age)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(columnId);
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(agentId);
        ArgumentNullException.ThrowIfNull(retry);
        ArgumentNullException.ThrowIfNull(age);
    }
}
