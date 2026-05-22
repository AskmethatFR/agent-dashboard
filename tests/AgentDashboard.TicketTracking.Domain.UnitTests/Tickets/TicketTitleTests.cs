using AgentDashboard.TicketTracking.Domain.Tickets;
using AgentDashboard.TicketTracking.Domain.UnitTests.Contracts;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Tickets;

public sealed class TicketTitleTests : ConstrainedStringValueObjectContract<TicketTitle>
{
    protected override TicketTitle Create(string value) => new(value);

    protected override string ValueOf(TicketTitle instance) => instance.Value;

    protected override int ExpectedMaxLength => 512;

    protected override string SampleValue => "implement";

    protected override string OtherSampleValue => "refactor";

    protected override string SampleValueAlternateCase => "Implement";

    [Fact]
    public void Should_HaveMaxLength_Of_512()
    {
        TicketTitle.MaxLength.Should().Be(512);
    }

    [Theory]
    [InlineData("hello\nworld")]
    [InlineData("carriage\rreturn")]
    [InlineData("null\0byte")]
    [InlineData("bell\ahere")]
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
}
