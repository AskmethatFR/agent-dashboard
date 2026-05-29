namespace AgentDashboard.TicketTracking.Domain.Tickets;

/// <summary>
/// A value object representing a GitHub repository in the format "owner/name".
/// </summary>
public sealed record RepositorySource
{
    /// <summary>
    /// The format string for repository source (e.g., "owner/name").
    /// </summary>
    public const string Format = "owner/name";

    /// <summary>
    /// The underlying repository source value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// The owner part of the repository.
    /// </summary>
    public string Owner { get; }

    /// <summary>
    /// The name part of the repository.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Creates a new <see cref="RepositorySource"/> with the specified repository string.
    /// </summary>
    /// <param name="value">The repository in format "owner/name".</param>
    /// <exception cref="ArgumentNullException">Thrown if the value is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the value is empty, has wrong format, or parts are invalid.</exception>
    public RepositorySource(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Repository source cannot be empty or whitespace.", nameof(value));
        }

        var parts = value.Split('/');
        if (parts.Length != 2)
        {
            throw new ArgumentException(
                "Repository source must be in format 'owner/name'.",
                nameof(value));
        }

        Owner = parts[0];
        Name = parts[1];

        if (string.IsNullOrWhiteSpace(Owner))
        {
            throw new ArgumentException(
                "Repository owner cannot be empty.",
                nameof(value));
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new ArgumentException(
                "Repository name cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    /// <summary>
    /// Implicit conversion from <see cref="string"/> to <see cref="RepositorySource"/>
    /// for convenient construction.
    /// </summary>
    public static implicit operator RepositorySource(string value) => new(value);

    /// <summary>
    /// Implicit conversion from <see cref="RepositorySource"/> to <see cref="string"/>
    /// for convenient usage in APIs that expect string.
    /// </summary>
    public static implicit operator string(RepositorySource repositorySource) => repositorySource.Value;

    /// <summary>
    /// Returns the repository source string.
    /// </summary>
    public override string ToString() => Value;
}
