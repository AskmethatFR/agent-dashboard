using AgentDashboard.TicketTracking.Application.GitHub;
using AgentDashboard.TicketTracking.Domain.Boards;

namespace AgentDashboard.TicketTracking.Application.Ports;

public interface IBoardProjection
{
    BoardSnapshot Project(IReadOnlyList<GitHubIssueRecord> records, DateTimeOffset asOf);
}
