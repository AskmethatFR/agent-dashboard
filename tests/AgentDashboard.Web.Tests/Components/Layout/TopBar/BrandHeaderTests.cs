using AgentDashboard.Web.Components.Layout.TopBar;
using Bunit;
using FluentAssertions;
using Xunit;

namespace AgentDashboard.Web.Tests.Components.Layout.TopBar;

using AgentDashboard.Web.Tests.Infrastructure;

// Test list for BrandHeader:
//   1. Renders the .brand container with the team/ wordmark and a .brand-dot child.
public sealed class BrandHeaderTests : IClassFixture<BunitFixture>
{
    private readonly BunitFixture _ctx;

    public BrandHeaderTests(BunitFixture ctx) => _ctx = ctx;

    [Fact]
    public void Should_RenderBrandWordmark_WithSquareDot()
    {
        var cut = _ctx.Context.Render<BrandHeader>();

        var brand = cut.Find(".brand");
        brand.QuerySelector(".brand-dot").Should().NotBeNull();
        brand.TextContent.Should().Contain("team/");
    }
}
