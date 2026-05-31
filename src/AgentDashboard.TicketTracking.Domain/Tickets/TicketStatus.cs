namespace AgentDashboard.TicketTracking.Domain.Tickets;

/// <summary>
/// Enumeration of possible ticket statuses.
/// </summary>
public enum TicketStatusValue
{
    Created,
    Specified,
    InDevelopment,
    InReview,
    InQa,
    AwaitingValidation,
    Done,
    Escalated
}

/// <summary>
/// A value object representing a ticket status.
/// </summary>
public sealed record TicketStatus
{
    /// <summary>
    /// The underlying status value.
    /// </summary>
    public TicketStatusValue Value { get; }

    /// <summary>
    /// Creates a new <see cref="TicketStatus"/> with the specified status.
    /// </summary>
    /// <param name="value">The ticket status value.</param>
    public TicketStatus(TicketStatusValue value)
    {
        Value = value;
    }

    /// <summary>
    /// Parses a string to create a <see cref="TicketStatus"/>
    /// </summary>
    /// <param name="value">The string representation of the status.</param>
    /// <returns>A <see cref="TicketStatus"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if the string cannot be parsed.</exception>
    public static TicketStatus Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            // Stryker disable once String
            throw new ArgumentException("Status value cannot be empty.", nameof(value));
        }

        var cleanValue = value.Replace("status:", string.Empty, System.StringComparison.OrdinalIgnoreCase);

        if (System.Enum.TryParse<TicketStatusValue>(cleanValue, true, out var statusValue))
        {
            return new TicketStatus(statusValue);
        }

        // Stryker disable once String
        var validValues = string.Join(", ", System.Enum.GetNames<TicketStatusValue>());
        // Stryker disable once String
        throw new ArgumentException(
            "Cannot parse '" + value + "' as a valid TicketStatus. Valid values are: " + validValues,
            nameof(value));
    }

    /// <summary>
    /// Implicit conversion from <see cref="TicketStatusValue"/> to <see cref="TicketStatus"/>
    /// for convenient construction.
    /// </summary>
    public static implicit operator TicketStatus(TicketStatusValue value) => new(value);

    /// <summary>
    /// Implicit conversion from <see cref="TicketStatus"/> to <see cref="TicketStatusValue"/>
    /// for convenient usage in APIs that expect TicketStatusValue.
    /// </summary>
    public static implicit operator TicketStatusValue(TicketStatus status) => status.Value;

    /// <summary>
    /// Returns the status value as a string.
    /// </summary>
    public override string ToString() => Value.ToString();
}
