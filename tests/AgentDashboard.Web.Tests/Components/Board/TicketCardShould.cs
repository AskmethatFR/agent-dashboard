using AgentDashboard.TicketTracking.Application.Queries.GetBoard.Dtos;
using AgentDashboard.Web.Components.Board;
using AgentDashboard.Web.Components.Shared;
using AgentDashboard.Web.Tests.Infrastructure;
using Bunit;
using FluentAssertions;
using Xunit;

namespace AgentDashboard.Web.Tests.Components.Board;

public class TicketCardShould : IClassFixture<BunitFixture>
{
    private static readonly AgentDto DevADto = new(
        "dev-a", "Developer A", "DA", "developer");

    private static readonly AgentDto DevBDto = new(
        "dev-b", "Developer B", "DB", "developer");

    private static readonly TicketDto SampleTicket = new(
        "Fix the bug", "dev-a", false);

    private static readonly TicketDto ThinkingTicket = new(
        "Review PR", "dev-a", true);

    private static readonly IReadOnlyList<AgentDto> Agents = new List<AgentDto> { DevADto, DevBDto };

    private readonly BunitFixture _ctx;

    public TicketCardShould(BunitFixture ctx) => _ctx = ctx;

    [Fact]
    public void RenderTicketTitle()
    {
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, SampleTicket)
            .Add(c => c.Agents, Agents));

        cut.Markup.Should().Contain("Fix the bug");
    }

    [Fact]
    public void RenderAgentChipWithGlyphAndName()
    {
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, SampleTicket)
            .Add(c => c.Agents, Agents));

        cut.Markup.Should().Contain("DA");
        cut.Markup.Should().Contain("Developer A");
    }

    [Fact]
    public void PassIsThinkingTrueToAgentChip_WhenTicketIsThinking()
    {
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, ThinkingTicket)
            .Add(c => c.Agents, Agents));

        var agentChip = cut.FindComponent<AgentChip>();
        agentChip.Instance.IsThinking.Should().BeTrue();
    }

    [Fact]
    public void PassIsThinkingFalseToAgentChip_WhenTicketIsNotThinking()
    {
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, SampleTicket)
            .Add(c => c.Agents, Agents));

        var agentChip = cut.FindComponent<AgentChip>();
        agentChip.Instance.IsThinking.Should().BeFalse();
    }

    [Fact]
    public void RenderNothingForAgentChip_WhenAgentNotFound()
    {
        var unknownAgentTicket = new TicketDto("Orphan ticket", "unknown-id", false);
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, unknownAgentTicket)
            .Add(c => c.Agents, Agents));

        // AgentChip renders nothing when Agent is null - verify no agent markup is present
        cut.Markup.Should().NotContain("agent-glyph");
        cut.Markup.Should().NotContain("agent-name");
    }

    [Fact]
    public void Should_RenderRetryCounter_When_RetryCountGreaterThanZero()
    {
        var ticketWithRetry = new TicketDto("Fix the bug", "dev-a", false, RetryCount: 2);
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, ticketWithRetry)
            .Add(c => c.Agents, Agents));

        var retryCounter = cut.FindComponent<RetryCounter>();
        retryCounter.Should().NotBeNull();
        retryCounter.Instance.Current.Should().Be(2);
    }

    [Fact]
    public void Should_RenderRetryCounter_When_RetryCountIsZero()
    {
        var ticketNoRetry = new TicketDto("Fix the bug", "dev-a", false, RetryCount: 0);
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, ticketNoRetry)
            .Add(c => c.Agents, Agents));

        var retryCounter = cut.FindComponent<RetryCounter>();
        retryCounter.Should().NotBeNull();
        retryCounter.Instance.Current.Should().Be(0);
    }

    // =========================================================================
    // NEW TESTS FOR TICKET CARD BADGES FEATURE
    // =========================================================================

    // --- Card ID display tests ---

    [Fact]
    public void Should_RenderTicketId_InCardHead_WithMonoClass()
    {
        var ticketWithId = new TicketDto("Fix the bug", "dev-a", false, Id: 42);
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, ticketWithId)
            .Add(c => c.Agents, Agents));

        cut.Markup.Should().Contain("#42");
        cut.Markup.Should().Contain("card-id mono");
    }

    // --- AgeBadge tests ---

    [Fact]
    public void Should_RenderAgeBadge_WithAgeAndFreshness()
    {
        var ticketWithAge = new TicketDto(
            "Fix the bug", "dev-a", false,
            Age: TimeSpan.FromMinutes(30),
            Freshness: "Stale");
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, ticketWithAge)
            .Add(c => c.Agents, Agents));

        var ageBadge = cut.FindComponent<AgeBadge>();
        ageBadge.Should().NotBeNull();
        ageBadge.Instance.Age.Should().Be(TimeSpan.FromMinutes(30));
        ageBadge.Instance.Freshness.Should().Be("Stale");
    }

    // --- Escalation badge tests ---

    [Fact]
    public void Should_RenderEscalationBadge_When_IsEscalated()
    {
        var escalatedTicket = new TicketDto(
            "Escalated ticket", "dev-a", false,
            IsEscalated: true,
            EscalationTargetId: "dev-b");
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, escalatedTicket)
            .Add(c => c.Agents, Agents));

        cut.Markup.Should().Contain("esc-badge");
        cut.Markup.Should().Contain("siren");
        cut.Markup.Should().Contain("◆");
        cut.Markup.Should().Contain("esc → dev-b");
    }

    [Fact]
    public void Should_NotRenderEscalationBadge_When_NotEscalated()
    {
        var normalTicket = new TicketDto("Normal ticket", "dev-a", false);
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, normalTicket)
            .Add(c => c.Agents, Agents));

        cut.Markup.Should().NotContain("esc-badge");
        cut.Markup.Should().NotContain("siren");
    }

    // --- Cross-review indicator tests ---

    [Fact]
    public void Should_RenderCrossReviewGlyph_When_IsInCrossReview()
    {
        var crossReviewTicket = new TicketDto(
            "Cross review ticket", "dev-a", false,
            IsInCrossReview: true,
            CoAgentId: "dev-b");
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, crossReviewTicket)
            .Add(c => c.Agents, Agents));

        cut.Markup.Should().Contain("⇄");
    }

    [Fact]
    public void Should_NotRenderCrossReviewGlyph_When_NotInCrossReview()
    {
        var normalTicket = new TicketDto("Normal ticket", "dev-a", false);
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, normalTicket)
            .Add(c => c.Agents, Agents));

        cut.Markup.Should().NotContain("⇄");
    }

    // --- CoAgentChip tests ---

    [Fact]
    public void Should_RenderCoAgentChip_When_IsInCrossReview_And_CoAgentExists()
    {
        var crossReviewTicket = new TicketDto(
            "Cross review ticket", "dev-a", false,
            IsInCrossReview: true,
            CoAgentId: "dev-b");
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, crossReviewTicket)
            .Add(c => c.Agents, Agents));

        var agentChips = cut.FindComponents<AgentChip>();
        agentChips.Should().HaveCount(2); // Main agent + co-agent
        
        // Verify one of them is for dev-b
        var coAgentChip = agentChips.FirstOrDefault(c => 
            c.Instance.Agent?.Id == "dev-b");
        coAgentChip.Should().NotBeNull();
    }

    [Fact]
    public void Should_NotRenderCoAgentChip_When_NotInCrossReview()
    {
        var normalTicket = new TicketDto("Normal ticket", "dev-a", false);
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, normalTicket)
            .Add(c => c.Agents, Agents));

        var agentChips = cut.FindComponents<AgentChip>();
        agentChips.Should().HaveCount(1); // Only main agent
    }

    [Fact]
    public void Should_NotRenderCoAgentChip_When_CoAgentNotFound()
    {
        var crossReviewTicket = new TicketDto(
            "Cross review ticket", "dev-a", false,
            IsInCrossReview: true,
            CoAgentId: "unknown-agent");
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, crossReviewTicket)
            .Add(c => c.Agents, Agents));

        var agentChips = cut.FindComponents<AgentChip>();
        agentChips.Should().HaveCount(1); // Only main agent, co-agent not found
    }

    // --- Card state class tests ---

    [Fact]
    public void Should_ApplyEscalatedClass_When_IsEscalated()
    {
        var escalatedTicket = new TicketDto(
            "Escalated", "dev-a", false,
            IsEscalated: true,
            RetryCount: 5, // Even with high retry count, escalated takes priority
            Freshness: "Fresh");
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, escalatedTicket)
            .Add(c => c.Agents, Agents));

        cut.Markup.Should().Contain("class=\"card escalated\"");
    }

    [Fact]
    public void Should_ApplyDangerClass_When_RetryCount_Gte_3()
    {
        var dangerTicket = new TicketDto(
            "Danger ticket", "dev-a", false,
            RetryCount: 3);
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, dangerTicket)
            .Add(c => c.Agents, Agents));

        cut.Markup.Should().Contain("class=\"card danger\"");
        cut.Markup.Should().NotContain("class=\"card escalated\"");
    }

    [Fact]
    public void Should_ApplyWarnClass_When_RetryCount_Equals_2()
    {
        var warnTicket = new TicketDto(
            "Warn ticket", "dev-a", false,
            RetryCount: 2);
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, warnTicket)
            .Add(c => c.Agents, Agents));

        cut.Markup.Should().Contain("class=\"card warn\"");
        cut.Markup.Should().NotContain("class=\"card danger\"");
    }

    [Fact]
    public void Should_ApplyFreshClass_When_Freshness_Is_Fresh()
    {
        var freshTicket = new TicketDto(
            "Fresh ticket", "dev-a", false,
            Freshness: "Fresh");
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, freshTicket)
            .Add(c => c.Agents, Agents));

        cut.Markup.Should().Contain("class=\"card fresh\"");
    }

    [Fact]
    public void Should_NotApplyAnyStateClass_When_NoConditionsMatch()
    {
        var neutralTicket = new TicketDto(
            "Neutral ticket", "dev-a", false,
            Freshness: "Neutral",
            RetryCount: 1);
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, neutralTicket)
            .Add(c => c.Agents, Agents));

        // Should have base "card" class (may have trailing space, so check for "card" followed by ">" or space)
        cut.Markup.Should().Contain("class=\"card");
        cut.Markup.Should().NotContain("class=\"card fresh\"");
        cut.Markup.Should().NotContain("class=\"card warn\"");
        cut.Markup.Should().NotContain("class=\"card danger\"");
        cut.Markup.Should().NotContain("class=\"card escalated\"");
    }

    [Fact]
    public void Should_PrioritizeEscalated_Over_Danger_When_BothConditionsTrue()
    {
        var bothTicket = new TicketDto(
            "Both", "dev-a", false,
            IsEscalated: true,
            RetryCount: 5,
            Freshness: "Fresh");
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, bothTicket)
            .Add(c => c.Agents, Agents));

        cut.Markup.Should().Contain("class=\"card escalated\"");
        cut.Markup.Should().NotContain("class=\"card danger\"");
        cut.Markup.Should().NotContain("class=\"card fresh\"");
    }

    [Fact]
    public void Should_PrioritizeDanger_Over_Warn_When_RetryCount_3()
    {
        var bothTicket = new TicketDto(
            "Both", "dev-a", false,
            RetryCount: 3); // >= 3 triggers danger, not warn
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, bothTicket)
            .Add(c => c.Agents, Agents));

        cut.Markup.Should().Contain("class=\"card danger\"");
        cut.Markup.Should().NotContain("class=\"card warn\"");
    }

    [Fact]
    public void Should_ApplyStaleClass_When_Freshness_Is_Stale()
    {
        var staleTicket = new TicketDto(
            "Stale ticket", "dev-a", false,
            Freshness: "Stale",
            RetryCount: 0);
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, staleTicket)
            .Add(c => c.Agents, Agents));

        cut.Markup.Should().Contain("class=\"card stale\"");
    }

    [Fact]
    public void Should_RenderAgentChipWithName_When_NotInCrossReview()
    {
        var normalTicket = new TicketDto("Normal ticket", "dev-a", false);
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, normalTicket)
            .Add(c => c.Agents, Agents));

        var agentChip = cut.FindComponent<AgentChip>();
        agentChip.Instance.IsDense.Should().BeFalse();
        cut.Markup.Should().Contain("Developer A"); // Name should be visible
    }

    // --- Cross-review structure tests ---

    [Fact]
    public void Should_RenderCrossWrapper_When_IsInCrossReview()
    {
        var crossReviewTicket = new TicketDto(
            "Cross review ticket", "dev-a", false,
            IsInCrossReview: true,
            CoAgentId: "dev-b");
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, crossReviewTicket)
            .Add(c => c.Agents, Agents));

        cut.Markup.Should().Contain("class=\"cross\"");
    }

    [Fact]
    public void Should_RenderBothAgentChipsAsDense_When_IsInCrossReview()
    {
        var crossReviewTicket = new TicketDto(
            "Cross review ticket", "dev-a", false,
            IsInCrossReview: true,
            CoAgentId: "dev-b");
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, crossReviewTicket)
            .Add(c => c.Agents, Agents));

        var agentChips = cut.FindComponents<AgentChip>();
        agentChips.Should().HaveCount(2);
        agentChips.All(c => c.Instance.IsDense).Should().BeTrue();
    }

    [Fact]
    public void Should_NotRenderCrossWrapper_When_NotInCrossReview()
    {
        var normalTicket = new TicketDto("Normal ticket", "dev-a", false);
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, normalTicket)
            .Add(c => c.Agents, Agents));

        cut.Markup.Should().NotContain("class=\"cross\"");
    }

    [Fact]
    public void Should_RenderSingleAgentChipWithDenseFalse_When_NotInCrossReview()
    {
        var normalTicket = new TicketDto("Normal ticket", "dev-a", false);
        var cut = _ctx.Context.Render<TicketCard>(p => p
            .Add(c => c.Ticket, normalTicket)
            .Add(c => c.Agents, Agents));

        var agentChips = cut.FindComponents<AgentChip>();
        agentChips.Should().HaveCount(1);
        agentChips[0].Instance.IsDense.Should().BeFalse();
    }
}
