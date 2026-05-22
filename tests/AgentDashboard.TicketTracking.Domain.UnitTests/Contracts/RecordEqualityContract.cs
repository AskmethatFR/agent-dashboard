namespace AgentDashboard.TicketTracking.Domain.UnitTests.Contracts;

public abstract class RecordEqualityContract<T> where T : notnull
{
    /// <summary>
    /// Returns a fresh instance with stable property values.
    /// Each call MUST return a new reference, never a cached singleton —
    /// the contract asserts value equality between two independent builds.
    /// </summary>
    protected abstract T NewInstance();

    [Fact]
    public void Should_BeEqual_When_TwoInstancesHaveSameProperties()
    {
        var first = NewInstance();
        var second = NewInstance();

        first.Should().Be(second);
    }

    [Fact]
    public void Should_ProduceEqualHashCodes_When_TwoInstancesHaveSameProperties()
    {
        var first = NewInstance();
        var second = NewInstance();

        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    [Fact]
    public void Should_BeSymmetric_When_ComparingTwoEqualInstances()
    {
        var first = NewInstance();
        var second = NewInstance();

        first.Equals(second).Should().Be(second.Equals(first));
        first.Equals(second).Should().BeTrue();
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithNull()
    {
        NewInstance().Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithDifferentType()
    {
        NewInstance().Equals(new object()).Should().BeFalse();
    }
}
