namespace AgentDashboard.TicketTracking.Infrastructure.GitHub;

public sealed class GitHubPollingOptions
{
    public required string Token { get; init; }
    public required string RepositoryOwner { get; init; }
    public required string RepositoryName { get; init; }
    public required TimeSpan PollInterval { get; init; }
}
