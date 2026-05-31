// Integration proof for Decision 7 (ADR-014):
// A malformed-label record causes the board to degrade gracefully:
//  - cache is updated (not blank)
//  - valid tickets are present
//  - a warning is logged by BoardSnapshotUpdater (EventId 210)
//
// Test list:
//  [✓] Update_WithMalformedLabel_UpdatesCacheWithValidTicketsPresentAndLogsWarning (now + EventId 210 assertion — B4)
//  [✓] Update_WithOutOfRangeRetry_DoesNotThrowAndLogsWarning (B1 integration proof)

using AgentDashboard.TicketTracking.Application.Boards;
using AgentDashboard.TicketTracking.Application.GitHub;
using AgentDashboard.TicketTracking.Infrastructure.Boards;
using AgentDashboard.TicketTracking.TestShared.Factories;
using AgentDashboard.TicketTracking.Infrastructure.IntegrationTests.GitHub.Fakes;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AgentDashboard.TicketTracking.Infrastructure.IntegrationTests.Boards;

public sealed class BoardSnapshotUpdaterIntegrationTests
{
    private static readonly DateTimeOffset AsOf = new(2026, 5, 31, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Update_WithMalformedLabel_UpdatesCacheWithValidTicketsPresentAndLogsWarning()
    {
        // Arrange
        var recordingLogger = new RecordingLogger<BoardSnapshotUpdater>();
        var cache = new BoardSnapshotCache();
        var projection = new BoardProjection();
        var sut = new BoardSnapshotUpdater(projection, cache, recordingLogger);

        var records = new List<GitHubIssueRecord>
        {
            new GitHubIssueRecordBuilder()
                .WithNumber(1)
                .WithTitle("Valid issue")
                .WithLabels("status:created", "agent:pm")
                .WithCreatedAt(AsOf.AddHours(-1))
                .WithHtmlUrl("https://github.com/AskmethatFR/agent-dashboard/issues/1")
                .WithUpdatedAt(AsOf.AddHours(-1))
                .AsOpen()
                .Build(),
            new GitHubIssueRecordBuilder()
                .WithNumber(2)
                .WithTitle("Malformed issue")
                .WithLabels("status:foo", "agent:pm")
                .WithCreatedAt(AsOf.AddHours(-2))
                .WithHtmlUrl("https://github.com/AskmethatFR/agent-dashboard/issues/2")
                .WithUpdatedAt(AsOf.AddHours(-2))
                .AsOpen()
                .Build()
        };

        // Act
        var act = () => sut.Update(records, AsOf);

        // Assert: board updates without throwing
        act.Should().NotThrow();

        // Assert: cache is not blank — valid tickets are present
        var snapshot = cache.GetLatest();
        snapshot.Should().NotBeNull();
        snapshot!.Tickets.Should().HaveCount(2, "both records render even when one has a malformed label");

        // Assert: warning is logged (EventId 210, Level Warning, about issue #2)
        var warnings = recordingLogger.Entries
            .Where(e => e.Level == LogLevel.Warning)
            .ToList();
        warnings.Should().ContainSingle("exactly one malformed label warning expected");
        warnings[0].Message.Should().Contain("Issue #2");
        warnings[0].Message.Should().Contain("status:foo");
        warnings[0].Message.Should().Contain("ignored");
        warnings[0].EventId.Id.Should().Be(210, "BoardSnapshotUpdaterLog.ProjectionWarning uses EventId 210");
    }

    [Fact]
    public void Update_WithOutOfRangeRetry_DoesNotThrowAndLogsWarning()
    {
        // Arrange — B1: retry:4 used to throw ArgumentOutOfRangeException via new Retry(4)
        var recordingLogger = new RecordingLogger<BoardSnapshotUpdater>();
        var cache = new BoardSnapshotCache();
        var projection = new BoardProjection();
        var sut = new BoardSnapshotUpdater(projection, cache, recordingLogger);

        var records = new List<GitHubIssueRecord>
        {
            new GitHubIssueRecordBuilder()
                .WithNumber(1)
                .WithTitle("Valid issue")
                .WithLabels("status:created", "agent:pm")
                .WithCreatedAt(AsOf.AddHours(-1))
                .WithHtmlUrl("https://github.com/AskmethatFR/agent-dashboard/issues/1")
                .WithUpdatedAt(AsOf.AddHours(-1))
                .AsOpen()
                .Build(),
            new GitHubIssueRecordBuilder()
                .WithNumber(2)
                .WithTitle("Out-of-range retry issue")
                .WithLabels("status:created", "agent:pm", "retry:4")
                .WithCreatedAt(AsOf.AddHours(-2))
                .WithHtmlUrl("https://github.com/AskmethatFR/agent-dashboard/issues/2")
                .WithUpdatedAt(AsOf.AddHours(-2))
                .AsOpen()
                .Build()
        };

        // Act
        var act = () => sut.Update(records, AsOf);

        // Assert: board updates without throwing (was ArgumentOutOfRangeException before fix)
        act.Should().NotThrow();

        // Assert: both tickets render
        var snapshot = cache.GetLatest();
        snapshot.Should().NotBeNull();
        snapshot!.Tickets.Should().HaveCount(2);

        // Assert: warning logged with EventId 210
        var warningEntries = recordingLogger.Entries
            .Where(e => e.Level == LogLevel.Warning)
            .ToList();
        warningEntries.Should().ContainSingle();
        warningEntries[0].EventId.Id.Should().Be(210);
        warningEntries[0].Message.Should().Contain("retry:4");
    }
}
