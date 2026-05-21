using AgentDashboard.TicketTracking.Domain.Boards;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Boards;

public sealed class BoardColumnLabelTests
{
    [Fact]
    public void Should_Throw_ArgumentNullException_When_ValueIsNull()
    {
        var act = () => new BoardColumnLabel(null!);

        act.Should().ThrowExactly<ArgumentNullException>()
            .WithParameterName("value");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Should_Throw_ArgumentException_When_ValueIsEmptyOrWhitespace(string input)
    {
        var act = () => new BoardColumnLabel(input);

        act.Should().ThrowExactly<ArgumentException>()
            .WithParameterName("value");
    }

    [Fact]
    public void Should_Accept_When_ValueLengthIsOne()
    {
        new BoardColumnLabel("a").Value.Should().Be("a");
    }

    [Fact]
    public void Should_Accept_When_ValueLengthIsExactlyMaxLength()
    {
        var atMax = new string('l', BoardColumnLabel.MaxLength);

        new BoardColumnLabel(atMax).Value.Should().Be(atMax);
    }

    [Fact]
    public void Should_Throw_ArgumentOutOfRangeException_When_ValueLengthIsAboveMaxLength()
    {
        var tooLong = new string('l', BoardColumnLabel.MaxLength + 1);

        var act = () => new BoardColumnLabel(tooLong);

        act.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .WithParameterName("value");
    }

    [Fact]
    public void Should_ExposeMaxLength_As_StaticReadonly_NotConst()
    {
        var field = typeof(BoardColumnLabel).GetField(nameof(BoardColumnLabel.MaxLength));

        field.Should().NotBeNull();
        field!.IsInitOnly.Should().BeTrue();
        field.IsLiteral.Should().BeFalse();
    }

    [Fact]
    public void Should_HaveMaxLength_Of_128()
    {
        BoardColumnLabel.MaxLength.Should().Be(128);
    }

    [Fact]
    public void Should_PreserveValue_WithoutTrimming_When_ValueHasSurroundingSpaces()
    {
        new BoardColumnLabel(" Created ").Value.Should().Be(" Created ");
    }

    [Fact]
    public void Should_BeCaseSensitive_When_ComparingTwoInstances()
    {
        new BoardColumnLabel("Created").Should().NotBe(new BoardColumnLabel("created"));
    }

    [Fact]
    public void Should_BeEqual_When_TwoInstancesHaveSameValue()
    {
        new BoardColumnLabel("Created").Should().Be(new BoardColumnLabel("Created"));
    }

    [Fact]
    public void Should_NotBeEqual_When_TwoInstancesHaveDifferentValues()
    {
        new BoardColumnLabel("Created").Should().NotBe(new BoardColumnLabel("Done"));
    }

    [Fact]
    public void Should_ProduceEqualHashCodes_When_TwoInstancesHaveSameValue()
    {
        new BoardColumnLabel("Created").GetHashCode()
            .Should().Be(new BoardColumnLabel("Created").GetHashCode());
    }

    [Fact]
    public void Should_BeSymmetric_When_ComparingEqualInstances()
    {
        var a = new BoardColumnLabel("Created");
        var b = new BoardColumnLabel("Created");

        a.Equals(b).Should().Be(b.Equals(a));
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithNull()
    {
        new BoardColumnLabel("Created").Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithStringOfSameValue()
    {
        new BoardColumnLabel("Created").Equals("Created").Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnValue_When_ToStringIsCalled()
    {
        new BoardColumnLabel("Created").ToString().Should().Be("Created");
    }
}
