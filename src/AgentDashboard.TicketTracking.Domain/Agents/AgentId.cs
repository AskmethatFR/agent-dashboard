namespace AgentDashboard.TicketTracking.Domain.Agents;

public sealed record AgentId
{
    public static readonly int MaxLength = 64;

    public string Value { get; }

    public AgentId(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("AgentId cannot be empty.", nameof(value));
        if (value.Length > MaxLength)
            throw new ArgumentOutOfRangeException(
                nameof(value),
                $"AgentId cannot exceed {MaxLength} characters.");
        Value = value;
    }

    public override string ToString() => Value;
}
