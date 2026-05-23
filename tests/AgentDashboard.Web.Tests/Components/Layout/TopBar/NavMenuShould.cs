using AgentDashboard.Web.Components.Layout.TopBar;
using Bunit;
using FluentAssertions;
using Xunit;

namespace AgentDashboard.Web.Tests.Components.Layout.TopBar;

// Test list for NavMenu:
//   1. Renders seven nav buttons in the canonical order
//      (Home, Team Board, Sessions, Replay, Agent, Flow, Escalations).
//   2. Exactly one entry has aria-current="page" and it is "Team Board".
//   3. The six placeholder entries are disabled and carry aria-disabled="true".
public sealed class NavMenuShould
{
    private static readonly string[] CanonicalEntries =
    [
        "Home",
        "Team Board",
        "Sessions",
        "Replay",
        "Agent",
        "Flow",
        "Escalations",
    ];

    [Fact]
    public void Should_RenderSevenEntries_InCanonicalOrder()
    {
        using var ctx = new BunitContext();

        var cut = ctx.Render<NavMenu>();

        var labels = cut.FindAll("nav button")
            .Select(b => b.TextContent.Trim())
            .ToArray();

        labels.Should().Equal(CanonicalEntries);
    }

    [Fact]
    public void Should_MarkOnlyTeamBoardAsActive()
    {
        using var ctx = new BunitContext();

        var cut = ctx.Render<NavMenu>();

        var current = cut.FindAll("nav button[aria-current='page']");
        current.Should().HaveCount(1);
        current[0].TextContent.Trim().Should().Be("Team Board");
    }

    [Fact]
    public void Should_DisableSixPlaceholderEntries()
    {
        using var ctx = new BunitContext();

        var cut = ctx.Render<NavMenu>();

        var disabled = cut.FindAll("nav button[disabled][aria-disabled='true']");
        disabled.Should().HaveCount(6);
    }
}
