using AgentDashboard.TicketTracking.Application.Boards;
using AgentDashboard.TicketTracking.Application.GitHub;
using AgentDashboard.TicketTracking.Application.Ports;
using AgentDashboard.TicketTracking.Infrastructure.Boards;

namespace AgentDashboard.TicketTracking.Infrastructure.UnitTests.Boards.Fakes;

internal sealed class FakeBoardSnapshotUpdater : IBoardSnapshotUpdater
{
    private readonly BoardSnapshotCache? _cache;
    private readonly bool _shouldUpdateCache;

    public FakeBoardSnapshotUpdater(BoardSnapshotCache? cache = null, bool shouldUpdateCache = true)
    {
        _cache = cache;
        _shouldUpdateCache = shouldUpdateCache;
    }

    public void Update(IReadOnlyList<GitHubIssueRecord> records, DateTimeOffset asOf)
    {
        if (_shouldUpdateCache && _cache is not null)
        {
            var result = new BoardProjection().Project(records, asOf);
            _cache.Update(result.Snapshot, asOf);
        }
    }
}
