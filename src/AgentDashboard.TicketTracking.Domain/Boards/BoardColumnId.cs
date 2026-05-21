namespace AgentDashboard.TicketTracking.Domain.Boards;

public sealed record BoardColumnId
{
    public string Value { get; }

    public BoardColumnId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("BoardColumnId cannot be empty.", nameof(value));
        Value = value;
    }

    public override string ToString() => Value;
}
