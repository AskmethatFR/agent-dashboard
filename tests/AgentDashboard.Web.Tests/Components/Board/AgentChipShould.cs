using AgentDashboard.TicketTracking.Application.Queries.GetBoard.Dtos;
using AgentDashboard.Web.Components.Board;
using AgentDashboard.Web.Tests.Infrastructure;
using Bunit;
using FluentAssertions;
using Xunit;

namespace AgentDashboard.Web.Tests.Components.Board;

public class AgentChipShould : IClassFixture<BunitFixture>
{
    private static readonly AgentDto DevA = new(
        "dev-a", "Developer A", "DA", "developer");

    private readonly BunitFixture _ctx;

    public AgentChipShould(BunitFixture ctx) => _ctx = ctx;

    [Fact]
    public void RenderNothing_WhenAgentIsNull()
    {
        var cut = _ctx.Context.Render<AgentChip>(p => p.Add(c => c.Agent, null));

        cut.Markup.Should().BeEmpty();
    }

    [Fact]
    public void RenderGlyphAndName_WhenNotDense()
    {
        var cut = _ctx.Context.Render<AgentChip>(p => p
            .Add(c => c.Agent, DevA)
            .Add(c => c.IsDense, false));

        cut.Markup.Should().Contain("DA");
        cut.Markup.Should().Contain("Developer A");
        cut.Markup.Should().Contain("developer — Developer A");
        cut.Markup.Should().Contain("agent-glyph");
        cut.Markup.Should().Contain("agent-name");
    }

    [Fact]
    public void RenderOnlyGlyph_WhenDense()
    {
        var cut = _ctx.Context.Render<AgentChip>(p => p
            .Add(c => c.Agent, DevA)
            .Add(c => c.IsDense, true));

        cut.Markup.Should().Contain("DA");
        // Note: "Developer A" appears in title attribute even when Dense=true
        cut.Markup.Should().NotContain("agent-name");
        cut.Markup.Should().Contain("agent-glyph");
    }

    [Fact]
    public void GetAgentClass_ReturnsAgentThinking_WhenIsThinkingIsTrue()
    {
        var cut = _ctx.Context.Render<AgentChip>(p => p
            .Add(c => c.Agent, DevA)
            .Add(c => c.IsThinking, true));

        var instance = cut.Instance;
        instance.GetAgentClass().Should().Be("agent thinking");
    }

    [Fact]
    public void GetAgentClass_ReturnsAgentOnly_WhenIsThinkingIsFalse()
    {
        var cut = _ctx.Context.Render<AgentChip>(p => p
            .Add(c => c.Agent, DevA)
            .Add(c => c.IsThinking, false));

        var instance = cut.Instance;
        instance.GetAgentClass().Should().Be("agent");
    }

    [Fact]
    public void HaveAriaHidden_OnGlyph()
    {
        var cut = _ctx.Context.Render<AgentChip>(p => p.Add(c => c.Agent, DevA));

        cut.Markup.Should().Contain("aria-hidden=\"true\"");
    }

    [Fact]
    public void HaveTitle_WithRoleAndName()
    {
        var cut = _ctx.Context.Render<AgentChip>(p => p.Add(c => c.Agent, DevA));

        cut.Markup.Should().Contain("title=\"developer — Developer A\"");
    }
}
