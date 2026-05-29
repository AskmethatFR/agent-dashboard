using AgentDashboard.TicketTracking.Application.GitHub;
using AgentDashboard.TicketTracking.Application.Ports;
using Octokit;

namespace AgentDashboard.TicketTracking.Infrastructure.GitHub;

internal sealed class OctokitGitHubIssuesClient : IGitHubIssuesClient
{
    private const string ProductHeader = "agent-dashboard";
    private static readonly TimeSpan GitHubApiTimeout = TimeSpan.FromSeconds(30);

    private readonly GitHubPollingOptions _options;
    private readonly GitHubClient _client;

    public OctokitGitHubIssuesClient(GitHubPollingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
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
}
