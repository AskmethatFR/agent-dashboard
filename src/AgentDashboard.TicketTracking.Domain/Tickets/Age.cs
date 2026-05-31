namespace AgentDashboard.TicketTracking.Domain.Tickets;

public sealed record Age
{
    public static readonly TimeSpan WarningThreshold = TimeSpan.FromHours(3);

    public TimeSpan Value { get; }

    public Age(TimeSpan value)
    {
        if (value < TimeSpan.Zero)
            // Stryker disable once String
            throw new ArgumentOutOfRangeException(nameof(value), "Age cannot be negative.");
        Value = value;
    }

    public bool IsWarning => Value >= WarningThreshold;
}
