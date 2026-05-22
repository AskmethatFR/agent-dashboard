using AgentDashboard.TicketTracking.Application;
using AgentDashboard.TicketTracking.Infrastructure;
using AgentDashboard.Web.Components.Pages;
using AngleSharp.Dom;
using Bunit;
using FluentAssertions;
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
        cut.WaitForState(() => cut.FindAll("h2").Count >= ExpectedColumnLabels.Length);

        var headings = cut.FindAll("h2")
            .Select(ExtractColumnLabel)
            .ToArray();

        headings.Should().Equal(ExpectedColumnLabels);
    }

    [Fact]
    public void Should_RenderCountPerColumn_MatchingSeed()
    {
        using var ctx = BuildContext();

        var cut = ctx.Render<Home>();
        cut.WaitForState(() => cut.FindAll("h2").Count >= ExpectedColumnLabels.Length);

        var counts = cut.FindAll("h2")
            .Select(ExtractColumnCount)
            .ToArray();

        counts.Should().Equal(ExpectedTicketCounts);
    }

    [Fact]
    public void Should_RenderBrandPlaceholder()
    {
        using var ctx = BuildContext();

        var cut = ctx.Render<Home>();
        cut.WaitForState(() => cut.FindAll("h2").Count >= ExpectedColumnLabels.Length);

        cut.Markup.Should().Contain("team/");
    }

    private static BunitContext BuildContext()
    {
        var ctx = new BunitContext();
        ctx.Services.AddTicketTrackingApplication();
        ctx.Services.AddTicketTrackingInfrastructure();
        return ctx;
    }

    private static string ExtractColumnLabel(IElement heading)
    {
        var raw = heading.TextContent.Trim();
        var openParen = raw.LastIndexOf('(');
        return openParen < 0 ? raw : raw[..openParen].TrimEnd();
    }

    private static int ExtractColumnCount(IElement heading)
    {
        var raw = heading.TextContent;
        var openParen = raw.LastIndexOf('(');
        var closeParen = raw.LastIndexOf(')');
        if (openParen < 0 || closeParen <= openParen)
            return -1;
        var inside = raw[(openParen + 1)..closeParen];
        return int.Parse(inside, System.Globalization.CultureInfo.InvariantCulture);
    }
}
