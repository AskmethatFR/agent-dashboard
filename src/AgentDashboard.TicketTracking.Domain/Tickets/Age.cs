namespace AgentDashboard.TicketTracking.Domain.Tickets;

public sealed record Age
{
    public static readonly TimeSpan WarningThreshold = TimeSpan.FromHours(3);

    public TimeSpan Value { get; }

    public Age(TimeSpan value)
    {
        if (value < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(value), "Age cannot be negative.");
        Value = value;
    }

    public bool IsWarning => Value >= WarningThreshold;
}
