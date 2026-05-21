using AgentDashboard.TicketTracking.Domain.Tickets;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Tickets;

public sealed class RetryTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    [InlineData(100)]
    public void ConstructorRejectsOutOfRange(int value)
    {
        var act = () => new Retry(value);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void ConstructorAcceptsInRange(int value)
    {
        new Retry(value).Value.Should().Be(value);
    }

    [Theory]
    [InlineData(0, false, false)]
    [InlineData(1, false, false)]
    [InlineData(2, true,  false)]
    [InlineData(3, false, true)]
    public void ThresholdsMatchRetryProtocol(int value, bool warn, bool danger)
    {
        var retry = new Retry(value);
        retry.IsAtWarnThreshold.Should().Be(warn);
        retry.IsAtDangerThreshold.Should().Be(danger);
    }
}
