namespace AgentDashboard.TicketTracking.Domain.Agents;

public sealed record AgentGlyph
{
    public static readonly int MaxLength = 8;

    public string Value { get; }

    public AgentGlyph(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Agent glyph cannot be empty.", nameof(value));
        if (value.Length > MaxLength)
            throw new ArgumentOutOfRangeException(
                nameof(value),
                $"Agent glyph cannot exceed {MaxLength} characters.");

        Value = value;
    }

    public override string ToString() => Value;
}
