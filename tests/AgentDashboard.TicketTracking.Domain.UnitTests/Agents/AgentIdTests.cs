using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.UnitTests.Contracts;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Agents;

public sealed class AgentIdTests : ConstrainedStringValueObjectContract<AgentId>
{
    protected override AgentId Create(string value) => new(value);

    protected override string ValueOf(AgentId instance) => instance.Value;

    protected override int ExpectedMaxLength => 64;

    protected override string SampleValue => "DA";

    protected override string OtherSampleValue => "DB";

    protected override string SampleValueAlternateCase => "da";

    [Fact]
    public void Should_HaveMaxLength_Of_64()
    {
        AgentId.MaxLength.Should().Be(64);
    }
}
