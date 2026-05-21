using AgentDashboard.TicketTracking.Domain.Tickets;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Tickets;

public sealed class AgeTests
{
    [Fact]
    public void Should_Throw_ArgumentOutOfRangeException_When_ValueIsNegative()
    {
        var act = () => new Age(TimeSpan.FromSeconds(-1));

        act.Should().Throw<ArgumentOutOfRangeException>()
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
    public void Should_BeEqual_When_TwoInstancesHaveSameValue()
    {
        new Age(TimeSpan.FromMinutes(5)).Should().Be(new Age(TimeSpan.FromMinutes(5)));
    }

    [Fact]
    public void Should_NotBeEqual_When_TwoInstancesHaveDifferentValues()
    {
        new Age(TimeSpan.FromMinutes(5)).Should().NotBe(new Age(TimeSpan.FromMinutes(6)));
    }

    [Fact]
    public void Should_ProduceEqualHashCodes_When_TwoInstancesHaveSameValue()
    {
        new Age(TimeSpan.FromMinutes(5)).GetHashCode()
            .Should().Be(new Age(TimeSpan.FromMinutes(5)).GetHashCode());
    }

    [Fact]
    public void Should_BeSymmetric_When_ComparingEqualInstances()
    {
        var a = new Age(TimeSpan.FromMinutes(5));
        var b = new Age(TimeSpan.FromMinutes(5));

        a.Equals(b).Should().Be(b.Equals(a));
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithNull()
    {
        new Age(TimeSpan.FromMinutes(5)).Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithBoxedTimeSpan()
    {
        new Age(TimeSpan.FromMinutes(5)).Equals(TimeSpan.FromMinutes(5)).Should().BeFalse();
    }
}
