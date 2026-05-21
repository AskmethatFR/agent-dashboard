using AgentDashboard.TicketTracking.Domain.Tickets;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Tickets;

public sealed class TicketIdTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void ConstructorRejectsNonPositive(int value)
    {
        var act = () => new TicketId(value);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(502)]
    [InlineData(int.MaxValue)]
    public void ConstructorAcceptsPositive(int value)
    {
        new TicketId(value).Value.Should().Be(value);
    }

    [Fact]
    public void EqualityIsByValue()
    {
        new TicketId(42).Should().Be(new TicketId(42));
        new TicketId(42).Should().NotBe(new TicketId(43));
    }

    [Fact]
    public void ToStringUsesInvariantCulture()
    {
        new TicketId(1234).ToString().Should().Be("1234");
    }
}
