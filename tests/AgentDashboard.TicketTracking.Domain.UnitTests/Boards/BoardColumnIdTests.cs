using AgentDashboard.TicketTracking.Domain.Boards;
using AgentDashboard.TicketTracking.Domain.UnitTests.Contracts;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Boards;

public sealed class BoardColumnIdTests : ConstrainedStringValueObjectContract<BoardColumnId>
{
    protected override BoardColumnId Create(string value) => new(value);

    protected override string ValueOf(BoardColumnId instance) => instance.Value;

    protected override int ExpectedMaxLength => 64;

    protected override string SampleValue => "CREATED";

    protected override string OtherSampleValue => "DONE";

    protected override string SampleValueAlternateCase => "created";

    [Fact]
    public void Should_HaveMaxLength_Of_64()
    {
        BoardColumnId.MaxLength.Should().Be(64);
    }
}
