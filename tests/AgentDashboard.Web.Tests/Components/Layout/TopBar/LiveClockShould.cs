using AgentDashboard.Web.Components.Layout.TopBar;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace AgentDashboard.Web.Tests.Components.Layout.TopBar;

// Test list for LiveClock:
//   1. Renders initial UTC time on first render based on the injected TimeProvider.
//   2. Updates rendered time when one second elapses on the fake provider.
public sealed class LiveClockShould
{
    private static readonly DateTimeOffset FixedStart =
        new(2026, 5, 22, 10, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Should_RenderInitialUtcTime_OnFirstRender()
    {
        using var ctx = BuildContext(out _);

        var cut = ctx.Render<LiveClock>();

        cut.Markup.Should().Contain("10:30:00 UTC");
    }

    [Fact]
    public void Should_UpdateRenderedTime_When_OneSecondElapses()
    {
        using var ctx = BuildContext(out var time);

        var cut = ctx.Render<LiveClock>();

        time.Advance(TimeSpan.FromSeconds(1));

        cut.WaitForState(() => cut.Markup.Contains("10:30:01 UTC"));
        cut.Markup.Should().Contain("10:30:01 UTC");
    }

    private static BunitContext BuildContext(out FakeTimeProvider time)
    {
        var ctx = new BunitContext();
        time = new FakeTimeProvider(FixedStart);
        ctx.Services.AddSingleton<TimeProvider>(time);
        return ctx;
    }
}
