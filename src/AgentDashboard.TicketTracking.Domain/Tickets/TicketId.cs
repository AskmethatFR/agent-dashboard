namespace AgentDashboard.TicketTracking.Domain.Tickets;

public sealed record TicketId
{
    public int Value { get; }

    public TicketId(int value)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value), "TicketId must be positive.");
        Value = value;
    }

    public override string ToString() => Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
