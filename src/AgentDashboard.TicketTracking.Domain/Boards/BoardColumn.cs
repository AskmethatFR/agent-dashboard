namespace AgentDashboard.TicketTracking.Domain.Boards;

public sealed record BoardColumn
{
    public const int MaxLabelLength = 128;

    public BoardColumnId Id { get; }
    public string Label { get; }

    public BoardColumn(BoardColumnId id, string label)
    {
        ArgumentNullException.ThrowIfNull(id);
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Column label cannot be empty.", nameof(label));
        if (label.Length > MaxLabelLength)
            throw new ArgumentException($"Column label cannot exceed {MaxLabelLength} characters.", nameof(label));

        Id = id;
        Label = label;
    }
}
