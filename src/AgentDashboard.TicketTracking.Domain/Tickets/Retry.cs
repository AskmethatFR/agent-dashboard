namespace AgentDashboard.TicketTracking.Domain.Tickets;

public sealed record Retry
{
    public const int MaxBeforeEscalation = 3;
    public const int WarnAt = 2;

    public int Value { get; }

    public Retry(int value)
    {
        if (value < 0 || value > MaxBeforeEscalation)
            throw new ArgumentOutOfRangeException(
                nameof(value),
                $"Retry must be between 0 and {MaxBeforeEscalation}.");
        Value = value;
    }

    public bool IsAtWarnThreshold => Value == WarnAt;
    public bool IsAtDangerThreshold => Value >= MaxBeforeEscalation;
}
