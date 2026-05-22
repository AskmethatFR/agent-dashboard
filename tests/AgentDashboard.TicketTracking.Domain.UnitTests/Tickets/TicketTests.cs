using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Boards;
using AgentDashboard.TicketTracking.Domain.Tickets;
using AgentDashboard.TicketTracking.TestShared.Tickets;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Tickets;

public sealed class TicketTests
{
    [Fact]
    public void Should_LeaveCoAgentNull_When_OpenedWithoutPair()
    {
        var ticket = TicketFixtures.Default;

        ticket.CoAgentId.Should().BeNull();
        ticket.IsInCrossReview.Should().BeFalse();
    }

    [Fact]
    public void Should_LeaveEscalationNull_When_OpenedWithoutEscalation()
    {
        var ticket = TicketFixtures.Default;

        ticket.IsEscalated.Should().BeFalse();
        ticket.EscalationTarget.Should().BeNull();
    }

    [Fact]
    public void Should_AssignAllProperties_When_Built()
    {
        var ticket = TicketFixtures.Open(
            id: 42,
            columnId: "IN_DEVELOPMENT",
            title: "implement feature",
            agentId: "DA",
            retry: 1,
            age: TimeSpan.FromHours(1),
            thinking: true,
            freshness: TicketFreshness.Fresh);

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
        var ticket = TicketFixtures.InCrossReview(coAgentId: "DB");

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
        var ticket = TicketFixtures.Escalated(escalationTarget: "PM", retry: 3);

        ticket.EscalationTarget.Should().Be(new AgentId("PM"));
        ticket.IsEscalated.Should().BeTrue();
        ticket.IsInCrossReview.Should().BeFalse();
    }

    [Fact]
    public void Should_BeBothEscalatedAndInCrossReview_When_InCrossReviewWithEscalationTarget()
    {
        var ticket = TicketFixtures.InCrossReview(
            coAgentId: "DB",
            escalationTarget: "PM",
            retry: 3);

        ticket.IsInCrossReview.Should().BeTrue();
        ticket.IsEscalated.Should().BeTrue();
        ticket.CoAgentId.Should().Be(new AgentId("DB"));
        ticket.EscalationTarget.Should().Be(new AgentId("PM"));
    }

    [Fact]
    public void Should_BeBothEscalatedAndInCrossReview_When_EscalatedWithCoAgent()
    {
        var ticket = TicketFixtures.Escalated(
            coAgentId: "DB",
            escalationTarget: "PM",
            retry: 3);

        ticket.IsInCrossReview.Should().BeTrue();
        ticket.IsEscalated.Should().BeTrue();
        ticket.CoAgentId.Should().Be(new AgentId("DB"));
        ticket.EscalationTarget.Should().Be(new AgentId("PM"));
    }

    [Fact]
    public void Should_AcceptFreshFreshness_When_Open()
    {
        var ticket = TicketFixtures.Open(freshness: TicketFreshness.Fresh);

        ticket.Freshness.Should().Be(TicketFreshness.Fresh);
    }

    [Fact]
    public void Should_AcceptStaleFreshness_When_Open()
    {
        var ticket = TicketFixtures.Open(freshness: TicketFreshness.Stale);

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
        var ticket = escalated
            ? TicketFixtures.Escalated(retry: retry, escalationTarget: "PM")
            : TicketFixtures.Open(retry: retry);

        ticket.Severity.Should().Be(expected);
    }

    [Theory]
    [InlineData(false, TicketFreshness.Neutral)]
    [InlineData(true,  TicketFreshness.Fresh)]
    [InlineData(false, TicketFreshness.Stale)]
    public void Should_KeepSeverityIndependentOfThinkingAndFreshness(bool thinking, TicketFreshness freshness)
    {
        var ticket = TicketFixtures.Open(retry: 2, thinking: thinking, freshness: freshness);

        ticket.Severity.Should().Be(TicketSeverity.Warn);
    }

    [Fact]
    public void Should_KeepSeverityIndependentOfCrossReview()
    {
        var solo = TicketFixtures.Open(retry: 2);
        var paired = TicketFixtures.InCrossReview(retry: 2, coAgentId: "DB");

        solo.Severity.Should().Be(paired.Severity);
    }

    [Fact]
    public void Should_BeEqual_When_TwoTicketsBuiltWithSameProperties()
    {
        var a = FullyAssembled();
        var b = FullyAssembled();

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
        var baseTicket = TicketFixtures.Default;
        var modified = ApplyDivergence(property);

        modified.Should().NotBe(baseTicket, $"differing {property} should break equality");
    }

    private static Ticket ApplyDivergence(string property) => property switch
    {
        "Id" => TicketFixtures.Open(id: 99),
        "ColumnId" => TicketFixtures.Open(columnId: "DONE"),
        "Title" => TicketFixtures.Open(title: "other"),
        "AgentId" => TicketFixtures.Open(agentId: "DB"),
        "Retry" => TicketFixtures.Open(retry: 3),
        "Age" => TicketFixtures.Open(age: TimeSpan.FromHours(5)),
        "IsThinking" => TicketFixtures.Open(thinking: true),
        "Freshness" => TicketFixtures.Open(freshness: TicketFreshness.Fresh),
        "CoAgentAndCrossReview" => TicketFixtures.InCrossReview(coAgentId: "DB"),
        "EscalationTargetAndEscalated" => TicketFixtures.Escalated(escalationTarget: "PM"),
        _ => throw new ArgumentOutOfRangeException(nameof(property), property, "Unknown divergence"),
    };

    [Fact]
    public void Should_NotBeEqual_When_OnlyEscalationFlagsDifferOnCrossReviewedTicket()
    {
        var withoutEscalation = TicketFixtures.InCrossReview(coAgentId: "DB");
        var withEscalation = TicketFixtures.InCrossReview(coAgentId: "DB", escalationTarget: "PM");

        withEscalation.Should().NotBe(withoutEscalation);
    }

    [Fact]
    public void Should_NotBeEqual_When_OnlyCrossReviewFlagsDifferOnEscalatedTicket()
    {
        var withoutCoAgent = TicketFixtures.Escalated(escalationTarget: "PM");
        var withCoAgent = TicketFixtures.Escalated(escalationTarget: "PM", coAgentId: "DB");

        withCoAgent.Should().NotBe(withoutCoAgent);
    }

    [Fact]
    public void Should_HaveSameHashCode_When_PropertiesMatch()
    {
        var a = FullyAssembled();
        var b = FullyAssembled();

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithNull()
    {
        var ticket = TicketFixtures.Default;

        ticket.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithDifferentType()
    {
        var ticket = TicketFixtures.Default;

        ticket.Equals("not a ticket").Should().BeFalse();
    }

    [Fact]
    public void Should_BeSymmetric_When_EqualsCalledOnEqualTickets()
    {
        var a = FullyAssembled();
        var b = FullyAssembled();

        a.Equals(b).Should().Be(b.Equals(a));
        a.Equals(b).Should().BeTrue();
    }

    private static Ticket FullyAssembled() => TicketFixtures.Open(
        id: 7,
        columnId: "CREATED",
        title: "t",
        agentId: "DA",
        retry: 0,
        age: TimeSpan.FromMinutes(10),
        thinking: false,
        freshness: TicketFreshness.Neutral);
}
