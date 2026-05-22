using System.Reflection;
using AgentDashboard.TicketTracking.Domain.Tickets;
using AgentDashboard.TicketTracking.Domain.UnitTests.Contracts;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Tickets;

public sealed class RetryTests : RecordEqualityContract<Retry>
{
    protected override Retry NewInstance() => new(2);

    [Fact]
    public void Should_Throw_ArgumentOutOfRangeException_When_ValueIsBelowMinimum()
    {
        var act = () => new Retry(-1);

        act.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .WithParameterName("value");
    }

    [Fact]
    public void Should_Accept_When_ValueIsZero()
    {
        new Retry(0).Value.Should().Be(0);
    }

    [Fact]
    public void Should_Accept_When_ValueIsAtMaxBeforeEscalation()
    {
        new Retry(Retry.MaxBeforeEscalation).Value.Should().Be(Retry.MaxBeforeEscalation);
    }

    [Fact]
    public void Should_Throw_ArgumentOutOfRangeException_When_ValueIsAboveMaxBeforeEscalation()
    {
        var act = () => new Retry(Retry.MaxBeforeEscalation + 1);

        act.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .WithParameterName("value");
    }

    [Theory]
    [InlineData(0, false, false)]
    [InlineData(1, false, false)]
    [InlineData(2, true,  false)]
    [InlineData(3, false, true)]
    public void Should_ReflectThresholds_When_ValueIsInRange(int value, bool warn, bool danger)
    {
        var retry = new Retry(value);

        retry.IsAtWarnThreshold.Should().Be(warn);
        retry.IsAtDangerThreshold.Should().Be(danger);
    }

    [Fact]
    public void Should_NotBeEqual_When_TwoInstancesHaveDifferentValues()
    {
        new Retry(1).Should().NotBe(new Retry(2));
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithBoxedInt()
    {
        new Retry(2).Equals(2).Should().BeFalse();
    }

    [Fact]
    public void Should_ExposeMaxBeforeEscalation_As_StaticReadonly_NotConst()
    {
        var field = typeof(Retry).GetField(
            nameof(Retry.MaxBeforeEscalation),
            BindingFlags.Public | BindingFlags.Static);

        field.Should().NotBeNull();
        field!.IsInitOnly.Should().BeTrue("MaxBeforeEscalation must be static readonly to avoid compile-time inlining drift");
        field.IsLiteral.Should().BeFalse("MaxBeforeEscalation must NOT be const");
    }

    [Fact]
    public void Should_ExposeWarnAt_As_StaticReadonly_NotConst()
    {
        var field = typeof(Retry).GetField(
            nameof(Retry.WarnAt),
            BindingFlags.Public | BindingFlags.Static);

        field.Should().NotBeNull();
        field!.IsInitOnly.Should().BeTrue("WarnAt must be static readonly to avoid compile-time inlining drift");
        field.IsLiteral.Should().BeFalse("WarnAt must NOT be const");
    }

    [Fact]
    public void Should_HaveMaxBeforeEscalation_Of_3()
    {
        Retry.MaxBeforeEscalation.Should().Be(3);
    }

    [Fact]
    public void Should_HaveWarnAt_Of_2()
    {
        Retry.WarnAt.Should().Be(2);
    }
}
