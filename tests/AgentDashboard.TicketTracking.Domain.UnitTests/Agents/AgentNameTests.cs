using AgentDashboard.TicketTracking.Domain.Agents;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Agents;

public sealed class AgentNameTests
{
    [Fact]
    public void Should_Throw_ArgumentNullException_When_ValueIsNull()
    {
        var act = () => new AgentName(null!);

        act.Should().ThrowExactly<ArgumentNullException>()
            .WithParameterName("value");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Should_Throw_ArgumentException_When_ValueIsEmptyOrWhitespace(string input)
    {
        var act = () => new AgentName(input);

        act.Should().ThrowExactly<ArgumentException>()
            .WithParameterName("value");
    }

    [Fact]
    public void Should_Accept_When_ValueLengthIsOne()
    {
        var name = new AgentName("a");

        name.Value.Should().Be("a");
    }

    [Fact]
    public void Should_Accept_When_ValueLengthIsExactlyMaxLength()
    {
        var atMax = new string('n', AgentName.MaxLength);

        var name = new AgentName(atMax);

        name.Value.Should().Be(atMax);
    }

    [Fact]
    public void Should_Throw_ArgumentOutOfRangeException_When_ValueLengthIsAboveMaxLength()
    {
        var tooLong = new string('n', AgentName.MaxLength + 1);

        var act = () => new AgentName(tooLong);

        act.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .WithParameterName("value");
    }

    [Fact]
    public void Should_ExposeMaxLength_As_StaticReadonly_NotConst()
    {
        var field = typeof(AgentName).GetField(nameof(AgentName.MaxLength));

        field.Should().NotBeNull();
        field!.IsInitOnly.Should().BeTrue("MaxLength must be static readonly to avoid compile-time inlining drift");
        field.IsLiteral.Should().BeFalse("MaxLength must NOT be const");
    }

    [Fact]
    public void Should_HaveMaxLength_Of_128()
    {
        AgentName.MaxLength.Should().Be(128);
    }

    [Fact]
    public void Should_PreserveValue_WithoutTrimming_When_ValueHasSurroundingSpaces()
    {
        var name = new AgentName(" alice ");

        name.Value.Should().Be(" alice ");
    }

    [Fact]
    public void Should_BeCaseSensitive_When_ComparingTwoInstances()
    {
        new AgentName("Alice").Should().NotBe(new AgentName("alice"));
    }

    [Fact]
    public void Should_BeEqual_When_TwoInstancesHaveSameValue()
    {
        new AgentName("DevA").Should().Be(new AgentName("DevA"));
    }

    [Fact]
    public void Should_NotBeEqual_When_TwoInstancesHaveDifferentValues()
    {
        new AgentName("DevA").Should().NotBe(new AgentName("DevB"));
    }

    [Fact]
    public void Should_ProduceEqualHashCodes_When_TwoInstancesHaveSameValue()
    {
        new AgentName("DevA").GetHashCode().Should().Be(new AgentName("DevA").GetHashCode());
    }

    [Fact]
    public void Should_BeSymmetric_When_ComparingEqualInstances()
    {
        var a = new AgentName("DevA");
        var b = new AgentName("DevA");

        a.Equals(b).Should().Be(b.Equals(a));
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithNull()
    {
        var name = new AgentName("DevA");

        name.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithStringOfSameValue()
    {
        var name = new AgentName("DevA");

        name.Equals("DevA").Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnValue_When_ToStringIsCalled()
    {
        new AgentName("DevA").ToString().Should().Be("DevA");
    }
}
