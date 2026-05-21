using AgentDashboard.TicketTracking.Domain.Tickets;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Tickets;

public sealed class AgeTests
{
    [Fact]
    public void ConstructorRejectsNegative()
    {
        var act = () => new Age(TimeSpan.FromSeconds(-1));
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ConstructorAcceptsZero()
    {
        new Age(TimeSpan.Zero).Value.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void IsWarningFalseBelowThreshold()
    {
        new Age(TimeSpan.FromHours(2.999)).IsWarning.Should().BeFalse();
    }

    [Fact]
    public void IsWarningTrueAtExactlyThreshold()
    {
        new Age(TimeSpan.FromHours(3)).IsWarning.Should().BeTrue();
    }

    [Fact]
    public void IsWarningTrueAboveThreshold()
    {
        new Age(TimeSpan.FromHours(5)).IsWarning.Should().BeTrue();
    }
}
