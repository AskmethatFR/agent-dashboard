using AgentDashboard.Web.Components.Layout.TopBar;
using Bunit;
using FluentAssertions;
using Xunit;

namespace AgentDashboard.Web.Tests.Components.Layout.TopBar;

// Test list for BrandHeader:
//   1. Renders the .brand container with the team/ wordmark and a .brand-dot child.
public sealed class BrandHeaderShould
{
    [Fact]
    public void Should_RenderBrandWordmark_WithSquareDot()
    {
        using var ctx = new BunitContext();

        var cut = ctx.Render<BrandHeader>();

        var brand = cut.Find(".brand");
        brand.QuerySelector(".brand-dot").Should().NotBeNull();
        brand.TextContent.Should().Contain("team/");
    }
}
