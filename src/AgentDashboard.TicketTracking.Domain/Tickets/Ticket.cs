using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Boards;

namespace AgentDashboard.TicketTracking.Domain.Tickets;

public sealed record Ticket
{
    public TicketId Id { get; }
    public BoardColumnId ColumnId { get; }
    public string Title { get; }
    public AgentId AgentId { get; }
    public AgentId? CoAgentId { get; }
    public bool CrossReview { get; }
    public Retry Retry { get; }
    public Age Age { get; }
    public bool Thinking { get; }
    public bool Fresh { get; }
    public bool Stale { get; }
    public bool Escalated { get; }
    public AgentId? EscalationTarget { get; }

    public Ticket(
        TicketId id,
        BoardColumnId columnId,
        string title,
        AgentId agentId,
        AgentId? coAgentId,
        bool crossReview,
        Retry retry,
        Age age,
        bool thinking,
        bool fresh,
        bool stale,
        bool escalated,
        AgentId? escalationTarget)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(columnId);
        ArgumentNullException.ThrowIfNull(agentId);
        ArgumentNullException.ThrowIfNull(retry);
        ArgumentNullException.ThrowIfNull(age);

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Ticket title cannot be empty.", nameof(title));
        if (crossReview && coAgentId is null)
            throw new ArgumentException("Cross-review requires a co-agent.", nameof(coAgentId));
        if (!crossReview && coAgentId is not null)
            throw new ArgumentException("Co-agent is only allowed in cross-review.", nameof(coAgentId));
        if (escalated && escalationTarget is null)
            throw new ArgumentException("Escalated ticket requires an escalation target.", nameof(escalationTarget));
        if (!escalated && escalationTarget is not null)
            throw new ArgumentException("Escalation target is only allowed when escalated.", nameof(escalationTarget));
        if (fresh && stale)
            throw new ArgumentException("Ticket cannot be both fresh and stale.", nameof(fresh));

        Id = id;
        ColumnId = columnId;
        Title = title;
        AgentId = agentId;
        CoAgentId = coAgentId;
        CrossReview = crossReview;
        Retry = retry;
        Age = age;
        Thinking = thinking;
        Fresh = fresh;
        Stale = stale;
        Escalated = escalated;
        EscalationTarget = escalationTarget;
    }

    public TicketSeverity Severity =>
        Escalated ? TicketSeverity.Escalated
        : Retry.IsAtDangerThreshold ? TicketSeverity.Danger
        : Retry.IsAtWarnThreshold ? TicketSeverity.Warn
        : TicketSeverity.Normal;
}
