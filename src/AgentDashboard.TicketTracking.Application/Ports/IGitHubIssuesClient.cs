using AgentDashboard.TicketTracking.Application.GitHub;

namespace AgentDashboard.TicketTracking.Application.Ports;

public interface IGitHubIssuesClient
{
    Task<IReadOnlyList<GitHubIssueRecord>> GetOpenIssuesAsync(CancellationToken cancellationToken);
}
