using AgentDashboard.TicketTracking.Application;
using AgentDashboard.TicketTracking.Infrastructure;
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

public class HomeShould
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
}
