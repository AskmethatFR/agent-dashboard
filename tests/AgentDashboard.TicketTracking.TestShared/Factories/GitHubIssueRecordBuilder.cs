using AgentDashboard.TicketTracking.Application.GitHub;

namespace AgentDashboard.TicketTracking.TestShared.Factories;

public sealed class GitHubIssueRecordBuilder
{
    private long _number = 1;
    private string _title = "Test Issue";
    private List<string> _labels = new();
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;
    private string _htmlUrl = "https://github.com/AskmethatFR/agent-dashboard/issues/1";
    private DateTimeOffset _updatedAt = DateTimeOffset.UtcNow;
    private DateTimeOffset? _closedAt;

    public GitHubIssueRecordBuilder WithNumber(long number)
    {
        _number = number;
        return this;
    }

    public GitHubIssueRecordBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public GitHubIssueRecordBuilder WithLabels(params string[] labels)
    {
        _labels = new List<string>(labels);
        return this;
    }

    public GitHubIssueRecordBuilder WithLabels(List<string> labels)
    {
        _labels = labels;
        return this;
    }

    public GitHubIssueRecordBuilder WithLabels(IEnumerable<string> labels)
    {
        _labels = new List<string>(labels);
        return this;
    }

    public GitHubIssueRecordBuilder WithCreatedAt(DateTimeOffset createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public GitHubIssueRecordBuilder WithHtmlUrl(string htmlUrl)
    {
        _htmlUrl = htmlUrl;
        return this;
    }

    public GitHubIssueRecordBuilder WithUpdatedAt(DateTimeOffset updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    public GitHubIssueRecordBuilder WithClosedAt(DateTimeOffset? closedAt)
    {
        _closedAt = closedAt;
        return this;
    }

    public GitHubIssueRecordBuilder AsOpen()
    {
        _closedAt = null;
        return this;
    }

    public GitHubIssueRecordBuilder AsClosed(DateTimeOffset closedAt)
    {
        _closedAt = closedAt;
        return this;
    }

    public GitHubIssueRecord Build()
    {
        return new GitHubIssueRecord(
            _number,
            _title,
            _labels,
            _createdAt,
            _htmlUrl,
            _updatedAt,
            _closedAt);
    }
}
