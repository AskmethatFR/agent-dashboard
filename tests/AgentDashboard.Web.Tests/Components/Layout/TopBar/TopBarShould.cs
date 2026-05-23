using AgentDashboard.TicketTracking.Application.Ports;
using AgentDashboard.Web.Tests.Components.Layout.TopBar.Fakes;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Xunit;
using TopBarComponent = AgentDashboard.Web.Components.Layout.TopBar.TopBar;

namespace AgentDashboard.Web.Tests.Components.Layout.TopBar;

// Test list for TopBar:
//   1. Composition smoke — all four regions are present:
//      brand wordmark, primary nav, live clock, refresh button.
public sealed class TopBarShould
{
    [Fact]
    public void Should_RenderAllFourRegions()
    {
        using var ctx = BuildContext();

        var cut = ctx.Render<TopBarComponent>();

        cut.Find(".brand").Should().NotBeNull();
        cut.Find("nav[aria-label='primary']").Should().NotBeNull();
        cut.Find(".live-clock").Should().NotBeNull();
        cut.Find("button[aria-label='Refresh tickets now']").Should().NotBeNull();
    }

    private static BunitContext BuildContext()
    {
        var ctx = new BunitContext();
        ctx.Services.AddSingleton<TimeProvider>(
            new FakeTimeProvider(new DateTimeOffset(2026, 5, 22, 10, 30, 0, TimeSpan.Zero)));
        ctx.Services.AddSingleton<IBoardRefreshTrigger>(new FakeBoardRefreshTrigger());
        return ctx;
    }
}
