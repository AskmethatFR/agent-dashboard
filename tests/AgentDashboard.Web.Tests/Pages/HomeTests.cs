using AgentDashboard.TicketTracking.Application;
using AgentDashboard.TicketTracking.Application.Ports;
using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Boards;
using AgentDashboard.TicketTracking.Domain.Tickets;
using AgentDashboard.TicketTracking.Infrastructure;
using AgentDashboard.TicketTracking.Infrastructure.Boards;
using AgentDashboard.Web.Components.Pages;
using AgentDashboard.Web.Store;
using AngleSharp.Dom;
using Bunit;
using Blazor.Redux;
using Blazor.Redux.Core;
using Blazor.Redux.Extensions;
using Blazor.Redux.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AgentDashboard.Web.Tests.Pages;

public class HomeTests
{
    private static readonly string[] ExpectedColumnLabels =
    [
        "Created",
        "Specified",
        "In Development",
        "In Review",
        "In Qa",
        "Awaiting Validation",
        "Done",
    ];

    private static readonly int[] ExpectedTicketCounts = [2, 1, 2, 1, 1, 0, 2];

    [Fact]
    public void Should_RenderSevenColumns_InCanonicalOrder()
    {
        using var ctx = BuildContext();

        var cut = ctx.Render<Home>();
        cut.WaitForState(() => cut.FindAll(".column-title").Count >= ExpectedColumnLabels.Length, TimeSpan.FromSeconds(5));

        var headings = cut.FindAll(".column-title")
            .Select(ExtractColumnLabel)
            .ToArray();

        headings.Should().Equal(ExpectedColumnLabels);
    }

    [Fact]
    public void Should_RenderCountPerColumn_MatchingSeed()
    {
        using var ctx = BuildContext();

        var cut = ctx.Render<Home>();
        cut.WaitForState(() => cut.FindAll(".column-title").Count >= ExpectedColumnLabels.Length, TimeSpan.FromSeconds(5));

        var counts = cut.FindAll(".column-title")
            .Select(ExtractColumnCount)
            .ToArray();

        counts.Should().Equal(ExpectedTicketCounts);
    }

    [Fact]
    public void Should_NotThrow_NullReferenceException_WhenBoardIsNull()
    {
        using var ctx = BuildContext();

        var cut = ctx.Render<Home>();

        cut.Markup.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Should_DisplayLoadingState_WhenBoardIsNull()
    {
        using var ctx = BuildContext();
        ctx.Services.AddSingleton<IBoardReader>(new LoadingStateBoardReader());

        var cut = ctx.Render<Home>();

        cut.Markup.Should().Contain("Loading...");
    }

    private static BunitContext BuildContext()
    {
        var ctx = new BunitContext();
        ctx.Services.AddTicketTrackingApplication();
        ctx.Services.AddTicketTrackingInfrastructure();
        
        // Configure Blazor.Redux
        ctx.Services.AddBlazorRedux(new BlazorReduxOption
        {
            Slices = [BoardSlice.Initial],
            ReplayLastAction = false,
            SnapshotStrategy = SnapshotStrategy.DeepCopy,
            EffectsCancellationStrategy = EffectsCancellationStrategy.None
        });

        ctx.Services.AddScoped<IReducer<BoardSlice, LoadBoardAction>, LoadBoardReducer>();
        ctx.Services.AddScoped<IReducer<BoardSlice, LoadBoardSuccessAction>, LoadBoardSuccessReducer>();
        ctx.Services.AddScoped<IReducer<BoardSlice, LoadBoardFailureAction>, LoadBoardFailureReducer>();
        ctx.Services.AddScoped<IAsyncReducer<BoardSlice, LoadBoardAction>, LoadBoardAsyncReducer>();

        ctx.Services.AddScoped<BoardCacheMonitor>();
        ctx.Services.AddHostedService<BoardCacheMonitorHostService>();

        // Seed the cache with test data so CachedBoardReader can return it
        var cache = new BoardSnapshotCache();
        cache.Update(SeedTestSnapshot(), DateTimeOffset.UtcNow);
        ctx.Services.AddSingleton(cache);

        return ctx;
    }

    private static string ExtractColumnLabel(IElement heading)
    {
        return heading.TextContent.Trim();
    }

    private static int ExtractColumnCount(IElement heading)
    {
        // New structure: <span class="column-title">Label</span><span class="column-meta"><span class="column-count">N</span></span>
        // heading is .column-title, we need to find the sibling .column-count
        var columnHeader = heading.Parent as IParentNode;
        if (columnHeader == null)
            return -1;

        var countSpan = columnHeader.QuerySelector<IElement>(".column-count");
        if (countSpan == null || !int.TryParse(countSpan.TextContent.Trim(), out var count))
            return -1;

        return count;
    }

    private static BoardSnapshot SeedTestSnapshot()
    {
        var column1 = new BoardColumn(new BoardColumnId("CREATED"), new BoardColumnLabel("Created"));
        var column2 = new BoardColumn(new BoardColumnId("SPECIFIED"), new BoardColumnLabel("Specified"));
        var column3 = new BoardColumn(new BoardColumnId("IN_DEV"), new BoardColumnLabel("In Development"));
        var column4 = new BoardColumn(new BoardColumnId("IN_REVIEW"), new BoardColumnLabel("In Review"));
        var column5 = new BoardColumn(new BoardColumnId("IN_QA"), new BoardColumnLabel("In Qa"));
        var column6 = new BoardColumn(new BoardColumnId("AWAITING_VALIDATION"), new BoardColumnLabel("Awaiting Validation"));
        var column7 = new BoardColumn(new BoardColumnId("DONE"), new BoardColumnLabel("Done"));
        var agent = new Agent(new AgentId("DA"), new AgentName("Developer A"), new AgentGlyph("DA"), new AgentRole("developer"));
        var tickets = new List<TicketSnapshot>();
        // Add tickets to match ExpectedTicketCounts: [2, 1, 2, 1, 1, 0, 2]
        // Created: 2 tickets
        tickets.Add(TicketSnapshot.Open(new TicketId(1), column1.Id, new TicketTitle("Ticket 1"), agent.Id, new Retry(0), new Age(TimeSpan.Zero), thinking: false, freshness: TicketFreshness.Neutral));
        tickets.Add(TicketSnapshot.Open(new TicketId(2), column1.Id, new TicketTitle("Ticket 2"), agent.Id, new Retry(0), new Age(TimeSpan.Zero), thinking: false, freshness: TicketFreshness.Neutral));
        // Specified: 1 ticket
        tickets.Add(TicketSnapshot.Open(new TicketId(3), column2.Id, new TicketTitle("Ticket 3"), agent.Id, new Retry(0), new Age(TimeSpan.Zero), thinking: false, freshness: TicketFreshness.Neutral));
        // In Development: 2 tickets
        tickets.Add(TicketSnapshot.Open(new TicketId(4), column3.Id, new TicketTitle("Ticket 4"), agent.Id, new Retry(0), new Age(TimeSpan.Zero), thinking: false, freshness: TicketFreshness.Neutral));
        tickets.Add(TicketSnapshot.Open(new TicketId(5), column3.Id, new TicketTitle("Ticket 5"), agent.Id, new Retry(0), new Age(TimeSpan.Zero), thinking: false, freshness: TicketFreshness.Neutral));
        // In Review: 1 ticket
        tickets.Add(TicketSnapshot.Open(new TicketId(6), column4.Id, new TicketTitle("Ticket 6"), agent.Id, new Retry(0), new Age(TimeSpan.Zero), thinking: false, freshness: TicketFreshness.Neutral));
        // In Qa: 1 ticket
        tickets.Add(TicketSnapshot.Open(new TicketId(7), column5.Id, new TicketTitle("Ticket 7"), agent.Id, new Retry(0), new Age(TimeSpan.Zero), thinking: false, freshness: TicketFreshness.Neutral));
        // Done: 2 tickets
        tickets.Add(TicketSnapshot.Open(new TicketId(8), column7.Id, new TicketTitle("Ticket 8"), agent.Id, new Retry(0), new Age(TimeSpan.Zero), thinking: false, freshness: TicketFreshness.Neutral));
        tickets.Add(TicketSnapshot.Open(new TicketId(9), column7.Id, new TicketTitle("Ticket 9"), agent.Id, new Retry(0), new Age(TimeSpan.Zero), thinking: false, freshness: TicketFreshness.Neutral));
        return new BoardSnapshot(
            new[] { column1, column2, column3, column4, column5, column6, column7 },
            tickets,
            new[] { agent });
    }

    private sealed class LoadingStateBoardReader : IBoardReader
    {
        public Task<BoardSnapshot> GetCurrentAsync(CancellationToken cancellationToken) =>
            new TaskCompletionSource<BoardSnapshot>().Task;
    }
}
