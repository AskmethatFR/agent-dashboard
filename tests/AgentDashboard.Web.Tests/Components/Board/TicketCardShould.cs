using AgentDashboard.TicketTracking.Application.Queries.GetBoard.Dtos;
using AgentDashboard.Web.Components.Board;
using Bunit;
using FluentAssertions;
using Xunit;

namespace AgentDashboard.Web.Tests.Components.Board;

public class TicketCardShould
{
    private static readonly AgentDto DevADto = new(
        "dev-a", "Developer A", "DA", "developer");

    private static readonly TicketDto SampleTicket = new(
        "Fix the bug", "dev-a", false);

    private static readonly TicketDto ThinkingTicket = new(
        "Review PR", "dev-a", true);

    private static readonly IReadOnlyList<AgentDto> Agents = new List<AgentDto> { DevADto };

    [Fact]
    public void RenderTicketTitle()
    {
        using var ctx = new BunitContext();
        var cut = ctx.Render<TicketCard>(p => p
            .Add(c => c.Ticket, SampleTicket)
            .Add(c => c.Agents, Agents));

        cut.Markup.Should().Contain("Fix the bug");
    }

    [Fact]
    public void RenderAgentChipWithGlyphAndName()
    {
        using var ctx = new BunitContext();
        var cut = ctx.Render<TicketCard>(p => p
            .Add(c => c.Ticket, SampleTicket)
            .Add(c => c.Agents, Agents));

        cut.Markup.Should().Contain("DA");
        cut.Markup.Should().Contain("Developer A");
    }

    [Fact]
    public void RenderAgentChipWithThinkingClass_WhenIsThinkingIsTrue()
    {
        using var ctx = new BunitContext();
        var cut = ctx.Render<TicketCard>(p => p
            .Add(c => c.Ticket, ThinkingTicket)
            .Add(c => c.Agents, Agents));

        var agentChip = cut.FindComponent<AgentChip>();
        var instance = agentChip.Instance;
        instance.GetAgentClass().Should().Be("agent thinking");
    }

    [Fact]
    public void RenderAgentChipWithAgentClass_WhenIsThinkingIsFalse()
    {
        using var ctx = new BunitContext();
        var cut = ctx.Render<TicketCard>(p => p
            .Add(c => c.Ticket, SampleTicket)
            .Add(c => c.Agents, Agents));

        var agentChip = cut.FindComponent<AgentChip>();
        var instance = agentChip.Instance;
        instance.GetAgentClass().Should().Be("agent");
    }

    [Fact]
    public void RenderNothingForAgentChip_WhenAgentNotFound()
    {
        using var ctx = new BunitContext();
        var unknownAgentTicket = new TicketDto("Orphan ticket", "unknown-id", false);
        var cut = ctx.Render<TicketCard>(p => p
            .Add(c => c.Ticket, unknownAgentTicket)
            .Add(c => c.Agents, Agents));

        // AgentChip renders nothing when Agent is null
        cut.Markup.Should().NotContain("agent-glyph");
        cut.Markup.Should().NotContain("agent-name");
    }
}
