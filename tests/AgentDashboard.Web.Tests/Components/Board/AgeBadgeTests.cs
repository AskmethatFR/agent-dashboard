using AgentDashboard.TicketTracking.Application.Queries.GetBoard.Dtos;
using AgentDashboard.Web.Components.Board;
using AgentDashboard.Web.Tests.Infrastructure;
using Bunit;
using FluentAssertions;
using Xunit;

namespace AgentDashboard.Web.Tests.Components.Board;

public sealed class AgeBadgeTests : IClassFixture<BunitFixture>
{
    private readonly BunitFixture _ctx;

    public AgeBadgeTests(BunitFixture ctx) => _ctx = ctx;

    // Test List (TDD by Example - Kent Beck's Test List technique)
    // ============================================================
    // Happy paths:
    // 1. Fresh state: renders ✦ ⏱ {age} with age-badge age-badge--fresh classes
    // 2. Neutral state: renders ⏱ {age} with age-badge class only
    // 3. Stale state: renders ⏱ {age} · zZz with age-badge age-badge--warn classes
    //
    // Boundaries/Edge cases:
    // 4. Age formatting: uses hh:mm format
    // 5. Empty/zero age: renders correctly
    // 6. Long duration: renders correctly
    //
    // Invalid/state transitions:
    // 7. Unknown Freshness value: falls back to neutral
    //
    // CSS classes:
    // 8. Fresh: has age-badge and age-badge--fresh
    // 9. Neutral: has only age-badge
    // 10. Stale: has age-badge and age-badge--warn

    [Fact]
    public void RenderSparklePrefix_When_FreshnessIsFresh()
    {
        var ticket = new TicketDto("Test", Age: TimeSpan.FromHours(1), Freshness: "Fresh");
        var cut = _ctx.Context.Render<AgeBadge>(p => p
            .Add(c => c.Age, ticket.Age)
            .Add(c => c.Freshness, ticket.Freshness));

        cut.Markup.Should().Contain("✦");
    }

    [Fact]
    public void RenderZzzSuffix_When_FreshnessIsStale()
    {
        var ticket = new TicketDto("Test", Age: TimeSpan.FromHours(1), Freshness: "Stale");
        var cut = _ctx.Context.Render<AgeBadge>(p => p
            .Add(c => c.Age, ticket.Age)
            .Add(c => c.Freshness, ticket.Freshness));

        cut.Markup.Should().Contain("· zZz");
    }

    [Fact]
    public void NotRenderSparkle_When_FreshnessIsNeutral()
    {
        var ticket = new TicketDto("Test", Age: TimeSpan.FromHours(1), Freshness: "Neutral");
        var cut = _ctx.Context.Render<AgeBadge>(p => p
            .Add(c => c.Age, ticket.Age)
            .Add(c => c.Freshness, ticket.Freshness));

        cut.Markup.Should().NotContain("✦");
    }

    [Fact]
    public void NotRenderZzz_When_FreshnessIsNeutral()
    {
        var ticket = new TicketDto("Test", Age: TimeSpan.FromHours(1), Freshness: "Neutral");
        var cut = _ctx.Context.Render<AgeBadge>(p => p
            .Add(c => c.Age, ticket.Age)
            .Add(c => c.Freshness, ticket.Freshness));

        cut.Markup.Should().NotContain("zZz");
    }

    [Fact]
    public void RenderClockIcon_ForAllStates()
    {
        var freshTicket = new TicketDto("Test", Age: TimeSpan.FromHours(1), Freshness: "Fresh");
        var neutralTicket = new TicketDto("Test", Age: TimeSpan.FromHours(1), Freshness: "Neutral");
        var staleTicket = new TicketDto("Test", Age: TimeSpan.FromHours(1), Freshness: "Stale");

        var freshCut = _ctx.Context.Render<AgeBadge>(p => p
            .Add(c => c.Age, freshTicket.Age)
            .Add(c => c.Freshness, freshTicket.Freshness));
        var neutralCut = _ctx.Context.Render<AgeBadge>(p => p
            .Add(c => c.Age, neutralTicket.Age)
            .Add(c => c.Freshness, neutralTicket.Freshness));
        var staleCut = _ctx.Context.Render<AgeBadge>(p => p
            .Add(c => c.Age, staleTicket.Age)
            .Add(c => c.Freshness, staleTicket.Freshness));

        freshCut.Markup.Should().Contain("⏱");
        neutralCut.Markup.Should().Contain("⏱");
        staleCut.Markup.Should().Contain("⏱");
    }

    [Fact]
    public void RenderAgeInHhMmFormat()
    {
        var ticket = new TicketDto("Test", Age: TimeSpan.FromHours(2).Add(TimeSpan.FromMinutes(30)), Freshness: "Neutral");
        var cut = _ctx.Context.Render<AgeBadge>(p => p
            .Add(c => c.Age, ticket.Age)
            .Add(c => c.Freshness, ticket.Freshness));

        cut.Markup.Should().Contain("02:30");
    }

    [Fact]
    public void HaveAgeBadgeClass_ForAllStates()
    {
        var freshTicket = new TicketDto("Test", Age: TimeSpan.FromHours(1), Freshness: "Fresh");
        var neutralTicket = new TicketDto("Test", Age: TimeSpan.FromHours(1), Freshness: "Neutral");
        var staleTicket = new TicketDto("Test", Age: TimeSpan.FromHours(1), Freshness: "Stale");

        var freshCut = _ctx.Context.Render<AgeBadge>(p => p
            .Add(c => c.Age, freshTicket.Age)
            .Add(c => c.Freshness, freshTicket.Freshness));
        var neutralCut = _ctx.Context.Render<AgeBadge>(p => p
            .Add(c => c.Age, neutralTicket.Age)
            .Add(c => c.Freshness, neutralTicket.Freshness));
        var staleCut = _ctx.Context.Render<AgeBadge>(p => p
            .Add(c => c.Age, staleTicket.Age)
            .Add(c => c.Freshness, staleTicket.Freshness));

        freshCut.Find(".age-badge").Should().NotBeNull();
        neutralCut.Find(".age-badge").Should().NotBeNull();
        staleCut.Find(".age-badge").Should().NotBeNull();
    }

    [Fact]
    public void HaveAgeBadgeFreshClass_When_FreshnessIsFresh()
    {
        var ticket = new TicketDto("Test", Age: TimeSpan.FromHours(1), Freshness: "Fresh");
        var cut = _ctx.Context.Render<AgeBadge>(p => p
            .Add(c => c.Age, ticket.Age)
            .Add(c => c.Freshness, ticket.Freshness));

        cut.Find(".age-badge--fresh").Should().NotBeNull();
    }

    [Fact]
    public void HaveAgeBadgeWarnClass_When_FreshnessIsStale()
    {
        var ticket = new TicketDto("Test", Age: TimeSpan.FromHours(1), Freshness: "Stale");
        var cut = _ctx.Context.Render<AgeBadge>(p => p
            .Add(c => c.Age, ticket.Age)
            .Add(c => c.Freshness, ticket.Freshness));

        cut.Find(".age-badge--warn").Should().NotBeNull();
    }

    [Fact]
    public void NotHaveFreshOrWarnClass_When_FreshnessIsNeutral()
    {
        var ticket = new TicketDto("Test", Age: TimeSpan.FromHours(1), Freshness: "Neutral");
        var cut = _ctx.Context.Render<AgeBadge>(p => p
            .Add(c => c.Age, ticket.Age)
            .Add(c => c.Freshness, ticket.Freshness));

        cut.Markup.Should().NotContain("age-badge--fresh");
        cut.Markup.Should().NotContain("age-badge--warn");
    }

    [Fact]
    public void RenderFullFreshBadge_When_FreshnessIsFresh()
    {
        var ticket = new TicketDto("Test", Age: TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(30)), Freshness: "Fresh");
        var cut = _ctx.Context.Render<AgeBadge>(p => p
            .Add(c => c.Age, ticket.Age)
            .Add(c => c.Freshness, ticket.Freshness));

        // Should contain: ✦ ⏱ 01:30
        cut.Markup.Should().Contain("✦");
        cut.Markup.Should().Contain("⏱");
        cut.Markup.Should().Contain("01:30");
        cut.Markup.Should().NotContain("zZz");
    }

    [Fact]
    public void RenderFullStaleBadge_When_FreshnessIsStale()
    {
        var ticket = new TicketDto("Test", Age: TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(30)), Freshness: "Stale");
        var cut = _ctx.Context.Render<AgeBadge>(p => p
            .Add(c => c.Age, ticket.Age)
            .Add(c => c.Freshness, ticket.Freshness));

        // Should contain: ⏱ 01:30 · zZz
        cut.Markup.Should().Contain("⏱");
        cut.Markup.Should().Contain("01:30");
        cut.Markup.Should().Contain("· zZz");
        cut.Markup.Should().NotContain("✦");
    }

    [Fact]
    public void RenderFullNeutralBadge_When_FreshnessIsNeutral()
    {
        var ticket = new TicketDto("Test", Age: TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(30)), Freshness: "Neutral");
        var cut = _ctx.Context.Render<AgeBadge>(p => p
            .Add(c => c.Age, ticket.Age)
            .Add(c => c.Freshness, ticket.Freshness));

        // Should contain: ⏱ 01:30 (no decorations)
        cut.Markup.Should().Contain("⏱");
        cut.Markup.Should().Contain("01:30");
        cut.Markup.Should().NotContain("✦");
        cut.Markup.Should().NotContain("zZz");
    }
}
