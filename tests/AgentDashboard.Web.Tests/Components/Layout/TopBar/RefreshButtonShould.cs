using AgentDashboard.TicketTracking.Application.Ports;
using AgentDashboard.Web.Components.Layout.TopBar;
using AgentDashboard.Web.Tests.Components.Layout.TopBar.Fakes;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace AgentDashboard.Web.Tests.Components.Layout.TopBar;

// Test list for RefreshButton:
//   1. Click calls IBoardRefreshTrigger.TriggerNowAsync exactly once.
//   2. While the trigger is in-flight, the button advertises aria-busy="true";
//      once the trigger completes, aria-busy returns to "false".
//   3. A second click within 1s of a successful trigger is debounced
//      (CallCount stays at 1). After 1s elapses on the fake provider,
//      a further click is accepted (CallCount == 2).
//   4. When the trigger throws, the button enters the error state
//      (.is-error class + title carries the message); after 3s elapse
//      on the fake provider, the error indicator clears.
public sealed class RefreshButtonShould
{
    private static readonly DateTimeOffset FixedStart =
        new(2026, 5, 22, 10, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Should_CallTriggerOnce_When_Clicked()
    {
        using var ctx = BuildContext(out _, out var trigger);

        var cut = ctx.Render<RefreshButton>();
        cut.Find("button").Click();

        cut.WaitForState(() => trigger.CallCount == 1);
        trigger.CallCount.Should().Be(1);
    }

    [Fact]
    public void Should_SetAriaBusy_While_TriggerInFlight()
    {
        using var ctx = BuildContext(out _, out var trigger);
        trigger.Gate = new TaskCompletionSource();

        var cut = ctx.Render<RefreshButton>();
        cut.Find("button").Click();

        cut.WaitForState(() => cut.Find("button").GetAttribute("aria-busy") == "true");

        trigger.Gate.SetResult();

        cut.WaitForState(() => cut.Find("button").GetAttribute("aria-busy") == "false");
    }

    [Fact]
    public void Should_DebounceSecondClickWithin_OneSecond()
    {
        using var ctx = BuildContext(out var time, out var trigger);

        var cut = ctx.Render<RefreshButton>();
        cut.Find("button").Click();
        cut.WaitForState(() => trigger.CallCount == 1);

        cut.Find("button").Click();
        trigger.CallCount.Should().Be(1);

        time.Advance(TimeSpan.FromSeconds(1));
        cut.Find("button").Click();

        cut.WaitForState(() => trigger.CallCount == 2);
        trigger.CallCount.Should().Be(2);
    }

    [Fact]
    public void Should_ShowErrorIndicator_When_TriggerThrows()
    {
        using var ctx = BuildContext(out var time, out var trigger);
        trigger.ToThrow = new InvalidOperationException("boom");

        var cut = ctx.Render<RefreshButton>();
        cut.Find("button").Click();

        cut.WaitForState(() => cut.Find("button").ClassList.Contains("is-error"));
        cut.Find("button").GetAttribute("title").Should().Contain("boom");

        time.Advance(TimeSpan.FromSeconds(3));

        cut.WaitForState(() => !cut.Find("button").ClassList.Contains("is-error"));
    }

    private static BunitContext BuildContext(out FakeTimeProvider time, out FakeBoardRefreshTrigger trigger)
    {
        var ctx = new BunitContext();
        time = new FakeTimeProvider(FixedStart);
        trigger = new FakeBoardRefreshTrigger();
        ctx.Services.AddSingleton<TimeProvider>(time);
        ctx.Services.AddSingleton<IBoardRefreshTrigger>(trigger);
        return ctx;
    }
}
