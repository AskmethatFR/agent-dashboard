namespace AgentDashboard.TicketTracking.Domain.UnitTests.Contracts;

public abstract class ConstrainedStringValueObjectContract<T> where T : notnull
{
    protected abstract T Create(string value);

    protected abstract string ValueOf(T instance);

    protected abstract int ExpectedMaxLength { get; }

    protected abstract string SampleValue { get; }

    protected abstract string OtherSampleValue { get; }

    protected abstract string SampleValueAlternateCase { get; }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_ValueIsNull()
    {
        var act = () => Create(null!);

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
        var act = () => Create(input);

        act.Should().ThrowExactly<ArgumentException>()
            .WithParameterName("value");
    }

    [Fact]
    public void Should_Accept_When_ValueLengthIsOne()
    {
        ValueOf(Create("a")).Should().Be("a");
    }

    [Fact]
    public void Should_Accept_When_ValueLengthIsExactlyMaxLength()
    {
        var atMax = new string('a', ExpectedMaxLength);

        ValueOf(Create(atMax)).Should().Be(atMax);
    }

    [Fact]
    public void Should_Throw_ArgumentOutOfRangeException_When_ValueLengthIsAboveMaxLength()
    {
        var tooLong = new string('a', ExpectedMaxLength + 1);

        var act = () => Create(tooLong);

        act.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .WithParameterName("value");
    }

    [Fact]
    public void Should_ExposeMaxLength_As_StaticReadonly_NotConst()
    {
        var field = typeof(T).GetField("MaxLength");

        field.Should().NotBeNull();
        field!.IsInitOnly.Should().BeTrue();
        field.IsLiteral.Should().BeFalse();
    }

    [Fact]
    public void Should_PreserveValue_WithoutTrimming_When_ValueHasSurroundingSpaces()
    {
        var padded = $" {SampleValue} ";

        ValueOf(Create(padded)).Should().Be(padded);
    }

    [Fact]
    public void Should_BeCaseSensitive_When_ComparingTwoInstances()
    {
        Create(SampleValue).Should().NotBe(Create(SampleValueAlternateCase));
    }

    [Fact]
    public void Should_BeEqual_When_TwoInstancesHaveSameValue()
    {
        Create(SampleValue).Should().Be(Create(SampleValue));
    }

    [Fact]
    public void Should_NotBeEqual_When_TwoInstancesHaveDifferentValues()
    {
        Create(SampleValue).Should().NotBe(Create(OtherSampleValue));
    }

    [Fact]
    public void Should_ProduceEqualHashCodes_When_TwoInstancesHaveSameValue()
    {
        Create(SampleValue).GetHashCode()
            .Should().Be(Create(SampleValue).GetHashCode());
    }

    [Fact]
    public void Should_BeSymmetric_When_ComparingEqualInstances()
    {
        var a = Create(SampleValue);
        var b = Create(SampleValue);

        a.Equals(b).Should().Be(b.Equals(a));
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithNull()
    {
        Create(SampleValue).Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithRawString()
    {
        Create(SampleValue).Equals(SampleValue).Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnValue_When_ToStringIsCalled()
    {
        Create(SampleValue).ToString().Should().Be(SampleValue);
    }
}
