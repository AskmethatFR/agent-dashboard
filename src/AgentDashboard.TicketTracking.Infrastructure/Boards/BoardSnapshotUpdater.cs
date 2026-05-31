using AgentDashboard.TicketTracking.Application.GitHub;
using AgentDashboard.TicketTracking.Application.Ports;

namespace AgentDashboard.TicketTracking.Infrastructure.Boards;

/// <summary>
/// Adapter that implements IBoardSnapshotUpdater using BoardSnapshotCache.
/// Projects GitHub issue records to board snapshots and updates the cache.
/// </summary>
internal sealed class BoardSnapshotUpdater : IBoardSnapshotUpdater
{
    private readonly IBoardProjection _projection;
    private readonly BoardSnapshotCache _cache;

    public BoardSnapshotUpdater(IBoardProjection projection, BoardSnapshotCache cache)
    {
        _projection = projection ?? throw new ArgumentNullException(nameof(projection));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public void Update(IReadOnlyList<GitHubIssueRecord> records, DateTimeOffset asOf)
    {
        var snapshot = _projection.Project(records, asOf);
        _cache.Update(snapshot, asOf);
    }
}
