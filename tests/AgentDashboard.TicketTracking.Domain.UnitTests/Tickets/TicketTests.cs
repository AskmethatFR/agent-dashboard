using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Boards;
using AgentDashboard.TicketTracking.Domain.Tickets;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Tickets;

public sealed class TicketTests
{
    private static TicketId AnyId() => new(1);
    private static BoardColumnId AnyColumn() => new("CREATED");
    private static AgentId DevA() => new("DA");
    private static AgentId DevB() => new("DB");
    private static AgentId Pm() => new("PM");
    private static Retry Zero() => new(0);
    private static Age TenMinutes() => new(TimeSpan.FromMinutes(10));

    [Fact]
    public void OpenBuildsTicketWithoutCoAgentNorEscalation()
    {
        var ticket = Ticket.Open(
            AnyId(), AnyColumn(), "any title", DevA(),
            Zero(), TenMinutes(), thinking: false, TicketFreshness.Neutral);

        ticket.CoAgentId.Should().BeNull();
        ticket.IsInCrossReview.Should().BeFalse();
        ticket.IsEscalated.Should().BeFalse();
        ticket.EscalationTarget.Should().BeNull();
    }

    [Fact]
    public void OpenAssignsAllProperties()
    {
        var ticket = Ticket.Open(
            new TicketId(42),
            new BoardColumnId("IN_DEVELOPMENT"),
            "implement feature",
            DevA(),
            new Retry(1),
            new Age(TimeSpan.FromHours(1)),
            thinking: true,
            TicketFreshness.Fresh);

        ticket.Id.Value.Should().Be(42);
        ticket.ColumnId.Value.Should().Be("IN_DEVELOPMENT");
        ticket.Title.Should().Be("implement feature");
        ticket.AgentId.Value.Should().Be("DA");
        ticket.Retry.Value.Should().Be(1);
        ticket.Age.Value.Should().Be(TimeSpan.FromHours(1));
        ticket.IsThinking.Should().BeTrue();
        ticket.Freshness.Should().Be(TicketFreshness.Fresh);
    }

    [Fact]
    public void InCrossReviewRequiresCoAgent()
    {
        var act = () => Ticket.InCrossReview(
            AnyId(), AnyColumn(), "t", DevA(),
            coAgentId: null!,
            Zero(), TenMinutes(), thinking: false, TicketFreshness.Neutral);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void InCrossReviewBuildsTicketWithCoAgentAndCrossReviewFlag()
    {
        var ticket = Ticket.InCrossReview(
            AnyId(), AnyColumn(), "t", DevA(), DevB(),
            Zero(), TenMinutes(), thinking: false, TicketFreshness.Neutral);

        ticket.CoAgentId.Should().Be(DevB());
        ticket.IsInCrossReview.Should().BeTrue();
        ticket.IsEscalated.Should().BeFalse();
    }

    [Fact]
    public void EscalatedRequiresEscalationTarget()
    {
        var act = () => Ticket.Escalated(
            AnyId(), AnyColumn(), "t", DevA(),
            escalationTarget: null!,
            Zero(), TenMinutes(), thinking: false, TicketFreshness.Neutral);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EscalatedBuildsTicketWithEscalationTargetAndFlag()
    {
        var ticket = Ticket.Escalated(
            AnyId(), AnyColumn(), "t", DevA(), Pm(),
            new Retry(3), TenMinutes(), thinking: false, TicketFreshness.Neutral);

        ticket.EscalationTarget.Should().Be(Pm());
        ticket.IsEscalated.Should().BeTrue();
        ticket.IsInCrossReview.Should().BeFalse();
    }

    [Fact]
    public void InCrossReviewMayAlsoBeEscalated()
    {
        var ticket = Ticket.InCrossReview(
            AnyId(), AnyColumn(), "t", DevA(), DevB(),
            new Retry(3), TenMinutes(), thinking: false, TicketFreshness.Neutral,
            escalationTarget: Pm());

        ticket.IsInCrossReview.Should().BeTrue();
        ticket.IsEscalated.Should().BeTrue();
        ticket.CoAgentId.Should().Be(DevB());
        ticket.EscalationTarget.Should().Be(Pm());
    }

    [Fact]
    public void EscalatedMayAlsoBeInCrossReview()
    {
        var ticket = Ticket.Escalated(
            AnyId(), AnyColumn(), "t", DevA(), Pm(),
            new Retry(3), TenMinutes(), thinking: false, TicketFreshness.Neutral,
            coAgentId: DevB());

        ticket.IsInCrossReview.Should().BeTrue();
        ticket.IsEscalated.Should().BeTrue();
        ticket.CoAgentId.Should().Be(DevB());
        ticket.EscalationTarget.Should().Be(Pm());
    }

    [Fact]
    public void OpenRejectsNullId()
    {
        var act = () => Ticket.Open(
            null!, AnyColumn(), "t", DevA(),
            Zero(), TenMinutes(), thinking: false, TicketFreshness.Neutral);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void OpenRejectsNullColumnId()
    {
        var act = () => Ticket.Open(
            AnyId(), null!, "t", DevA(),
            Zero(), TenMinutes(), thinking: false, TicketFreshness.Neutral);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void OpenRejectsNullAgentId()
    {
        var act = () => Ticket.Open(
            AnyId(), AnyColumn(), "t", null!,
            Zero(), TenMinutes(), thinking: false, TicketFreshness.Neutral);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void OpenRejectsNullRetry()
    {
        var act = () => Ticket.Open(
            AnyId(), AnyColumn(), "t", DevA(),
            null!, TenMinutes(), thinking: false, TicketFreshness.Neutral);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void OpenRejectsNullAge()
    {
        var act = () => Ticket.Open(
            AnyId(), AnyColumn(), "t", DevA(),
            Zero(), null!, thinking: false, TicketFreshness.Neutral);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void OpenRejectsEmptyTitle(string title)
    {
        var act = () => Ticket.Open(
            AnyId(), AnyColumn(), title, DevA(),
            Zero(), TenMinutes(), thinking: false, TicketFreshness.Neutral);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void OpenRejectsNullTitle()
    {
        var act = () => Ticket.Open(
            AnyId(), AnyColumn(), null!, DevA(),
            Zero(), TenMinutes(), thinking: false, TicketFreshness.Neutral);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void OpenAcceptsTitleAtMaxLength()
    {
        var maxTitle = new string('a', Ticket.MaxTitleLength);
        var ticket = Ticket.Open(
            AnyId(), AnyColumn(), maxTitle, DevA(),
            Zero(), TenMinutes(), thinking: false, TicketFreshness.Neutral);
        ticket.Title.Should().Be(maxTitle);
    }

    [Fact]
    public void OpenRejectsTitleOverMaxLength()
    {
        var tooLong = new string('a', Ticket.MaxTitleLength + 1);
        var act = () => Ticket.Open(
            AnyId(), AnyColumn(), tooLong, DevA(),
            Zero(), TenMinutes(), thinking: false, TicketFreshness.Neutral);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("hello\nworld")]
    [InlineData("carriage\rreturn")]
    [InlineData("null\0byte")]
    public void OpenRejectsControlCharactersInTitle(string title)
    {
        var act = () => Ticket.Open(
            AnyId(), AnyColumn(), title, DevA(),
            Zero(), TenMinutes(), thinking: false, TicketFreshness.Neutral);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void OpenAcceptsFreshFreshness()
    {
        var ticket = Ticket.Open(
            AnyId(), AnyColumn(), "t", DevA(),
            Zero(), TenMinutes(), thinking: false, TicketFreshness.Fresh);
        ticket.Freshness.Should().Be(TicketFreshness.Fresh);
    }

    [Fact]
    public void OpenAcceptsStaleFreshness()
    {
        var ticket = Ticket.Open(
            AnyId(), AnyColumn(), "t", DevA(),
            Zero(), TenMinutes(), thinking: false, TicketFreshness.Stale);
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
    public void SeverityFollowsCascade(int retry, bool escalated, TicketSeverity expected)
    {
        var ticket = escalated
            ? Ticket.Escalated(
                AnyId(), AnyColumn(), "t", DevA(), Pm(),
                new Retry(retry), TenMinutes(), thinking: false, TicketFreshness.Neutral)
            : Ticket.Open(
                AnyId(), AnyColumn(), "t", DevA(),
                new Retry(retry), TenMinutes(), thinking: false, TicketFreshness.Neutral);

        ticket.Severity.Should().Be(expected);
    }

    [Theory]
    [InlineData(false, TicketFreshness.Neutral)]
    [InlineData(true,  TicketFreshness.Fresh)]
    [InlineData(false, TicketFreshness.Stale)]
    public void SeverityIsUnaffectedByThinkingAndFreshness(bool thinking, TicketFreshness freshness)
    {
        var ticket = Ticket.Open(
            AnyId(), AnyColumn(), "t", DevA(),
            new Retry(2), TenMinutes(), thinking, freshness);
        ticket.Severity.Should().Be(TicketSeverity.Warn);
    }

    [Fact]
    public void SeverityIsUnaffectedByCrossReview()
    {
        var solo = Ticket.Open(
            AnyId(), AnyColumn(), "t", DevA(),
            new Retry(2), TenMinutes(), thinking: false, TicketFreshness.Neutral);
        var paired = Ticket.InCrossReview(
            AnyId(), AnyColumn(), "t", DevA(), DevB(),
            new Retry(2), TenMinutes(), thinking: false, TicketFreshness.Neutral);

        solo.Severity.Should().Be(paired.Severity);
    }
}
