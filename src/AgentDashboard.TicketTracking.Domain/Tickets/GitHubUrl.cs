namespace AgentDashboard.TicketTracking.Domain.Tickets;

/// <summary>
/// A value object representing a valid GitHub HTTPS URL.
/// Ensures that URLs are HTTPS, point to github.com, and are not empty.
/// </summary>
public sealed record GitHubUrl
{
    private const string GitHubDomain = "github.com";
    private const string HttpsScheme = "https";

    /// <summary>
    /// The underlying URL value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new <see cref="GitHubUrl"/> with the specified URL.
    /// </summary>
    /// <param name="value">The GitHub URL. Must be HTTPS and point to github.com.</param>
    /// <exception cref="ArgumentNullException">Thrown if the value is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the value is empty, not HTTPS, or not a GitHub URL.</exception>
    public GitHubUrl(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("GitHub URL cannot be empty or whitespace.", nameof(value));
        }

        if (!value.StartsWith(HttpsScheme + "://", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "GitHub URL must use HTTPS scheme.",
                nameof(value));
        }

        // Extract the host
        var uri = new Uri(value);
        if (!uri.Host.Equals(GitHubDomain, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "GitHub URL must point to github.com domain.",
                nameof(value));
        }

        // Ensure there is a path (not just the domain)
        if (string.IsNullOrEmpty(uri.PathAndQuery) || uri.PathAndQuery == "/")
        {
            throw new ArgumentException(
                "GitHub URL must include a path.",
                nameof(value));
        }

        Value = value;
    }

    /// <summary>
    /// Implicit conversion from <see cref="string"/> to <see cref="GitHubUrl"/>
    /// for convenient construction.
    /// </summary>
    public static implicit operator GitHubUrl(string value) => new(value);

    /// <summary>
    /// Implicit conversion from <see cref="GitHubUrl"/> to <see cref="string"/>
    /// for convenient usage in APIs that expect string.
    /// </summary>
    public static implicit operator string(GitHubUrl gitHubUrl) => gitHubUrl.Value;

    /// <summary>
    /// Returns the URL string.
    /// </summary>
    public override string ToString() => Value;
}
