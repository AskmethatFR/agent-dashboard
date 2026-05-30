using AgentDashboard.TicketTracking.Application.Ports;
using AgentDashboard.TicketTracking.Infrastructure.GitHub;
using AgentDashboard.TicketTracking.Infrastructure.IntegrationTests.GitHub.Fakes;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
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

    public Task InitializeAsync()
    {
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
                        ["DATA_PATH"] = Path.GetTempPath(),
                    });
                });

                builder.ConfigureServices(services =>
                {
                    services.Replace(ServiceDescriptor.Singleton<IGitHubIssuesClient>(_fakeClient));
                    services.Replace(ServiceDescriptor.Singleton<TimeProvider>(_timeProvider));
                    services.Replace(ServiceDescriptor.Singleton<ILogger<GitHubIssuesPoller>>(_pollerLogger));
                });
            });
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
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
}
