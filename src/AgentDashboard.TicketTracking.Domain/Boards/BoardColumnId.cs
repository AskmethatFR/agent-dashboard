namespace AgentDashboard.TicketTracking.Domain.Boards;

public sealed record BoardColumnId
{
    public const int MaxLength = 64;

    public string Value { get; }

    public BoardColumnId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("BoardColumnId cannot be empty.", nameof(value));
        if (value.Length > MaxLength)
            throw new ArgumentException($"BoardColumnId cannot exceed {MaxLength} characters.", nameof(value));
        Value = value;
    }

    public override string ToString() => Value;
}
