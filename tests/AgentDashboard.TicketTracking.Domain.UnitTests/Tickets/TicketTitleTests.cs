using AgentDashboard.TicketTracking.Domain.Tickets;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Tickets;

public sealed class TicketTitleTests
{
    [Fact]
    public void Should_Throw_ArgumentNullException_When_ValueIsNull()
    {
        var act = () => new TicketTitle(null!);

        act.Should().ThrowExactly<ArgumentNullException>()
            .WithParameterName("value");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Should_Throw_ArgumentException_When_ValueIsEmptyOrWhitespace(string input)
    {
        var act = () => new TicketTitle(input);

        act.Should().ThrowExactly<ArgumentException>()
            .WithParameterName("value");
    }

    [Fact]
    public void Should_Accept_When_ValueLengthIsOne()
    {
        new TicketTitle("a").Value.Should().Be("a");
    }

    [Fact]
    public void Should_Accept_When_ValueLengthIsExactlyMaxLength()
    {
        var atMax = new string('a', TicketTitle.MaxLength);

        new TicketTitle(atMax).Value.Should().Be(atMax);
    }

    [Fact]
    public void Should_Throw_ArgumentOutOfRangeException_When_ValueLengthIsAboveMaxLength()
    {
        var tooLong = new string('a', TicketTitle.MaxLength + 1);

        var act = () => new TicketTitle(tooLong);

        act.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .WithParameterName("value");
    }

    [Fact]
    public void Should_ExposeMaxLength_As_StaticReadonly_NotConst()
    {
        var field = typeof(TicketTitle).GetField(nameof(TicketTitle.MaxLength));

        field.Should().NotBeNull();
        field!.IsInitOnly.Should().BeTrue();
        field.IsLiteral.Should().BeFalse();
    }

    [Fact]
    public void Should_HaveMaxLength_Of_512()
    {
        TicketTitle.MaxLength.Should().Be(512);
    }

    [Theory]
    [InlineData("hello\nworld")]
    [InlineData("carriage\rreturn")]
    [InlineData("null\0byte")]
    [InlineData("bellhere")]
    public void Should_Throw_ArgumentException_When_ValueContainsControlCharacters(string input)
    {
        var act = () => new TicketTitle(input);

        act.Should().ThrowExactly<ArgumentException>()
            .WithParameterName("value");
    }

    [Fact]
    public void Should_Accept_When_ValueContainsTabCharacter()
    {
        var withTab = "tab\there";

        new TicketTitle(withTab).Value.Should().Be(withTab);
    }

    [Fact]
    public void Should_PreserveValue_WithoutTrimming_When_ValueHasSurroundingSpaces()
    {
        new TicketTitle(" implement feature ").Value.Should().Be(" implement feature ");
    }

    [Fact]
    public void Should_BeCaseSensitive_When_ComparingTwoInstances()
    {
        new TicketTitle("Implement").Should().NotBe(new TicketTitle("implement"));
    }

    [Fact]
    public void Should_BeEqual_When_TwoInstancesHaveSameValue()
    {
        new TicketTitle("implement feature").Should().Be(new TicketTitle("implement feature"));
    }

    [Fact]
    public void Should_NotBeEqual_When_TwoInstancesHaveDifferentValues()
    {
        new TicketTitle("implement").Should().NotBe(new TicketTitle("refactor"));
    }

    [Fact]
    public void Should_ProduceEqualHashCodes_When_TwoInstancesHaveSameValue()
    {
        new TicketTitle("implement").GetHashCode()
            .Should().Be(new TicketTitle("implement").GetHashCode());
    }

    [Fact]
    public void Should_BeSymmetric_When_ComparingEqualInstances()
    {
        var a = new TicketTitle("implement");
        var b = new TicketTitle("implement");

        a.Equals(b).Should().Be(b.Equals(a));
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithNull()
    {
        new TicketTitle("implement").Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithStringOfSameValue()
    {
        new TicketTitle("implement").Equals("implement").Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnValue_When_ToStringIsCalled()
    {
        new TicketTitle("implement").ToString().Should().Be("implement");
    }
}
