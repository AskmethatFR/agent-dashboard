#pragma warning disable CA1848 // LoggerMessage delegates - using LogDebug for rate limit check failures

using AgentDashboard.TicketTracking.Application.GitHub;
using AgentDashboard.TicketTracking.Application.Ports;
using Microsoft.Extensions.Logging;
using Octokit;

namespace AgentDashboard.TicketTracking.Infrastructure.GitHub;

internal sealed class OctokitGitHubIssuesClient : IGitHubIssuesClient
{
    private const string ProductHeader = "agent-dashboard";
    private static readonly TimeSpan GitHubApiTimeout = TimeSpan.FromSeconds(30);
    private const int RateLimitThreshold = 10;
    private const int RateLimitBackoffSeconds = 60;

    private readonly GitHubPollingOptions _options;
    private readonly GitHubClient _client;
    private readonly ILogger<OctokitGitHubIssuesClient> _logger;

    internal OctokitGitHubIssuesClient(GitHubPollingOptions options, ILogger<OctokitGitHubIssuesClient> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        _options = options;
        _logger = logger;
        _client = new GitHubClient(new ProductHeaderValue(ProductHeader))
        {
            Credentials = new Credentials(options.Token),
        };
    }

    public async Task<IReadOnlyList<GitHubIssueRecord>> GetOpenIssuesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(GitHubApiTimeout);
        
        var request = new RepositoryIssueRequest { State = ItemStateFilter.Open };
        
        try
        {
            var issues = await _client.Issue
                .GetAllForRepository(_options.RepositoryOwner, _options.RepositoryName, request)
                .ConfigureAwait(false);

            await CheckRateLimitAndBackoffIfNeededAsync(cancellationToken).ConfigureAwait(false);

            var result = new List<GitHubIssueRecord>(issues.Count);
            foreach (var issue in issues)
            {
                var labels = new List<string>(issue.Labels.Count);
                foreach (var label in issue.Labels)
                {
                    labels.Add(label.Name);
                }
                result.Add(new GitHubIssueRecord(
                    issue.Number,
                    issue.Title,
                    labels,
                    issue.CreatedAt,
                    issue.HtmlUrl,
                    issue.UpdatedAt ?? issue.CreatedAt,
                    issue.ClosedAt));
            }
            return result;
        }
        catch (TaskCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            throw new TimeoutException("GitHub API request timed out after 30 seconds");
        }
    }

    private async Task CheckRateLimitAndBackoffIfNeededAsync(CancellationToken ct)
    {
        try
        {
            var rateLimitRemaining = GetRateLimitRemaining();
            if (rateLimitRemaining < RateLimitThreshold)
            {
                _logger.LogWarning("GitHub API rate limit low: {Remaining} requests remaining", rateLimitRemaining);
                await Task.Delay(TimeSpan.FromSeconds(RateLimitBackoffSeconds), ct).ConfigureAwait(false);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogDebug(ex, "Failed to check GitHub API rate limit");
        }
    }

    private int GetRateLimitRemaining()
    {
        var lastResponse = GetLastResponse(_client.Connection);
        if (lastResponse is null)
        {
            return 0;
        }
        
        var apiInfo = GetApiInfo(lastResponse);
        if (apiInfo is null)
        {
            return 0;
        }
        
        var rateLimit = GetRateLimit(apiInfo);
        if (rateLimit is null)
        {
            return 0;
        }
        
        return GetRemainingRequests(rateLimit);
    }

    private static object? GetLastResponse(object connection)
    {
        var connectionType = connection.GetType();
        var lastResponseProperty = connectionType.GetProperty(
            "LastResponse",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return lastResponseProperty?.GetValue(connection);
    }

    private static object? GetApiInfo(object lastResponse)
    {
        return lastResponse.GetType().GetProperty("ApiInfo")?.GetValue(lastResponse);
    }

    private static object? GetRateLimit(object apiInfo)
    {
        return apiInfo.GetType().GetProperty("RateLimit")?.GetValue(apiInfo);
    }

    private static int GetRemainingRequests(object rateLimit)
    {
        var remainingProperty = rateLimit.GetType().GetProperty("Remaining");
        return (int?)remainingProperty?.GetValue(rateLimit) ?? 0;
    }
}
