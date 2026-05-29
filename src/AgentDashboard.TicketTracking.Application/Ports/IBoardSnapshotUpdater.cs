using AgentDashboard.TicketTracking.Application.GitHub;

namespace AgentDashboard.TicketTracking.Application.Ports;

/// <summary>
/// Port for updating board snapshots.
/// Implementations handle the caching strategy and persistence of board state.
/// </summary>
public interface IBoardSnapshotUpdater
{
    /// <summary>
    /// Updates the board snapshot with the given GitHub issue records.
    /// </summary>
    /// <param name="records">The GitHub issue records to map and cache.</param>
    /// <param name="asOf">The timestamp when the snapshot was taken.</param>
    void Update(IReadOnlyList<GitHubIssueRecord> records, DateTimeOffset asOf);
}
