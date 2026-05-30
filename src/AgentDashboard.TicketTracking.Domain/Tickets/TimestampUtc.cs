namespace AgentDashboard.TicketTracking.Domain.Tickets;

/// <summary>
/// A value object representing a UTC timestamp.
/// Wraps DateTimeOffset to ensure all timestamps are explicitly UTC and to provide
/// a type-safe alternative to primitive DateTimeOffset usage.
/// </summary>
public sealed record TimestampUtc
{
    /// <summary>
    /// The underlying UTC timestamp value.
    /// </summary>
    public DateTimeOffset Value { get; }

    /// <summary>
    /// Creates a new <see cref="TimestampUtc"/> with the specified UTC timestamp.
    /// </summary>
    /// <param name="value">The UTC timestamp. Must have zero offset.</param>
    /// <exception cref="ArgumentException">Thrown if the timestamp has a non-zero offset.</exception>
    public TimestampUtc(DateTimeOffset value)
    {
        if (value.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Timestamp must be in UTC (offset must be zero).",
                nameof(value));
        }

        Value = value;
    }

    /// <summary>
    /// Implicit conversion from <see cref="DateTimeOffset"/> to <see cref="TimestampUtc"/>
    /// for convenient construction when the value is known to be UTC.
    /// </summary>
    public static implicit operator TimestampUtc(DateTimeOffset value) => new(value);

    /// <summary>
    /// Implicit conversion from <see cref="TimestampUtc"/> to <see cref="DateTimeOffset"/>
    /// for convenient usage in APIs that expect DateTimeOffset.
    /// </summary>
    public static implicit operator DateTimeOffset(TimestampUtc timestamp) => timestamp.Value;

    /// <summary>
    /// Returns the ISO 8601 string representation of the timestamp.
    /// </summary>
    public override string ToString() => Value.ToString("o");
}
