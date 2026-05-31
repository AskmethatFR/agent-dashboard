namespace AgentDashboard.TicketTracking.Domain.Tickets;

/// <summary>
/// A value object representing a GitHub issue number.
/// Must be a positive long integer.
/// </summary>
public sealed record GitHubIssueNumber
{
    /// <summary>
    /// The underlying issue number value.
    /// </summary>
    public long Value { get; }

    /// <summary>
    /// Creates a new <see cref="GitHubIssueNumber"/> with the specified number.
    /// </summary>
    /// <param name="value">The GitHub issue number. Must be positive.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is not positive.</exception>
    public GitHubIssueNumber(long value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                "GitHub issue number must be positive.");
        }

        Value = value;
    }

    /// <summary>
    /// Implicit conversion from <see cref="long"/> to <see cref="GitHubIssueNumber"/>
    /// for convenient construction.
    /// </summary>
    public static implicit operator GitHubIssueNumber(long value) => new(value);

    /// <summary>
    /// Implicit conversion from <see cref="GitHubIssueNumber"/> to <see cref="long"/>
    /// for convenient usage in APIs that expect long.
    /// </summary>
    public static implicit operator long(GitHubIssueNumber issueNumber) => issueNumber.Value;

    /// <summary>
    /// Returns the issue number as a string.
    /// </summary>
    public override string ToString() => Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
