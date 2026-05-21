using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Boards;
using AgentDashboard.TicketTracking.Domain.Tickets;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Tickets;

public sealed class TicketTests
{
    private static Ticket BuildValid(
        int retry = 0,
        bool escalated = false,
        bool crossReview = false,
        bool fresh = false,
        bool stale = false)
        => new(
            new TicketId(1),
            new BoardColumnId("CREATED"),
            "any title",
            new AgentId("DA"),
            coAgentId: crossReview ? new AgentId("DB") : null,
            crossReview: crossReview,
            new Retry(retry),
            new Age(TimeSpan.FromMinutes(10)),
            thinking: false,
            fresh: fresh,
            stale: stale,
            escalated: escalated,
            escalationTarget: escalated ? new AgentId("PM") : null);

    [Fact]
    public void ConstructorRejectsEmptyTitle()
    {
        var act = () => new Ticket(
            new TicketId(1), new BoardColumnId("CREATED"), " ",
            new AgentId("DA"), null, false,
            new Retry(0), new Age(TimeSpan.Zero),
            false, false, false, false, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CrossReviewRequiresCoAgent()
    {
        var act = () => new Ticket(
            new TicketId(1), new BoardColumnId("CREATED"), "t",
            new AgentId("DA"), coAgentId: null, crossReview: true,
            new Retry(0), new Age(TimeSpan.Zero),
            false, false, false, false, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CoAgentRequiresCrossReview()
    {
        var act = () => new Ticket(
            new TicketId(1), new BoardColumnId("CREATED"), "t",
            new AgentId("DA"), coAgentId: new AgentId("DB"), crossReview: false,
            new Retry(0), new Age(TimeSpan.Zero),
            false, false, false, false, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EscalatedRequiresEscalationTarget()
    {
        var act = () => new Ticket(
            new TicketId(1), new BoardColumnId("CREATED"), "t",
            new AgentId("DA"), null, false,
            new Retry(0), new Age(TimeSpan.Zero),
            false, false, false, escalated: true, escalationTarget: null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EscalationTargetRequiresEscalated()
    {
        var act = () => new Ticket(
            new TicketId(1), new BoardColumnId("CREATED"), "t",
            new AgentId("DA"), null, false,
            new Retry(0), new Age(TimeSpan.Zero),
            false, false, false, escalated: false, escalationTarget: new AgentId("PM"));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FreshAndStaleAreMutuallyExclusive()
    {
        var act = () => BuildValid(fresh: true, stale: true);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0, false, TicketSeverity.Normal)]
    [InlineData(1, false, TicketSeverity.Normal)]
    [InlineData(2, false, TicketSeverity.Warn)]
    [InlineData(3, false, TicketSeverity.Danger)]
    [InlineData(0, true,  TicketSeverity.Escalated)]
    [InlineData(2, true,  TicketSeverity.Escalated)]
    [InlineData(3, true,  TicketSeverity.Escalated)]
    public void SeverityFollowsCascade(int retry, bool escalated, TicketSeverity expected)
    {
        BuildValid(retry: retry, escalated: escalated).Severity.Should().Be(expected);
    }
}
