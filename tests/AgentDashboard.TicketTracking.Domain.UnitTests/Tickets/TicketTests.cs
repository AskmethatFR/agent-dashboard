using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Boards;
using AgentDashboard.TicketTracking.Domain.Tickets;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Tickets;

public sealed class TicketTests
{
    [Fact]
    public void Should_LeaveCoAgentNull_When_OpenedWithoutPair()
    {
        var ticket = new TicketBuilder().AsOpen().Build();

        ticket.CoAgentId.Should().BeNull();
        ticket.IsInCrossReview.Should().BeFalse();
    }

    [Fact]
    public void Should_LeaveEscalationNull_When_OpenedWithoutEscalation()
    {
        var ticket = new TicketBuilder().AsOpen().Build();

        ticket.IsEscalated.Should().BeFalse();
        ticket.EscalationTarget.Should().BeNull();
    }

    [Fact]
    public void Should_AssignAllProperties_When_Built()
    {
        var ticket = new TicketBuilder()
            .WithId(42)
            .WithColumn("IN_DEVELOPMENT")
            .WithTitle("implement feature")
            .WithAgent("DA")
            .WithRetry(1)
            .WithAge(TimeSpan.FromHours(1))
            .WithThinking(true)
            .WithFreshness(TicketFreshness.Fresh)
            .AsOpen()
            .Build();

        ticket.Id.Value.Should().Be(42);
        ticket.ColumnId.Value.Should().Be("IN_DEVELOPMENT");
        ticket.Title.Value.Should().Be("implement feature");
        ticket.AgentId.Value.Should().Be("DA");
        ticket.Retry.Value.Should().Be(1);
        ticket.Age.Value.Should().Be(TimeSpan.FromHours(1));
        ticket.IsThinking.Should().BeTrue();
        ticket.Freshness.Should().Be(TicketFreshness.Fresh);
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_OpenedWithNullId()
    {
        var act = () => Ticket.Open(
            null!, new BoardColumnId("CREATED"), new TicketTitle("t"), new AgentId("DA"),
            new Retry(0), new Age(TimeSpan.Zero), thinking: false, TicketFreshness.Neutral);

        act.Should().Throw<ArgumentNullException>().WithParameterName("id");
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_OpenedWithNullColumnId()
    {
        var act = () => Ticket.Open(
            new TicketId(1), null!, new TicketTitle("t"), new AgentId("DA"),
            new Retry(0), new Age(TimeSpan.Zero), thinking: false, TicketFreshness.Neutral);

        act.Should().Throw<ArgumentNullException>().WithParameterName("columnId");
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_OpenedWithNullTitle()
    {
        var act = () => Ticket.Open(
            new TicketId(1), new BoardColumnId("CREATED"), null!, new AgentId("DA"),
            new Retry(0), new Age(TimeSpan.Zero), thinking: false, TicketFreshness.Neutral);

        act.Should().Throw<ArgumentNullException>().WithParameterName("title");
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_OpenedWithNullAgentId()
    {
        var act = () => Ticket.Open(
            new TicketId(1), new BoardColumnId("CREATED"), new TicketTitle("t"), null!,
            new Retry(0), new Age(TimeSpan.Zero), thinking: false, TicketFreshness.Neutral);

        act.Should().Throw<ArgumentNullException>().WithParameterName("agentId");
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_OpenedWithNullRetry()
    {
        var act = () => Ticket.Open(
            new TicketId(1), new BoardColumnId("CREATED"), new TicketTitle("t"), new AgentId("DA"),
            null!, new Age(TimeSpan.Zero), thinking: false, TicketFreshness.Neutral);

        act.Should().Throw<ArgumentNullException>().WithParameterName("retry");
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_OpenedWithNullAge()
    {
        var act = () => Ticket.Open(
            new TicketId(1), new BoardColumnId("CREATED"), new TicketTitle("t"), new AgentId("DA"),
            new Retry(0), null!, thinking: false, TicketFreshness.Neutral);

        act.Should().Throw<ArgumentNullException>().WithParameterName("age");
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_InCrossReviewWithoutCoAgent()
    {
        var act = () => Ticket.InCrossReview(
            new TicketId(1), new BoardColumnId("CREATED"), new TicketTitle("t"), new AgentId("DA"),
            coAgentId: null!,
            new Retry(0), new Age(TimeSpan.Zero), thinking: false, TicketFreshness.Neutral);

        act.Should().Throw<ArgumentNullException>().WithParameterName("coAgentId");
    }

    [Fact]
    public void Should_SetCrossReviewFlag_When_BuiltInCrossReview()
    {
        var ticket = new TicketBuilder()
            .WithCoAgent("DB")
            .AsInCrossReview()
            .Build();

        ticket.CoAgentId.Should().Be(new AgentId("DB"));
        ticket.IsInCrossReview.Should().BeTrue();
        ticket.IsEscalated.Should().BeFalse();
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_EscalatedWithoutEscalationTarget()
    {
        var act = () => Ticket.Escalated(
            new TicketId(1), new BoardColumnId("CREATED"), new TicketTitle("t"), new AgentId("DA"),
            escalationTarget: null!,
            new Retry(0), new Age(TimeSpan.Zero), thinking: false, TicketFreshness.Neutral);

        act.Should().Throw<ArgumentNullException>().WithParameterName("escalationTarget");
    }

    [Fact]
    public void Should_SetEscalatedFlag_When_BuiltEscalated()
    {
        var ticket = new TicketBuilder()
            .WithEscalationTarget("PM")
            .WithRetry(3)
            .AsEscalated()
            .Build();

        ticket.EscalationTarget.Should().Be(new AgentId("PM"));
        ticket.IsEscalated.Should().BeTrue();
        ticket.IsInCrossReview.Should().BeFalse();
    }

    [Fact]
    public void Should_BeBothEscalatedAndInCrossReview_When_InCrossReviewWithEscalationTarget()
    {
        var ticket = new TicketBuilder()
            .WithCoAgent("DB")
            .WithEscalationTarget("PM")
            .WithRetry(3)
            .AsInCrossReview()
            .Build();

        ticket.IsInCrossReview.Should().BeTrue();
        ticket.IsEscalated.Should().BeTrue();
        ticket.CoAgentId.Should().Be(new AgentId("DB"));
        ticket.EscalationTarget.Should().Be(new AgentId("PM"));
    }

    [Fact]
    public void Should_BeBothEscalatedAndInCrossReview_When_EscalatedWithCoAgent()
    {
        var ticket = new TicketBuilder()
            .WithCoAgent("DB")
            .WithEscalationTarget("PM")
            .WithRetry(3)
            .AsEscalated()
            .Build();

        ticket.IsInCrossReview.Should().BeTrue();
        ticket.IsEscalated.Should().BeTrue();
        ticket.CoAgentId.Should().Be(new AgentId("DB"));
        ticket.EscalationTarget.Should().Be(new AgentId("PM"));
    }

    [Fact]
    public void Should_AcceptFreshFreshness_When_Open()
    {
        var ticket = new TicketBuilder().WithFreshness(TicketFreshness.Fresh).AsOpen().Build();

        ticket.Freshness.Should().Be(TicketFreshness.Fresh);
    }

    [Fact]
    public void Should_AcceptStaleFreshness_When_Open()
    {
        var ticket = new TicketBuilder().WithFreshness(TicketFreshness.Stale).AsOpen().Build();

        ticket.Freshness.Should().Be(TicketFreshness.Stale);
    }

    [Theory]
    [InlineData(0, false, TicketSeverity.Normal)]
    [InlineData(1, false, TicketSeverity.Normal)]
    [InlineData(2, false, TicketSeverity.Warn)]
    [InlineData(3, false, TicketSeverity.Danger)]
    [InlineData(0, true,  TicketSeverity.Escalated)]
    [InlineData(2, true,  TicketSeverity.Escalated)]
    [InlineData(3, true,  TicketSeverity.Escalated)]
    public void Should_DeriveSeverity_When_RetryAndEscalationVary(int retry, bool escalated, TicketSeverity expected)
    {
        var builder = new TicketBuilder().WithRetry(retry);
        var ticket = escalated
            ? builder.WithEscalationTarget("PM").AsEscalated().Build()
            : builder.AsOpen().Build();

        ticket.Severity.Should().Be(expected);
    }

    [Theory]
    [InlineData(false, TicketFreshness.Neutral)]
    [InlineData(true,  TicketFreshness.Fresh)]
    [InlineData(false, TicketFreshness.Stale)]
    public void Should_KeepSeverityIndependentOfThinkingAndFreshness(bool thinking, TicketFreshness freshness)
    {
        var ticket = new TicketBuilder()
            .WithRetry(2)
            .WithThinking(thinking)
            .WithFreshness(freshness)
            .AsOpen()
            .Build();

        ticket.Severity.Should().Be(TicketSeverity.Warn);
    }

    [Fact]
    public void Should_KeepSeverityIndependentOfCrossReview()
    {
        var solo = new TicketBuilder().WithRetry(2).AsOpen().Build();
        var paired = new TicketBuilder().WithRetry(2).WithCoAgent("DB").AsInCrossReview().Build();

        solo.Severity.Should().Be(paired.Severity);
    }

    [Fact]
    public void Should_BeEqual_When_TwoTicketsBuiltWithSameProperties()
    {
        var a = new TicketBuilder()
            .WithId(7).WithColumn("CREATED").WithTitle("t").WithAgent("DA")
            .WithRetry(0).WithAge(TimeSpan.FromMinutes(10))
            .WithThinking(false).WithFreshness(TicketFreshness.Neutral)
            .AsOpen().Build();
        var b = new TicketBuilder()
            .WithId(7).WithColumn("CREATED").WithTitle("t").WithAgent("DA")
            .WithRetry(0).WithAge(TimeSpan.FromMinutes(10))
            .WithThinking(false).WithFreshness(TicketFreshness.Neutral)
            .AsOpen().Build();

        a.Should().Be(b);
    }

    [Fact]
    public void Should_NotBeEqual_When_IdsDiffer()
    {
        var a = new TicketBuilder().WithId(1).AsOpen().Build();
        var b = new TicketBuilder().WithId(2).AsOpen().Build();

        a.Should().NotBe(b);
    }

    [Fact]
    public void Should_NotBeEqual_When_TitlesDiffer()
    {
        var a = new TicketBuilder().WithTitle("alpha").AsOpen().Build();
        var b = new TicketBuilder().WithTitle("beta").AsOpen().Build();

        a.Should().NotBe(b);
    }

    [Fact]
    public void Should_HaveSameHashCode_When_PropertiesMatch()
    {
        var a = new TicketBuilder()
            .WithId(7).WithColumn("CREATED").WithTitle("t").WithAgent("DA")
            .WithRetry(0).WithAge(TimeSpan.FromMinutes(10))
            .WithThinking(false).WithFreshness(TicketFreshness.Neutral)
            .AsOpen().Build();
        var b = new TicketBuilder()
            .WithId(7).WithColumn("CREATED").WithTitle("t").WithAgent("DA")
            .WithRetry(0).WithAge(TimeSpan.FromMinutes(10))
            .WithThinking(false).WithFreshness(TicketFreshness.Neutral)
            .AsOpen().Build();

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithNull()
    {
        var ticket = new TicketBuilder().AsOpen().Build();

        ticket.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithDifferentType()
    {
        var ticket = new TicketBuilder().AsOpen().Build();

        ticket.Equals("not a ticket").Should().BeFalse();
    }

    [Fact]
    public void Should_BeSymmetric_When_EqualsCalledOnEqualTickets()
    {
        var a = new TicketBuilder()
            .WithId(7).WithColumn("CREATED").WithTitle("t").WithAgent("DA")
            .WithRetry(0).WithAge(TimeSpan.FromMinutes(10))
            .WithThinking(false).WithFreshness(TicketFreshness.Neutral)
            .AsOpen().Build();
        var b = new TicketBuilder()
            .WithId(7).WithColumn("CREATED").WithTitle("t").WithAgent("DA")
            .WithRetry(0).WithAge(TimeSpan.FromMinutes(10))
            .WithThinking(false).WithFreshness(TicketFreshness.Neutral)
            .AsOpen().Build();

        a.Equals(b).Should().Be(b.Equals(a));
        a.Equals(b).Should().BeTrue();
    }
}
