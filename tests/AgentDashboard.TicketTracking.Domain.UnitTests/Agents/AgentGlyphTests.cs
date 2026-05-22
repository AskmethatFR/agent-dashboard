using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.UnitTests.Contracts;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Agents;

public sealed class AgentGlyphTests : ConstrainedStringValueObjectContract<AgentGlyph>
{
    protected override AgentGlyph Create(string value) => new(value);

    protected override string ValueOf(AgentGlyph instance) => instance.Value;

    protected override int ExpectedMaxLength => 8;

    protected override string SampleValue => "Da";

    protected override string OtherSampleValue => "Db";

    protected override string SampleValueAlternateCase => "da";

    [Fact]
    public void Should_HaveMaxLength_Of_8()
    {
        AgentGlyph.MaxLength.Should().Be(8);
    }
}
