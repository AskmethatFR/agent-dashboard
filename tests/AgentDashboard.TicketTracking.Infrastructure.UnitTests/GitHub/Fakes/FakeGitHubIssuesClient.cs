using AgentDashboard.TicketTracking.Application.GitHub;
using AgentDashboard.TicketTracking.Application.Ports;

namespace AgentDashboard.TicketTracking.Infrastructure.UnitTests.GitHub.Fakes;

internal sealed class FakeGitHubIssuesClient : IGitHubIssuesClient
{
    private readonly List<GitHubIssueRecord> _records;
    private readonly bool _shouldFail;
    private int _callCount;

    public int CallCount => _callCount;

    public FakeGitHubIssuesClient(IReadOnlyList<GitHubIssueRecord> records, bool shouldFail = false)
    {
        _records = new List<GitHubIssueRecord>(records);
        _shouldFail = shouldFail;
    }

    public Task<IReadOnlyList<GitHubIssueRecord>> GetOpenIssuesAsync(CancellationToken cancellationToken)
    {
        _callCount++;
        
        if (_shouldFail)
        {
            throw new InvalidOperationException("GitHub API call failed");
        }
        
        return Task.FromResult<IReadOnlyList<GitHubIssueRecord>>(_records);
    }
}
