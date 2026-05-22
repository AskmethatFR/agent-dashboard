namespace AgentDashboard.TicketTracking.Domain.Agents;

public sealed record AgentName
{
    public static readonly int MaxLength = 128;

    public string Value { get; }

    public AgentName(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Agent name cannot be empty.", nameof(value));
        if (value.Length > MaxLength)
            throw new ArgumentOutOfRangeException(
                nameof(value),
                $"Agent name cannot exceed {MaxLength} characters.");

        Value = value;
    }

    public override string ToString() => Value;
}
