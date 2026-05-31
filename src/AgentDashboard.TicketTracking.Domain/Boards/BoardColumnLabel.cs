namespace AgentDashboard.TicketTracking.Domain.Boards;

public sealed record BoardColumnLabel
{
    public static readonly int MaxLength = 128;

    public string Value { get; }

    public BoardColumnLabel(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        // Stryker disable once String
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Column label cannot be empty.", nameof(value));
        if (value.Length > MaxLength)
            // Stryker disable once String
            throw new ArgumentOutOfRangeException(
                nameof(value),
                $"Column label cannot exceed {MaxLength} characters.");

        Value = value;
    }

    public override string ToString() => Value;
}
