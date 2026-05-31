namespace AgentDashboard.TicketTracking.Domain.Tickets;

public sealed record TicketTitle
{
    public static readonly int MaxLength = 512;

    public string Value { get; }

    public TicketTitle(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        // Stryker disable once String
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Ticket title cannot be empty.", nameof(value));
        if (value.Length > MaxLength)
            // Stryker disable once String
            throw new ArgumentOutOfRangeException(
                nameof(value),
                $"Ticket title cannot exceed {MaxLength} characters.");
        if (ContainsRejectedControlCharacter(value))
            // Stryker disable once String
            throw new ArgumentException(
                "Ticket title cannot contain control characters.",
                nameof(value));

        Value = value;
    }

    public override string ToString() => Value;

    private static bool ContainsRejectedControlCharacter(string value)
    {
        foreach (var c in value)
        {
            if (char.IsControl(c) && c != '\t')
                return true;
        }
        return false;
    }
}
