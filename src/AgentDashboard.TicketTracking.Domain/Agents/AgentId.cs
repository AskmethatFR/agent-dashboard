namespace AgentDashboard.TicketTracking.Domain.Agents;

public sealed record AgentId
{
    public const int MaxLength = 64;

    public string Value { get; }

    public AgentId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("AgentId cannot be empty.", nameof(value));
        if (value.Length > MaxLength)
            throw new ArgumentException($"AgentId cannot exceed {MaxLength} characters.", nameof(value));
        Value = value;
    }

    public override string ToString() => Value;
}
