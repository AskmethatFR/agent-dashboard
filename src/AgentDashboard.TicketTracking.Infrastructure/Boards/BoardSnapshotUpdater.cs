using AgentDashboard.TicketTracking.Application.Boards;
using AgentDashboard.TicketTracking.Application.GitHub;
using AgentDashboard.TicketTracking.Application.Ports;
using AgentDashboard.TicketTracking.Infrastructure.GitHub;
using Microsoft.Extensions.Logging;

namespace AgentDashboard.TicketTracking.Infrastructure.Boards;

internal sealed class BoardSnapshotUpdater : IBoardSnapshotUpdater
{
    private readonly IBoardProjection _projection;
    private readonly BoardSnapshotCache _cache;
    private readonly ILogger<BoardSnapshotUpdater> _logger;

    public BoardSnapshotUpdater(
        IBoardProjection projection,
        BoardSnapshotCache cache,
        ILogger<BoardSnapshotUpdater> logger)
    {
        _projection = projection ?? throw new ArgumentNullException(nameof(projection));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Update(IReadOnlyList<GitHubIssueRecord> records, DateTimeOffset asOf)
    {
        var result = _projection.Project(records, asOf);
        foreach (var w in result.Warnings)
        {
            BoardSnapshotUpdaterLog.ProjectionWarning(_logger, GitHubLogSanitizer.Sanitize(FormatWarning(w)));
        }
        _cache.Update(result.Snapshot, asOf);
    }

    private static string FormatWarning(ProjectionWarning w) =>
        $"Issue #{w.IssueNumber}: malformed label '{w.OffendingLabel}' ignored; category defaulted.";
}

internal static partial class BoardSnapshotUpdaterLog
{
    private const int ProjectionWarningEventId = 210;

    [LoggerMessage(
        EventId = ProjectionWarningEventId,
        Level = LogLevel.Warning,
        Message = "{projection_warning}")]
    public static partial void ProjectionWarning(ILogger logger, string projection_warning);
}
