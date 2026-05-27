using AgentDashboard.TicketTracking.Application.GitHub;
using AgentDashboard.TicketTracking.Application.Ports;
using Octokit;

namespace AgentDashboard.TicketTracking.Infrastructure.GitHub;

internal sealed class OctokitGitHubIssuesClient : IGitHubIssuesClient
{
    private const string ProductHeader = "agent-dashboard";

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

        var request = new RepositoryIssueRequest { State = ItemStateFilter.Open };
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
            result.Add(new GitHubIssueRecord(issue.Number, issue.Title, labels, issue.CreatedAt));
        }
        return result;
    }
}
