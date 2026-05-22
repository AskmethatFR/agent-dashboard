namespace AgentDashboard.TicketTracking.Domain.Boards;

public sealed record BoardColumn
{
    public BoardColumnId Id { get; }
    public BoardColumnLabel Label { get; }

    public BoardColumn(BoardColumnId id, BoardColumnLabel label)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(label);

        Id = id;
        Label = label;
    }
}
