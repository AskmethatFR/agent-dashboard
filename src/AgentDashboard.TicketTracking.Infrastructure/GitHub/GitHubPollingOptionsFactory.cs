using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgentDashboard.TicketTracking.Infrastructure.GitHub;

public static partial class GitHubPollingOptionsFactory
{
    private const string TokenKey = "GITHUB_TOKEN";
    private const string RepoKey = "GITHUB_REPO";
    private const string IntervalKey = "POLL_INTERVAL_SECONDS";

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

        var repo = configuration[RepoKey];
        if (string.IsNullOrWhiteSpace(repo))
        {
            throw new InvalidOperationException(
                $"{RepoKey} environment variable is missing or empty. Set {RepoKey} in the format owner/name.");
        }

        if (!RepoFormat().IsMatch(repo))
        {
            throw new InvalidOperationException(
                $"{RepoKey} value '{repo}' is not in the expected format 'owner/name' (characters allowed: letters, digits, '.', '_', '-').");
        }

        var slash = repo.IndexOf('/', StringComparison.Ordinal);
        var owner = repo[..slash];
        var name = repo[(slash + 1)..];

        var interval = ResolveInterval(configuration, logger);

        return new GitHubPollingOptions
        {
            Token = token,
            RepositoryOwner = owner,
            RepositoryName = name,
            PollInterval = interval,
        };
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

    [GeneratedRegex("^[A-Za-z0-9._-]+/[A-Za-z0-9._-]+$")]
    private static partial Regex RepoFormat();
}

internal static partial class PollIntervalClampedLog
{
    [LoggerMessage(
        EventId = 100,
        Level = LogLevel.Warning,
        Message = "{interval_key} value {requested_seconds}s is below the minimum {minimum_seconds}s - clamping to minimum.")]
    public static partial void LogClamp(ILogger logger, string interval_key, int requested_seconds, int minimum_seconds);
}
