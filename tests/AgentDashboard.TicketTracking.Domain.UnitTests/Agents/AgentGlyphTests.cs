using AgentDashboard.TicketTracking.Domain.Agents;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Agents;

public sealed class AgentGlyphTests
{
    [Fact]
    public void Should_Throw_ArgumentNullException_When_ValueIsNull()
    {
        var act = () => new AgentGlyph(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("value");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Should_Throw_ArgumentException_When_ValueIsEmptyOrWhitespace(string input)
    {
        var act = () => new AgentGlyph(input);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("value");
    }

    [Fact]
    public void Should_Accept_When_ValueLengthIsOne()
    {
        new AgentGlyph("A").Value.Should().Be("A");
    }

    [Fact]
    public void Should_Accept_When_ValueLengthIsExactlyMaxLength()
    {
        var atMax = new string('g', AgentGlyph.MaxLength);

        new AgentGlyph(atMax).Value.Should().Be(atMax);
    }

    [Fact]
    public void Should_Throw_ArgumentOutOfRangeException_When_ValueLengthIsAboveMaxLength()
    {
        var tooLong = new string('g', AgentGlyph.MaxLength + 1);

        var act = () => new AgentGlyph(tooLong);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("value");
    }

    [Fact]
    public void Should_ExposeMaxLength_As_StaticReadonly_NotConst()
    {
        var field = typeof(AgentGlyph).GetField(nameof(AgentGlyph.MaxLength));

        field.Should().NotBeNull();
        field!.IsInitOnly.Should().BeTrue();
        field.IsLiteral.Should().BeFalse();
    }

    [Fact]
    public void Should_HaveMaxLength_Of_8()
    {
        AgentGlyph.MaxLength.Should().Be(8);
    }

    [Fact]
    public void Should_PreserveValue_WithoutTrimming_When_ValueHasSurroundingSpaces()
    {
        new AgentGlyph(" g ").Value.Should().Be(" g ");
    }

    [Fact]
    public void Should_BeCaseSensitive_When_ComparingTwoInstances()
    {
        new AgentGlyph("Da").Should().NotBe(new AgentGlyph("da"));
    }

    [Fact]
    public void Should_BeEqual_When_TwoInstancesHaveSameValue()
    {
        new AgentGlyph("Da").Should().Be(new AgentGlyph("Da"));
    }

    [Fact]
    public void Should_NotBeEqual_When_TwoInstancesHaveDifferentValues()
    {
        new AgentGlyph("Da").Should().NotBe(new AgentGlyph("Db"));
    }

    [Fact]
    public void Should_ProduceEqualHashCodes_When_TwoInstancesHaveSameValue()
    {
        new AgentGlyph("Da").GetHashCode().Should().Be(new AgentGlyph("Da").GetHashCode());
    }

    [Fact]
    public void Should_BeSymmetric_When_ComparingEqualInstances()
    {
        var a = new AgentGlyph("Da");
        var b = new AgentGlyph("Da");

        a.Equals(b).Should().Be(b.Equals(a));
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithNull()
    {
        new AgentGlyph("Da").Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithStringOfSameValue()
    {
        new AgentGlyph("Da").Equals("Da").Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnValue_When_ToStringIsCalled()
    {
        new AgentGlyph("Da").ToString().Should().Be("Da");
    }
}
