using AgentDashboard.TicketTracking.Application.GitHub;
using AgentDashboard.TicketTracking.Application.Ports;

namespace AgentDashboard.TicketTracking.Infrastructure.IntegrationTests.GitHub.Fakes;

internal sealed class FakeGitHubIssuesClient : IGitHubIssuesClient
{
    private readonly TimeProvider _timeProvider;
    private readonly List<DateTimeOffset> _callTimestamps = [];
    private readonly Lock _gate = new();

    private static readonly IReadOnlyList<string> FeatureLabels = new[] { "type:feat" };
    private static readonly IReadOnlyList<string> ChoreLabels = new[] { "type:chore" };
    private static readonly DateTimeOffset DefaultCreatedAt = new(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);

    private static readonly IReadOnlyList<GitHubIssueRecord> CannedResponse =
    [
        new GitHubIssueRecord(1, "first issue", FeatureLabels, DefaultCreatedAt),
        new GitHubIssueRecord(2, "second issue", ChoreLabels, DefaultCreatedAt.AddDays(1)),
    ];

    private IReadOnlyList<GitHubIssueRecord> _customIssues = CannedResponse;

    public FakeGitHubIssuesClient(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        _timeProvider = timeProvider;
    }

    public int CallCount
    {
        get
        {
            lock (_gate)
            {
                return _callTimestamps.Count;
            }
        }
    }

    public IReadOnlyList<DateTimeOffset> CallTimestamps
    {
        get
        {
            lock (_gate)
            {
                return [.. _callTimestamps];
            }
        }
    }

    public void SetIssues(IReadOnlyList<GitHubIssueRecord> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);
        _customIssues = issues;
    }

    public Task<IReadOnlyList<GitHubIssueRecord>> GetOpenIssuesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            _callTimestamps.Add(_timeProvider.GetUtcNow());
        }

        return Task.FromResult(_customIssues);
    }
}
