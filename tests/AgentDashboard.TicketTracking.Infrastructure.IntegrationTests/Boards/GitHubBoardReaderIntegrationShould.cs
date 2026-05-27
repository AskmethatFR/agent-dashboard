using AgentDashboard.TicketTracking.Application.GitHub;
using AgentDashboard.TicketTracking.Application.Ports;
using AgentDashboard.TicketTracking.Application.Queries.GetBoard;
using AgentDashboard.TicketTracking.Application.Queries.GetBoard.Dtos;
using AgentDashboard.TicketTracking.Infrastructure.IntegrationTests.GitHub.Fakes;
using Cortex.Mediator;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace AgentDashboard.TicketTracking.Infrastructure.IntegrationTests.Boards;

public sealed class GitHubBoardReaderIntegrationShould : IAsyncLifetime
{
    private const string ValidToken = "ghp_examplePAT12345";
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(600);

    private FakeTimeProvider _timeProvider = null!;
    private FakeGitHubIssuesClient _fakeClient = null!;
    private WebApplicationFactory<Program> _factory = null!;
    private IServiceScope _scope = null!;
    private IMediator _mediator = null!;
    private IBoardRefreshTrigger _refreshTrigger = null!;

    public Task InitializeAsync()
    {
        _timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 5, 27, 9, 0, 0, TimeSpan.Zero));
        _fakeClient = new FakeGitHubIssuesClient(_timeProvider);

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
                });
            });

        // Create a scope to resolve scoped services
        _scope = _factory.Services.CreateScope();
        _mediator = _scope.ServiceProvider.GetRequiredService<IMediator>();
        _refreshTrigger = _scope.ServiceProvider.GetRequiredService<IBoardRefreshTrigger>();

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _scope?.Dispose();
        _factory?.Dispose();
        return Task.CompletedTask;
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

    // Test list for GitHubBoardReaderIntegrationShould:
    //  1. GetBoardQuery with GitHub data returns mapped board with 7 columns and tickets
    //  2. GetBoardQuery called twice returns cached data on second call
    //  3. GetBoardQuery after refresh trigger updates board with new data
    //  4. GetBoardQuery with all labels maps all ticket properties correctly

    [Fact]
    public async Task GetBoardQuery_WhenGitHubHasIssues_ReturnsMappedBoard()
    {
        // Arrange: Configure FakeGitHubIssuesClient to return controlled issues
        var issues = new List<GitHubIssueRecord>
        {
            new GitHubIssueRecord(1, "Test Issue", new List<string> { "status:created", "agent:pm" }, DateTimeOffset.UtcNow.AddHours(-1))
        };
        _fakeClient.SetIssues(issues);

        // Act: Execute GetBoardQuery via Mediator
        var result = await _mediator.SendQueryAsync<GetBoardQuery, BoardDto>(new GetBoardQuery());

        // Assert: Verify that the board contains 7 columns and 1 mapped ticket
        result.Columns.Should().HaveCount(7);
        
        // Find the ticket in the columns
        var allTickets = result.Columns.SelectMany(c => c.Tickets).ToList();
        allTickets.Should().HaveCount(1);
        allTickets[0].Id.Should().Be(1);
        allTickets[0].Title.Should().Be("Test Issue");
    }

    [Fact]
    public async Task GetBoardQuery_WhenCalledTwice_ReturnsCachedDataOnSecondCall()
    {
        // Arrange: First call fills the cache
        var issues = new List<GitHubIssueRecord>
        {
            new GitHubIssueRecord(1, "First Issue", new List<string> { "status:created", "agent:pm" }, DateTimeOffset.UtcNow.AddHours(-1))
        };
        _fakeClient.SetIssues(issues);
        
        var result1 = await _mediator.SendQueryAsync<GetBoardQuery, BoardDto>(new GetBoardQuery());

        // Act: Second call
        var result2 = await _mediator.SendQueryAsync<GetBoardQuery, BoardDto>(new GetBoardQuery());

        // Assert: Both results are equivalent (same content)
        var tickets1 = result1.Columns.SelectMany(c => c.Tickets).ToList();
        var tickets2 = result2.Columns.SelectMany(c => c.Tickets).ToList();
        tickets1.Should().BeEquivalentTo(tickets2);
    }

    [Fact]
    public async Task GetBoardQuery_AfterRefreshTrigger_UpdatesBoard()
    {
        // Arrange: Initial call with data A
        var initialIssues = new List<GitHubIssueRecord> 
        {
            new GitHubIssueRecord(1, "Issue A", new List<string> { "status:created", "agent:pm" }, DateTimeOffset.UtcNow)
        };
        _fakeClient.SetIssues(initialIssues);
        var result1 = await _mediator.SendQueryAsync<GetBoardQuery, BoardDto>(new GetBoardQuery());

        // Act: Change GitHub data and trigger refresh
        var updatedIssues = new List<GitHubIssueRecord> 
        {
            new GitHubIssueRecord(2, "Issue B", new List<string> { "status:created", "agent:pm" }, DateTimeOffset.UtcNow)
        };
        _fakeClient.SetIssues(updatedIssues);
        await _refreshTrigger.TriggerNowAsync(default);

        // Wait for poller to process
        await Task.Delay(100);

        // Act: Call after refresh
        var result2 = await _mediator.SendQueryAsync<GetBoardQuery, BoardDto>(new GetBoardQuery());

        // Assert: Board now contains Issue B
        var tickets2 = result2.Columns.SelectMany(c => c.Tickets).ToList();
        tickets2.Should().HaveCount(1);
        tickets2[0].Id.Should().Be(2);
        tickets2[0].Title.Should().Be("Issue B");
    }

    [Fact]
    public async Task GetBoardQuery_WhenIssueHasAllLabels_MapsAllProperties()
    {
        // Arrange: Issue with all labels
        var issues = new List<GitHubIssueRecord>
        {
            new GitHubIssueRecord(
                42,
                "Feature",
                new List<string> 
                {
                    "status:in-review", 
                    "agent:dev-a", 
                    "retry:2",
                    "escalation-target:pm",
                    "co-agent:dev-b"
                },
                DateTimeOffset.UtcNow.AddHours(-1))
        };
        _fakeClient.SetIssues(issues);

        // Act
        var result = await _mediator.SendQueryAsync<GetBoardQuery, BoardDto>(new GetBoardQuery());

        // Assert: Verify that all properties are mapped
        var allTickets = result.Columns.SelectMany(c => c.Tickets).ToList();
        allTickets.Should().HaveCount(1);
        
        var ticket = allTickets[0];
        ticket.Id.Should().Be(42);
        ticket.Title.Should().Be("Feature");
        ticket.AgentId.Should().Be("dev-a");
        ticket.RetryCount.Should().Be(2);
        ticket.IsEscalated.Should().BeFalse(); // retry:2 is < 3, so not escalated
        ticket.IsInCrossReview.Should().BeTrue();
        ticket.CoAgentId.Should().Be("dev-b");
    }
}
