using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.UnitTests.Contracts;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Agents;

public sealed class AgentRoleTests : ConstrainedStringValueObjectContract<AgentRole>
{
    protected override AgentRole Create(string value) => new(value);

    protected override string ValueOf(AgentRole instance) => instance.Value;

    protected override int ExpectedMaxLength => 64;

    protected override string SampleValue => "Developer";

    protected override string OtherSampleValue => "QA";

    protected override string SampleValueAlternateCase => "developer";

    [Fact]
    public void Should_HaveMaxLength_Of_64()
    {
        AgentRole.MaxLength.Should().Be(64);
    }
}
