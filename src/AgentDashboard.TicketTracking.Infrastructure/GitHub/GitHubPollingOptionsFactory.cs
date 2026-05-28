using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgentDashboard.TicketTracking.Infrastructure.GitHub;

public static class GitHubPollingOptionsFactory
{
    private const string TokenKey = "GITHUB_TOKEN";
    private const string IntervalKey = "POLL_INTERVAL_SECONDS";
    private const string TokenPrefix = "ghp_";

    private static readonly TimeSpan DefaultInterval = TimeSpan.FromSeconds(600);
    private static readonly TimeSpan MinimumInterval = TimeSpan.FromSeconds(300);

    public static GitHubPollingOptions FromConfiguration(IConfiguration configuration, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(logger);

        var token = configuration[TokenKey];
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                $"{TokenKey} environment variable is missing or empty. Set {TokenKey} to a GitHub personal access token before starting the host.");
        }

        ValidateTokenFormat(token);

        var interval = ResolveInterval(configuration, logger);

        return GitHubPollingOptions.ForDogfooding(token, interval);
    }

    private static void ValidateTokenFormat(string token)
    {
        if (!token.StartsWith(TokenPrefix, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"{TokenKey} must be a valid GitHub Personal Access Token starting with '{TokenPrefix}'. Found: {token[..Math.Min(10, token.Length)]}...");
        }

        // Validate minimum length (ghp_ + at least some characters)
        if (token.Length < TokenPrefix.Length + 10)
        {
            throw new InvalidOperationException(
                $"{TokenKey} appears to be malformed. A valid GitHub PAT should be longer.");
        }
    }

    private static TimeSpan ResolveInterval(IConfiguration configuration, ILogger logger)
    {
        var raw = configuration[IntervalKey];
        if (string.IsNullOrWhiteSpace(raw))
        {
            return DefaultInterval;
        }

        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds))
        {
            throw new InvalidOperationException(
                $"{IntervalKey} value '{raw}' is not a valid integer number of seconds.");
        }

        var requested = TimeSpan.FromSeconds(seconds);
        if (requested < MinimumInterval)
        {
            PollIntervalClampedLog.LogClamp(logger, IntervalKey, seconds, (int)MinimumInterval.TotalSeconds);
            return MinimumInterval;
        }

        return requested;
    }
}

internal static partial class PollIntervalClampedLog
{
    [LoggerMessage(
        EventId = 100,
        Level = LogLevel.Warning,
        Message = "{interval_key} value {requested_seconds}s is below the minimum {minimum_seconds}s - clamping to minimum.")]
    public static partial void LogClamp(ILogger logger, string interval_key, int requested_seconds, int minimum_seconds);
}
