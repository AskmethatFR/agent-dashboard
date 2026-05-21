using AgentDashboard.TicketTracking.Domain.Boards;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Boards;

public sealed class BoardColumnIdTests
{
    [Fact]
    public void Should_Throw_ArgumentNullException_When_ValueIsNull()
    {
        var act = () => new BoardColumnId(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("value");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Should_Throw_ArgumentException_When_ValueIsEmptyOrWhitespace(string input)
    {
        var act = () => new BoardColumnId(input);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("value");
    }

    [Fact]
    public void Should_Accept_When_ValueLengthIsOne()
    {
        new BoardColumnId("a").Value.Should().Be("a");
    }

    [Fact]
    public void Should_Accept_When_ValueLengthIsExactlyMaxLength()
    {
        var atMax = new string('a', BoardColumnId.MaxLength);

        new BoardColumnId(atMax).Value.Should().Be(atMax);
    }

    [Fact]
    public void Should_Throw_ArgumentOutOfRangeException_When_ValueLengthIsAboveMaxLength()
    {
        var tooLong = new string('a', BoardColumnId.MaxLength + 1);

        var act = () => new BoardColumnId(tooLong);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("value");
    }

    [Fact]
    public void Should_ExposeMaxLength_As_StaticReadonly_NotConst()
    {
        var field = typeof(BoardColumnId).GetField(nameof(BoardColumnId.MaxLength));

        field.Should().NotBeNull();
        field!.IsInitOnly.Should().BeTrue();
        field.IsLiteral.Should().BeFalse();
    }

    [Fact]
    public void Should_PreserveValue_WithoutTrimming_When_ValueHasSurroundingSpaces()
    {
        new BoardColumnId(" CREATED ").Value.Should().Be(" CREATED ");
    }

    [Fact]
    public void Should_BeCaseSensitive_When_ComparingTwoInstances()
    {
        new BoardColumnId("CREATED").Should().NotBe(new BoardColumnId("created"));
    }

    [Fact]
    public void Should_BeEqual_When_TwoInstancesHaveSameValue()
    {
        new BoardColumnId("CREATED").Should().Be(new BoardColumnId("CREATED"));
    }

    [Fact]
    public void Should_NotBeEqual_When_TwoInstancesHaveDifferentValues()
    {
        new BoardColumnId("CREATED").Should().NotBe(new BoardColumnId("DONE"));
    }

    [Fact]
    public void Should_ProduceEqualHashCodes_When_TwoInstancesHaveSameValue()
    {
        new BoardColumnId("CREATED").GetHashCode()
            .Should().Be(new BoardColumnId("CREATED").GetHashCode());
    }

    [Fact]
    public void Should_BeSymmetric_When_ComparingEqualInstances()
    {
        var a = new BoardColumnId("CREATED");
        var b = new BoardColumnId("CREATED");

        a.Equals(b).Should().Be(b.Equals(a));
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithNull()
    {
        new BoardColumnId("CREATED").Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithStringOfSameValue()
    {
        new BoardColumnId("CREATED").Equals("CREATED").Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnValue_When_ToStringIsCalled()
    {
        new BoardColumnId("CREATED").ToString().Should().Be("CREATED");
    }
}
