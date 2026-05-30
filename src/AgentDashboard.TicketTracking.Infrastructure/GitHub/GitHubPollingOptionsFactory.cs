using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgentDashboard.TicketTracking.Infrastructure.GitHub;

internal static class GitHubPollingOptionsFactory
{
    private const string TokenKey = "GITHUB_TOKEN";
    private const string IntervalKey = "POLL_INTERVAL_SECONDS";

    private static readonly (string Prefix, int MinLength)[] AcceptedTokenPrefixes =
    {
        ("ghp_", 14),          // ghp_ + at least 10 chars  (preserves existing behavior)
        ("github_pat_", 21),   // github_pat_ + at least 10 chars
        ("gho_", 14),          // gh CLI OAuth token
    };

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
        var match = AcceptedTokenPrefixes.FirstOrDefault(p => token.StartsWith(p.Prefix, StringComparison.Ordinal));

        if (match.Prefix is null)
        {
            throw new InvalidOperationException(
                $"{TokenKey} must be a valid GitHub Personal Access Token. Accepted prefixes: 'ghp_' (classic), 'github_pat_' (fine-grained), or 'gho_' (GitHub CLI OAuth). Found a token with an unrecognized prefix.");
        }

        if (token.Length < match.MinLength)
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
