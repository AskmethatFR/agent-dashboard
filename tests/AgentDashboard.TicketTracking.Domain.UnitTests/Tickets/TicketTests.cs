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

        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("id");
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_OpenedWithNullColumnId()
    {
        var act = () => Ticket.Open(
            new TicketId(1), null!, new TicketTitle("t"), new AgentId("DA"),
            new Retry(0), new Age(TimeSpan.Zero), thinking: false, TicketFreshness.Neutral);

        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("columnId");
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_OpenedWithNullTitle()
    {
        var act = () => Ticket.Open(
            new TicketId(1), new BoardColumnId("CREATED"), null!, new AgentId("DA"),
            new Retry(0), new Age(TimeSpan.Zero), thinking: false, TicketFreshness.Neutral);

        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("title");
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_OpenedWithNullAgentId()
    {
        var act = () => Ticket.Open(
            new TicketId(1), new BoardColumnId("CREATED"), new TicketTitle("t"), null!,
            new Retry(0), new Age(TimeSpan.Zero), thinking: false, TicketFreshness.Neutral);

        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("agentId");
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_OpenedWithNullRetry()
    {
        var act = () => Ticket.Open(
            new TicketId(1), new BoardColumnId("CREATED"), new TicketTitle("t"), new AgentId("DA"),
            null!, new Age(TimeSpan.Zero), thinking: false, TicketFreshness.Neutral);

        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("retry");
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_OpenedWithNullAge()
    {
        var act = () => Ticket.Open(
            new TicketId(1), new BoardColumnId("CREATED"), new TicketTitle("t"), new AgentId("DA"),
            new Retry(0), null!, thinking: false, TicketFreshness.Neutral);

        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("age");
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_InCrossReviewWithoutCoAgent()
    {
        var act = () => Ticket.InCrossReview(
            new TicketId(1), new BoardColumnId("CREATED"), new TicketTitle("t"), new AgentId("DA"),
            coAgentId: null!,
            new Retry(0), new Age(TimeSpan.Zero), thinking: false, TicketFreshness.Neutral);

        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("coAgentId");
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

        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("escalationTarget");
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

    public static IEnumerable<object[]> NotEqualVariants() =>
    [
        ["Id"],
        ["ColumnId"],
        ["Title"],
        ["AgentId"],
        ["Retry"],
        ["Age"],
        ["IsThinking"],
        ["Freshness"],
        ["CoAgentAndCrossReview"],
        ["EscalationTargetAndEscalated"],
    ];

    [Theory]
    [MemberData(nameof(NotEqualVariants))]
    public void Should_NotBeEqual_When_PropertyDiffers(string property)
    {
        var baseTicket = new TicketBuilder().Build();
        var modified = ApplyDivergence(new TicketBuilder(), property).Build();

        modified.Should().NotBe(baseTicket, $"differing {property} should break equality");
    }

    private static TicketBuilder ApplyDivergence(TicketBuilder builder, string property) => property switch
    {
        "Id" => builder.WithId(99),
        "ColumnId" => builder.WithColumn("DONE"),
        "Title" => builder.WithTitle("other"),
        "AgentId" => builder.WithAgent("DB"),
        "Retry" => builder.WithRetry(3),
        "Age" => builder.WithAge(TimeSpan.FromHours(5)),
        "IsThinking" => builder.WithThinking(true),
        "Freshness" => builder.WithFreshness(TicketFreshness.Fresh),
        "CoAgentAndCrossReview" => builder.WithCoAgent("DB").AsInCrossReview(),
        "EscalationTargetAndEscalated" => builder.WithEscalationTarget("PM").AsEscalated(),
        _ => throw new ArgumentOutOfRangeException(nameof(property), property, "Unknown divergence"),
    };

    [Fact]
    public void Should_NotBeEqual_When_OnlyEscalationFlagsDifferOnCrossReviewedTicket()
    {
        var withoutEscalation = new TicketBuilder().WithCoAgent("DB").AsInCrossReview().Build();
        var withEscalation = new TicketBuilder()
            .WithCoAgent("DB")
            .WithEscalationTarget("PM")
            .AsInCrossReview()
            .Build();

        withEscalation.Should().NotBe(withoutEscalation);
    }

    [Fact]
    public void Should_NotBeEqual_When_OnlyCrossReviewFlagsDifferOnEscalatedTicket()
    {
        var withoutCoAgent = new TicketBuilder()
            .WithEscalationTarget("PM")
            .AsEscalated()
            .Build();
        var withCoAgent = new TicketBuilder()
            .WithEscalationTarget("PM")
            .WithCoAgent("DB")
            .AsEscalated()
            .Build();

        withCoAgent.Should().NotBe(withoutCoAgent);
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
