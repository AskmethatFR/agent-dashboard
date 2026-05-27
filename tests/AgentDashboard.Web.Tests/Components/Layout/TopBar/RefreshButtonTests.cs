using AgentDashboard.TicketTracking.Application;
using AgentDashboard.TicketTracking.Application.Ports;
using AgentDashboard.TicketTracking.Domain.Boards;
using AgentDashboard.TicketTracking.Infrastructure.Boards;
using AgentDashboard.Web.Components.Layout.TopBar;
using AgentDashboard.Web.Store;
using Blazor.Redux;
using Blazor.Redux.Core;
using Blazor.Redux.Extensions;
using Blazor.Redux.Interfaces;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace AgentDashboard.Web.Tests.Components.Layout.TopBar;

public sealed class RefreshButtonTests
{
    private static readonly DateTimeOffset FixedStart =
        new(2026, 5, 22, 10, 30, 0, TimeSpan.Zero);

    private static readonly TimeSpan SettleTimeout = TimeSpan.FromSeconds(5);

    [Fact]
    public void Should_DispatchActionOnce_When_Clicked()
    {
        using var ctx = BuildContext(out _, out var reader);

        var cut = ctx.Render<RefreshButton>();
        cut.Find("button").Click();

        cut.WaitForState(() => reader.CallCount == 1, SettleTimeout);
    }

    [Fact]
    public void Should_ResetAriaBusy_AfterDispatch()
    {
        using var ctx = BuildContext(out _, out var reader);

        var cut = ctx.Render<RefreshButton>();
        cut.Find("button").Click();

        cut.WaitForState(() => reader.CallCount == 1, SettleTimeout);
        cut.Find("button").GetAttribute("aria-busy").Should().Be("false");
    }

    [Fact]
    public void Should_DebounceSecondClickWithin_OneSecond()
    {
        using var ctx = BuildContext(out var time, out var reader);

        var cut = ctx.Render<RefreshButton>();

        cut.Find("button").Click();
        cut.WaitForState(() => reader.CallCount == 1, SettleTimeout);

        cut.Find("button").Click();
        reader.CallCount.Should().Be(1);

        time.Advance(TimeSpan.FromSeconds(1));
        cut.Find("button").Click();

        cut.WaitForState(() => reader.CallCount == 2, SettleTimeout);
    }

    private static BunitContext BuildContext(out FakeTimeProvider time, out CountingBoardReader reader)
    {
        var ctx = new BunitContext();
        time = new FakeTimeProvider(FixedStart);
        reader = new CountingBoardReader(new StubBoardReader());

        ctx.Services.AddSingleton<TimeProvider>(time);
        ctx.Services.AddTicketTrackingApplication();
        ctx.Services.AddSingleton<IBoardReader>(reader);

        ctx.Services.AddBlazorRedux(new BlazorReduxOption
        {
            Slices = [BoardSlice.Initial],
            ReplayLastAction = false,
            SnapshotStrategy = SnapshotStrategy.DeepCopy,
            EffectsCancellationStrategy = EffectsCancellationStrategy.None,
        });

        ctx.Services.AddScoped<IReducer<BoardSlice, LoadBoardAction>, LoadBoardReducer>();
        ctx.Services.AddScoped<IReducer<BoardSlice, LoadBoardSuccessAction>, LoadBoardSuccessReducer>();
        ctx.Services.AddScoped<IReducer<BoardSlice, LoadBoardFailureAction>, LoadBoardFailureReducer>();
        ctx.Services.AddScoped<IAsyncReducer<BoardSlice, LoadBoardAction>, LoadBoardAsyncReducer>();

        return ctx;
    }

    private sealed class CountingBoardReader : IBoardReader
    {
        private readonly IBoardReader _inner;
        private int _callCount;

        public CountingBoardReader(IBoardReader inner)
        {
            _inner = inner;
        }

        public int CallCount => Volatile.Read(ref _callCount);

        public Task<BoardSnapshot> GetCurrentAsync(CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _callCount);
            return _inner.GetCurrentAsync(cancellationToken);
        }
    }
}
