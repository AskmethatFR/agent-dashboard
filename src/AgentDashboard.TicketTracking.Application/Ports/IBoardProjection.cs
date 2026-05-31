using AgentDashboard.TicketTracking.Application.Boards;
using AgentDashboard.TicketTracking.Application.GitHub;

namespace AgentDashboard.TicketTracking.Application.Ports;

public interface IBoardProjection
{
    BoardProjectionResult Project(IReadOnlyList<GitHubIssueRecord> records, DateTimeOffset asOf);
}
