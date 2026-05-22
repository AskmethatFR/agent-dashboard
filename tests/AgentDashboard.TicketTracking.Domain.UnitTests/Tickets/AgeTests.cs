using AgentDashboard.TicketTracking.Domain.Tickets;
using AgentDashboard.TicketTracking.Domain.UnitTests.Contracts;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Tickets;

public sealed class AgeTests : RecordEqualityContract<Age>
{
    protected override Age NewInstance() => new(TimeSpan.FromMinutes(5));

    [Fact]
    public void Should_Throw_ArgumentOutOfRangeException_When_ValueIsNegative()
    {
        var act = () => new Age(TimeSpan.FromSeconds(-1));

        act.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .WithParameterName("value");
    }

    [Fact]
    public void Should_Accept_When_ValueIsZero()
    {
        new Age(TimeSpan.Zero).Value.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Should_ReturnFalse_When_IsWarningCalledBelowThreshold()
    {
        new Age(Age.WarningThreshold - TimeSpan.FromMilliseconds(1))
            .IsWarning.Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnTrue_When_IsWarningCalledAtExactlyThreshold()
    {
        new Age(Age.WarningThreshold).IsWarning.Should().BeTrue();
    }

    [Fact]
    public void Should_ReturnTrue_When_IsWarningCalledAboveThreshold()
    {
        new Age(Age.WarningThreshold + TimeSpan.FromMilliseconds(1))
            .IsWarning.Should().BeTrue();
    }

    [Fact]
    public void Should_NotBeEqual_When_TwoInstancesHaveDifferentValues()
    {
        new Age(TimeSpan.FromMinutes(5)).Should().NotBe(new Age(TimeSpan.FromMinutes(6)));
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithBoxedTimeSpan()
    {
        new Age(TimeSpan.FromMinutes(5)).Equals(TimeSpan.FromMinutes(5)).Should().BeFalse();
    }

    [Fact]
    public void Should_HaveWarningThreshold_Of_3Hours()
    {
        Age.WarningThreshold.Should().Be(TimeSpan.FromHours(3));
    }
}
