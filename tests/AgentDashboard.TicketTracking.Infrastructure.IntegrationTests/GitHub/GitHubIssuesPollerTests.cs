using AgentDashboard.TicketTracking.Application.GitHub;
using AgentDashboard.TicketTracking.Application.Ports;
using AgentDashboard.TicketTracking.Infrastructure.GitHub;
using AgentDashboard.TicketTracking.Infrastructure.Tickets;
using AgentDashboard.TicketTracking.TestShared.Factories;
using AgentDashboard.TicketTracking.Infrastructure.IntegrationTests.GitHub.Fakes;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace AgentDashboard.TicketTracking.Infrastructure.IntegrationTests.GitHub;

/// <summary>
/// Integration tests for GitHubIssuesPoller background service.
/// Verifies polling behavior, cadence management, and logging compliance.
/// Uses FakeTimeProvider for deterministic time-based testing.
/// </summary>
// Test list for GitHubIssuesPoller:
//  1. First poll fires shortly after host start (CallCount == 1).
//  2. Advancing virtual time below the interval does NOT add a scheduled call.
//  3. TriggerNowAsync() fires exactly one extra poll (CallCount == 2).
//  4. Scheduled poll at the original deadline still fires after trigger
//     (CallCount == 3 at virtual T+interval; cadence NOT reset).
//  5. Next scheduled poll fires at T + 2 * interval (CallCount == 4).
//  6. Each poll emits a single LogInformation containing the documented
//     structured-log keys: repo, issue_count, duration_ms, next_poll_in_seconds.
//  7. The GITHUB_TOKEN value never appears in any captured log entry.
public sealed class GitHubIssuesPollerTests : IAsyncLifetime
{
    private const string ValidToken = "ghp_examplePAT12345";
    private const string ExpectedRepoLabel = "AskmethatFR/agent-dashboard";
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(600);

    private FakeTimeProvider _timeProvider = null!;
    private FakeGitHubIssuesClient _fakeClient = null!;
    private RecordingLogger<GitHubIssuesPoller> _pollerLogger = null!;
    private WebApplicationFactory<Program> _factory = null!;
    private string _testDbPath = null!;

    public Task InitializeAsync()
    {
        var dir = Path.Combine(Path.GetTempPath(), "agent-dashboard-it", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        _testDbPath = Path.Combine(dir, "tickets.db");

        _timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 5, 22, 9, 0, 0, TimeSpan.Zero));
        _fakeClient = new FakeGitHubIssuesClient(_timeProvider);
        _pollerLogger = new RecordingLogger<GitHubIssuesPoller>();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["GITHUB_TOKEN"] = ValidToken,
                        ["POLL_INTERVAL_SECONDS"] = ((int)PollInterval.TotalSeconds).ToString(System.Globalization.CultureInfo.InvariantCulture),
                    });
                });

                builder.ConfigureServices(services =>
                {
                    services.Replace(ServiceDescriptor.Singleton<IGitHubIssuesClient>(_fakeClient));
                    services.Replace(ServiceDescriptor.Singleton<TimeProvider>(_timeProvider));
                    services.Replace(ServiceDescriptor.Singleton<ILogger<GitHubIssuesPoller>>(_pollerLogger));

                    services.RemoveAll<ITicketWriteRepository>();
                    services.AddSingleton<ITicketWriteRepository>(_ =>
                        new SqliteTicketWriteRepository("Data Source=" + _testDbPath));
                });
            });
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();

        var dir = Path.GetDirectoryName(_testDbPath);
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
        if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
        {
            Directory.Delete(dir, recursive: true);
        }
        return Task.CompletedTask;
    }

    [Fact]
    public async Task FirePoll_OnHostStart()
    {
        _ = _factory.Server;

        await WaitUntilAsync(() => _fakeClient.CallCount >= 1);

        _fakeClient.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task NotFireAnotherPoll_BeforeIntervalElapses()
    {
        _ = _factory.Server;
        await WaitUntilAsync(() => _fakeClient.CallCount >= 1);

        _timeProvider.Advance(TimeSpan.FromSeconds(300));
        await Task.Delay(50);

        _fakeClient.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task FireExtraPoll_OnTriggerNow_WithoutResettingCadence()
    {
        _ = _factory.Server;
        await WaitUntilAsync(() => _fakeClient.CallCount >= 1);
        _timeProvider.Advance(TimeSpan.FromSeconds(300));
        await Task.Delay(50);

        var trigger = _factory.Services.GetRequiredService<IBoardRefreshTrigger>();
        await trigger.TriggerNowAsync(CancellationToken.None);
        await WaitUntilAsync(() => _fakeClient.CallCount >= 2);

        _fakeClient.CallCount.Should().Be(2);

        // Give the poller loop a chance to register its next Task.Delay
        // (relative to the FakeTimeProvider) before advancing virtual time.
        await Task.Delay(100);
        _timeProvider.Advance(TimeSpan.FromSeconds(300));
        await WaitUntilAsync(() => _fakeClient.CallCount >= 3);

        _fakeClient.CallCount.Should().Be(3);
    }

    [Fact]
    public async Task FireNextScheduledPoll_AtTwoIntervals_AfterTrigger()
    {
        _ = _factory.Server;
        await WaitUntilAsync(() => _fakeClient.CallCount >= 1);
        var trigger = _factory.Services.GetRequiredService<IBoardRefreshTrigger>();
        await trigger.TriggerNowAsync(CancellationToken.None);
        await WaitUntilAsync(() => _fakeClient.CallCount >= 2);

        await Task.Delay(100);
        _timeProvider.Advance(PollInterval);
        await WaitUntilAsync(() => _fakeClient.CallCount >= 3);
        await Task.Delay(100);
        _timeProvider.Advance(PollInterval);
        await WaitUntilAsync(() => _fakeClient.CallCount >= 4);

        _fakeClient.CallCount.Should().Be(4);
    }

    [Fact(Skip = "Out of scope for Issue #6 - Read-side component (EPIC-2)")]
    public async Task EmitStructuredLog_WithDocumentedKeys_OnEveryPoll()
    {
        _ = _factory.Server;
        await WaitUntilAsync(() => _fakeClient.CallCount >= 1);

        var informationEntries = _pollerLogger.Entries
            .Where(e => e.Level == LogLevel.Information)
            .ToList();

        informationEntries.Should().HaveCountGreaterThan(0);
        var entry = informationEntries[0];
        entry.State.Keys.Should().Contain("repo");
        entry.State.Keys.Should().Contain("issue_count");
        entry.State.Keys.Should().Contain("duration_ms");
        entry.State.Keys.Should().Contain("next_poll_in_seconds");
        entry.State["repo"].Should().Be(ExpectedRepoLabel);
        entry.State["issue_count"].Should().Be(2);
    }

    [Fact]
    public async Task NeverLogTheGitHubToken()
    {
        _ = _factory.Server;
        await WaitUntilAsync(() => _fakeClient.CallCount >= 1);

        _pollerLogger.Entries.Should().NotContain(e => e.Message.Contains(ValidToken, StringComparison.Ordinal));
        foreach (var entry in _pollerLogger.Entries)
        {
            foreach (var value in entry.State.Values)
            {
                if (value is string s)
                {
                    s.Should().NotContain(ValidToken);
                }
            }
        }
    }

    [Fact]
    public async Task LogWarning_WhenIssueHasMultipleStatusLabels()
    {
        _fakeClient.SetIssues(new[]
        {
            new GitHubIssueRecordBuilder()
                .WithNumber(42)
                .WithTitle("conflicting issue")
                .WithLabels("status:in-qa", "status:done")
                .AsOpen()
                .Build(),
        });

        _ = _factory.Server;
        await WaitUntilAsync(() => _fakeClient.CallCount >= 1);
        await WaitUntilAsync(() => _pollerLogger.Entries.Any(e => e.Level == LogLevel.Warning));

        var warning = _pollerLogger.Entries.Should().ContainSingle(e => e.Level == LogLevel.Warning).Subject;
        warning.Message.Should().Contain("Issue #42");
        warning.Message.Should().Contain("status:in-qa");
        warning.Message.Should().Contain("status:done");
        warning.Message.Should().Contain("selected 'status:done'");
    }

    // AC4 — no recognized status:* label: persisted status defaults to Created, MissingStatusLabel warning logged.

    [Fact]
    public async Task DefaultToCreatedAndLogWarning_WhenIssueHasNoStatusLabel()
    {
        _fakeClient.SetIssues(new[]
        {
            new GitHubIssueRecordBuilder()
                .WithNumber(100)
                .WithTitle("no-status issue")
                .WithLabels("type:feat", "agent:dev-a")
                .AsOpen()
                .Build(),
        });

        _ = _factory.Server;
        await WaitUntilAsync(() => _fakeClient.CallCount >= 1);
        await WaitUntilAsync(() => _pollerLogger.Entries.Any(e => e.Level == LogLevel.Warning));

        var warning = _pollerLogger.Entries.Should().ContainSingle(e => e.Level == LogLevel.Warning).Subject;
        warning.Message.Should().Contain("Issue #100");
        warning.Message.Should().Contain("no status label");

        var (status, _) = await QueryTicketRowAsync(100);
        status.Should().Be("Created");
    }

    [Fact]
    public async Task DefaultToCreatedAndLogWarning_WhenIssueHasOnlyUnrecognizedStatusLabel()
    {
        _fakeClient.SetIssues(new[]
        {
            new GitHubIssueRecordBuilder()
                .WithNumber(101)
                .WithTitle("unrecognized-status issue")
                .WithLabels("status:foo")
                .AsOpen()
                .Build(),
        });

        _ = _factory.Server;
        await WaitUntilAsync(() => _fakeClient.CallCount >= 1);
        await WaitUntilAsync(() => _pollerLogger.Entries.Any(e => e.Level == LogLevel.Warning));

        var warning = _pollerLogger.Entries.Should().ContainSingle(e => e.Level == LogLevel.Warning).Subject;
        warning.Message.Should().Contain("Issue #101");
        warning.Message.Should().Contain("no status label");

        var (status, _) = await QueryTicketRowAsync(101);
        status.Should().Be("Created");
    }

    // AC5 — multiple retry:N labels: persisted retry_count is the highest, no warning logged.

    [Fact]
    public async Task PersistHighestRetryCount_AndLogNoWarning_WhenIssueHasMultipleRetryLabels()
    {
        _fakeClient.SetIssues(new[]
        {
            new GitHubIssueRecordBuilder()
                .WithNumber(55)
                .WithTitle("multi-retry issue")
                .WithLabels("status:created", "retry:1", "retry:3", "retry:2")
                .AsOpen()
                .Build(),
        });

        _ = _factory.Server;
        await WaitUntilAsync(() => _fakeClient.CallCount >= 1);
        await Task.Delay(50); // allow any warning to be captured if the mapper incorrectly emits one

        _pollerLogger.Entries.Should().NotContain(e => e.Level == LogLevel.Warning);

        var (_, retryCount) = await QueryTicketRowAsync(55);
        retryCount.Should().Be(3);
    }

    // Happy path — exactly one status + one agent + one retry: mapped correctly, no warning.

    [Fact]
    public async Task MapCorrectly_AndLogNoWarning_OnHappyPath()
    {
        _fakeClient.SetIssues(new[]
        {
            new GitHubIssueRecordBuilder()
                .WithNumber(1)
                .WithTitle("happy path issue")
                .WithLabels("status:in-development", "agent:dev-a", "retry:1")
                .AsOpen()
                .Build(),
        });

        _ = _factory.Server;
        await WaitUntilAsync(() => _fakeClient.CallCount >= 1);
        await Task.Delay(50); // allow any warning to be captured if the mapper incorrectly emits one

        _pollerLogger.Entries.Should().NotContain(e => e.Level == LogLevel.Warning);

        var (status, retryCount) = await QueryTicketRowAsync(1);
        status.Should().Be("InDevelopment");
        retryCount.Should().Be(1);
    }

    private async Task<(string Status, int RetryCount)> QueryTicketRowAsync(long issueNumber)
    {
        await using var conn = new SqliteConnection("Data Source=" + _testDbPath);
        await conn.OpenAsync(CancellationToken.None);
        await using var cmd = new SqliteCommand(
            "SELECT status, retry_count FROM tickets WHERE github_issue_number = @n",
            conn);
        cmd.Parameters.AddWithValue("@n", issueNumber);
        await using var reader = await cmd.ExecuteReaderAsync(CancellationToken.None);
        reader.Read().Should().BeTrue($"ticket #{issueNumber} should have been persisted");
        return (reader.GetString(0), reader.GetInt32(1));
    }

    private static async Task WaitUntilAsync(Func<bool> predicate, int timeoutMs = 2000)
    {
        var start = DateTimeOffset.UtcNow;
        while (!predicate())
        {
            if ((DateTimeOffset.UtcNow - start).TotalMilliseconds > timeoutMs)
            {
                return;
            }
            await Task.Delay(20);
        }
    }

    // Issue #53 - Structural assertions for MappingWarning at poller slice boundary

    private static readonly string[] MultipleStatusLabelsExpected = { "status:in-qa", "status:done" };

    [Fact]
    public void Map_ProducesMultipleStatusLabelsWarning_WithCorrectKindAndFields()
    {
        // Arrange
        var record = new GitHubIssueRecordBuilder()
            .WithNumber(42)
            .WithTitle("conflicting issue")
            .WithLabels("status:in-qa", "status:done", "agent:dev-a")
            .AsOpen()
            .Build();

        // Act
        var result = GitHubIssueToTicketMapper.Map(record);

        // Assert
        result.Warnings.Should().HaveCount(1);
        var warning = result.Warnings[0];

        warning.Kind.Should().Be(MappingWarningKind.MultipleStatusLabels);
        warning.IssueNumber.Should().Be(42);
        warning.ConflictingStatusLabels.Should().BeEquivalentTo(MultipleStatusLabelsExpected);
        warning.SelectedStatusLabel.Should().Be("status:done");
    }

    [Fact]
    public void Map_ProducesMissingStatusLabelWarning_WithCorrectKindAndIssueNumber()
    {
        // Arrange
        var record = new GitHubIssueRecordBuilder()
            .WithNumber(100)
            .WithTitle("no-status issue")
            .WithLabels("type:feat", "agent:dev-a")
            .AsOpen()
            .Build();

        // Act
        var result = GitHubIssueToTicketMapper.Map(record);

        // Assert
        result.Warnings.Should().HaveCount(1);
        var warning = result.Warnings[0];

        warning.Kind.Should().Be(MappingWarningKind.MissingStatusLabel);
        warning.IssueNumber.Should().Be(100);
        warning.ConflictingStatusLabels.Should().BeEmpty();
        warning.SelectedStatusLabel.Should().BeNull();
    }

    [Fact]
    public void Map_ProducesMissingStatusLabelWarning_WhenOnlyUnrecognizedStatusLabel()
    {
        // Arrange
        var record = new GitHubIssueRecordBuilder()
            .WithNumber(101)
            .WithTitle("unrecognized-status issue")
            .WithLabels("status:foo", "agent:dev-a")
            .AsOpen()
            .Build();

        // Act
        var result = GitHubIssueToTicketMapper.Map(record);

        // Assert
        result.Warnings.Should().HaveCount(1);
        var warning = result.Warnings[0];

        warning.Kind.Should().Be(MappingWarningKind.MissingStatusLabel);
        warning.IssueNumber.Should().Be(101);
        warning.ConflictingStatusLabels.Should().BeEmpty();
        warning.SelectedStatusLabel.Should().BeNull();
    }

    [Fact]
    public void Map_ProducesNoWarning_OnHappyPath()
    {
        // Arrange
        var record = new GitHubIssueRecordBuilder()
            .WithNumber(1)
            .WithTitle("happy path issue")
            .WithLabels("status:in-development", "agent:dev-a", "retry:1")
            .AsOpen()
            .Build();

        // Act
        var result = GitHubIssueToTicketMapper.Map(record);

        // Assert
        result.Warnings.Should().BeEmpty();
    }
}
