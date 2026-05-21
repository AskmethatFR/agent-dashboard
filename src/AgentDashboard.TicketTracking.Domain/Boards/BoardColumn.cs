namespace AgentDashboard.TicketTracking.Domain.Boards;

public sealed record BoardColumn
{
    public BoardColumnId Id { get; }
    public string Label { get; }

    public BoardColumn(BoardColumnId id, string label)
    {
        ArgumentNullException.ThrowIfNull(id);
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Column label cannot be empty.", nameof(label));

        Id = id;
        Label = label;
    }
}
