using AgentDashboard.TicketTracking.Domain.Boards;

namespace AgentDashboard.TicketTracking.Application.Boards;

public sealed record BoardProjectionResult(
    BoardSnapshot Snapshot,
    IReadOnlyList<ProjectionWarning> Warnings);
