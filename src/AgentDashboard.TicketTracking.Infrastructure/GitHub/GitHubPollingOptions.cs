namespace AgentDashboard.TicketTracking.Infrastructure.GitHub;

public sealed class GitHubPollingOptions
{
    private const string DogfoodingRepositoryOwner = "AskmethatFR";
    private const string DogfoodingRepositoryName = "agent-dashboard";

    internal required string Token { get; init; }
    public required string RepositoryOwner { get; init; }
    public required string RepositoryName { get; init; }
    public required TimeSpan PollInterval { get; init; }

    internal static GitHubPollingOptions ForDogfooding(string token, TimeSpan pollInterval) => new()
    {
        Token = token,
        RepositoryOwner = DogfoodingRepositoryOwner,
        RepositoryName = DogfoodingRepositoryName,
        PollInterval = pollInterval,
    };
}
