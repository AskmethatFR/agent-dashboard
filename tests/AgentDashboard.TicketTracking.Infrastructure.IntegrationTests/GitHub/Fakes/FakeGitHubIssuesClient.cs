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

    private static readonly IReadOnlyList<GitHubIssueRecord> CannedResponse =
    [
        new GitHubIssueRecord(1, "first issue", FeatureLabels),
        new GitHubIssueRecord(2, "second issue", ChoreLabels),
    ];

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

    public Task<IReadOnlyList<GitHubIssueRecord>> GetOpenIssuesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            _callTimestamps.Add(_timeProvider.GetUtcNow());
        }

        return Task.FromResult(CannedResponse);
    }
}
