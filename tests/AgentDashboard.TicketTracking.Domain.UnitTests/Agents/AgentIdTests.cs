using AgentDashboard.TicketTracking.Domain.Agents;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Agents;

public sealed class AgentIdTests
{
    [Fact]
    public void Should_Throw_ArgumentNullException_When_ValueIsNull()
    {
        var act = () => new AgentId(null!);

        act.Should().ThrowExactly<ArgumentNullException>()
            .WithParameterName("value");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Should_Throw_ArgumentException_When_ValueIsEmptyOrWhitespace(string input)
    {
        var act = () => new AgentId(input);

        act.Should().ThrowExactly<ArgumentException>()
            .WithParameterName("value");
    }

    [Fact]
    public void Should_Accept_When_ValueLengthIsOne()
    {
        new AgentId("a").Value.Should().Be("a");
    }

    [Fact]
    public void Should_Accept_When_ValueLengthIsExactlyMaxLength()
    {
        var atMax = new string('a', AgentId.MaxLength);

        new AgentId(atMax).Value.Should().Be(atMax);
    }

    [Fact]
    public void Should_Throw_ArgumentOutOfRangeException_When_ValueLengthIsAboveMaxLength()
    {
        var tooLong = new string('a', AgentId.MaxLength + 1);

        var act = () => new AgentId(tooLong);

        act.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .WithParameterName("value");
    }

    [Fact]
    public void Should_ExposeMaxLength_As_StaticReadonly_NotConst()
    {
        var field = typeof(AgentId).GetField(nameof(AgentId.MaxLength));

        field.Should().NotBeNull();
        field!.IsInitOnly.Should().BeTrue();
        field.IsLiteral.Should().BeFalse();
    }

    [Fact]
    public void Should_HaveMaxLength_Of_64()
    {
        AgentId.MaxLength.Should().Be(64);
    }

    [Fact]
    public void Should_PreserveValue_WithoutTrimming_When_ValueHasSurroundingSpaces()
    {
        new AgentId(" DA ").Value.Should().Be(" DA ");
    }

    [Fact]
    public void Should_BeCaseSensitive_When_ComparingTwoInstances()
    {
        new AgentId("DA").Should().NotBe(new AgentId("da"));
    }

    [Fact]
    public void Should_BeEqual_When_TwoInstancesHaveSameValue()
    {
        new AgentId("DA").Should().Be(new AgentId("DA"));
    }

    [Fact]
    public void Should_NotBeEqual_When_TwoInstancesHaveDifferentValues()
    {
        new AgentId("DA").Should().NotBe(new AgentId("DB"));
    }

    [Fact]
    public void Should_ProduceEqualHashCodes_When_TwoInstancesHaveSameValue()
    {
        new AgentId("DA").GetHashCode().Should().Be(new AgentId("DA").GetHashCode());
    }

    [Fact]
    public void Should_BeSymmetric_When_ComparingEqualInstances()
    {
        var a = new AgentId("DA");
        var b = new AgentId("DA");

        a.Equals(b).Should().Be(b.Equals(a));
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithNull()
    {
        new AgentId("DA").Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithStringOfSameValue()
    {
        new AgentId("DA").Equals("DA").Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnValue_When_ToStringIsCalled()
    {
        new AgentId("DA").ToString().Should().Be("DA");
    }
}
