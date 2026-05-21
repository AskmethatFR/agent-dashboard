using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Boards;
using AgentDashboard.TicketTracking.Domain.Tickets;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Tickets;

internal sealed class TicketBuilder
{
    private TicketId _id = new(1);
    private BoardColumnId _columnId = new("CREATED");
    private TicketTitle _title = new("any title");
    private AgentId _agentId = new("DA");
    private AgentId? _coAgentId;
    private AgentId? _escalationTarget;
    private Retry _retry = new(0);
    private Age _age = new(TimeSpan.FromMinutes(10));
    private bool _thinking;
    private TicketFreshness _freshness = TicketFreshness.Neutral;
    private TicketState _state = TicketState.Open;

    public TicketBuilder WithId(TicketId id) { _id = id; return this; }
    public TicketBuilder WithId(int id) { _id = new TicketId(id); return this; }
    public TicketBuilder WithColumn(BoardColumnId columnId) { _columnId = columnId; return this; }
    public TicketBuilder WithColumn(string columnId) { _columnId = new BoardColumnId(columnId); return this; }
    public TicketBuilder WithTitle(TicketTitle title) { _title = title; return this; }
    public TicketBuilder WithTitle(string title) { _title = new TicketTitle(title); return this; }
    public TicketBuilder WithAgent(AgentId agentId) { _agentId = agentId; return this; }
    public TicketBuilder WithAgent(string agentId) { _agentId = new AgentId(agentId); return this; }
    public TicketBuilder WithCoAgent(AgentId? coAgentId) { _coAgentId = coAgentId; return this; }
    public TicketBuilder WithCoAgent(string coAgentId) { _coAgentId = new AgentId(coAgentId); return this; }
    public TicketBuilder WithEscalationTarget(AgentId? target) { _escalationTarget = target; return this; }
    public TicketBuilder WithEscalationTarget(string target) { _escalationTarget = new AgentId(target); return this; }
    public TicketBuilder WithRetry(Retry retry) { _retry = retry; return this; }
    public TicketBuilder WithRetry(int retry) { _retry = new Retry(retry); return this; }
    public TicketBuilder WithAge(Age age) { _age = age; return this; }
    public TicketBuilder WithAge(TimeSpan age) { _age = new Age(age); return this; }
    public TicketBuilder WithThinking(bool thinking) { _thinking = thinking; return this; }
    public TicketBuilder WithFreshness(TicketFreshness freshness) { _freshness = freshness; return this; }
    public TicketBuilder AsOpen() { _state = TicketState.Open; return this; }
    public TicketBuilder AsInCrossReview() { _state = TicketState.InCrossReview; return this; }
    public TicketBuilder AsEscalated() { _state = TicketState.Escalated; return this; }

    public Ticket Build() => _state switch
    {
        TicketState.Open => Ticket.Open(
            _id, _columnId, _title, _agentId,
            _retry, _age, _thinking, _freshness),
        TicketState.InCrossReview => Ticket.InCrossReview(
            _id, _columnId, _title, _agentId, _coAgentId ?? new AgentId("DB"),
            _retry, _age, _thinking, _freshness,
            _escalationTarget),
        TicketState.Escalated => Ticket.Escalated(
            _id, _columnId, _title, _agentId, _escalationTarget ?? new AgentId("PM"),
            _retry, _age, _thinking, _freshness,
            _coAgentId),
        _ => throw new InvalidOperationException($"Unknown state {_state}"),
    };

    private enum TicketState
    {
        Open,
        InCrossReview,
        Escalated,
    }
}
