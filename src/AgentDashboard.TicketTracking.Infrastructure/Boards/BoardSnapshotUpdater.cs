using AgentDashboard.TicketTracking.Application.GitHub;
using AgentDashboard.TicketTracking.Application.Ports;

namespace AgentDashboard.TicketTracking.Infrastructure.Boards;

/// <summary>
/// Adapter that implements IBoardSnapshotUpdater using BoardSnapshotCache.
/// Maps GitHub issue records to board snapshots and updates the cache.
/// </summary>
internal sealed class BoardSnapshotUpdater : IBoardSnapshotUpdater
{
    private readonly BoardSnapshotCache _cache;

    public BoardSnapshotUpdater(BoardSnapshotCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public void Update(IReadOnlyList<GitHubIssueRecord> records, DateTimeOffset asOf)
    {
        var snapshot = GitHubBoardMapper.MapToBoardSnapshot(records, asOf);
        _cache.Update(snapshot, asOf);
    }
}
