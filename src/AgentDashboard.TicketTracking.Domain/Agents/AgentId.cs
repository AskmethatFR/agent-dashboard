namespace AgentDashboard.TicketTracking.Domain.Agents;

public sealed record AgentId
{
    public string Value { get; }

    public AgentId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("AgentId cannot be empty.", nameof(value));
        Value = value;
    }

    public override string ToString() => Value;
}
