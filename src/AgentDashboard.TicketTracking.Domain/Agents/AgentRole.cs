namespace AgentDashboard.TicketTracking.Domain.Agents;

public sealed record AgentRole
{
    public static readonly int MaxLength = 64;

    public string Value { get; }

    public AgentRole(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        // Stryker disable once String
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Agent role cannot be empty.", nameof(value));
        if (value.Length > MaxLength)
            // Stryker disable once String
            throw new ArgumentOutOfRangeException(
                nameof(value),
                $"Agent role cannot exceed {MaxLength} characters.");

        Value = value;
    }

    public override string ToString() => Value;
}
