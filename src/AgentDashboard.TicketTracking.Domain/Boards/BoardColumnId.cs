namespace AgentDashboard.TicketTracking.Domain.Boards;

public sealed record BoardColumnId
{
    public static readonly int MaxLength = 64;

    public string Value { get; }

    public BoardColumnId(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        // Stryker disable once String
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("BoardColumnId cannot be empty.", nameof(value));
        if (value.Length > MaxLength)
            // Stryker disable once String
            throw new ArgumentOutOfRangeException(
                nameof(value),
                $"BoardColumnId cannot exceed {MaxLength} characters.");
        Value = value;
    }

    public override string ToString() => Value;
}
