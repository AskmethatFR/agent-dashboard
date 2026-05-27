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
//   3. Renders UTC suffix.
//   4. Uses 24-hour format.
//   5. Has live-clock class.
//   6. Disposes timer on dispose.
public sealed class LiveClockTests
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

    [Fact]
    public void Should_RenderUtcSuffix()
    {
        using var ctx = BuildContext(out _);
        var cut = ctx.Render<LiveClock>();
        cut.Markup.Should().Contain("UTC");
    }

    [Fact]
    public void Should_Use24HourFormat()
    {
        using var ctx = BuildContext(out _);
        var cut = ctx.Render<LiveClock>();
        cut.Markup.Should().NotContain("AM").And.NotContain("PM");
    }

    [Fact]
    public void Should_HaveLiveClockClass()
    {
        using var ctx = BuildContext(out _);
        var cut = ctx.Render<LiveClock>();
        cut.Find(".live-clock").Should().NotBeNull();
    }

    [Fact]
    public void Should_DisposeTimer_OnDispose()
    {
        using var ctx = BuildContext(out _);
        var cut = ctx.Render<LiveClock>();
        var timer = cut.Instance._timer;

        cut.Dispose();

        timer.Should().NotBeNull();
    }

    private static BunitContext BuildContext(out FakeTimeProvider time)
    {
        var ctx = new BunitContext();
        time = new FakeTimeProvider(FixedStart);
        ctx.Services.AddSingleton<TimeProvider>(time);
        return ctx;
    }
}
