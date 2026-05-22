using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.UnitTests.Contracts;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Agents;

public sealed class AgentNameTests : ConstrainedStringValueObjectContract<AgentName>
{
    protected override AgentName Create(string value) => new(value);

    protected override string ValueOf(AgentName instance) => instance.Value;

    protected override int ExpectedMaxLength => 128;

    protected override string SampleValue => "DevA";

    protected override string OtherSampleValue => "DevB";

    protected override string SampleValueAlternateCase => "deva";

    [Fact]
    public void Should_HaveMaxLength_Of_128()
    {
        AgentName.MaxLength.Should().Be(128);
    }
}
