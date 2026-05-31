namespace AgentDashboard.TicketTracking.Domain.Tickets;

public sealed record Retry
{
    public static readonly int MaxBeforeEscalation = 3;
    public static readonly int WarnAt = 2;

    public int Value { get; }

    public Retry(int value)
    {
        if (value < 0 || value > MaxBeforeEscalation)
            // Stryker disable once String
            throw new ArgumentOutOfRangeException(
                nameof(value),
                $"Retry must be between 0 and {MaxBeforeEscalation}.");
        Value = value;
    }

    public bool IsAtWarnThreshold => Value == WarnAt;
    public bool IsAtDangerThreshold => Value >= MaxBeforeEscalation;
}
