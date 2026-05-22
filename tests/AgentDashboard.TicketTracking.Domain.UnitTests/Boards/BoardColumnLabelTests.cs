using AgentDashboard.TicketTracking.Domain.Boards;
using AgentDashboard.TicketTracking.Domain.UnitTests.Contracts;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Boards;

public sealed class BoardColumnLabelTests : ConstrainedStringValueObjectContract<BoardColumnLabel>
{
    protected override BoardColumnLabel Create(string value) => new(value);

    protected override string ValueOf(BoardColumnLabel instance) => instance.Value;

    protected override int ExpectedMaxLength => 128;

    protected override string SampleValue => "Created";

    protected override string OtherSampleValue => "Done";

    protected override string SampleValueAlternateCase => "created";

    [Fact]
    public void Should_HaveMaxLength_Of_128()
    {
        BoardColumnLabel.MaxLength.Should().Be(128);
    }
}
