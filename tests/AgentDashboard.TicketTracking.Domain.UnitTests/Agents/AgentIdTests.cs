using AgentDashboard.TicketTracking.Domain.Agents;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Agents;

public sealed class AgentIdTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void ConstructorRejectsEmptyOrWhitespace(string value)
    {
        var act = () => new AgentId(value);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConstructorRejectsNull()
    {
        var act = () => new AgentId(null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConstructorAcceptsNonEmptyValue()
    {
        var id = new AgentId("DA");
        id.Value.Should().Be("DA");
    }

    [Fact]
    public void ConstructorAcceptsValueAtMaxLength()
    {
        var maxLength = new string('a', 64);
        new AgentId(maxLength).Value.Should().Be(maxLength);
    }

    [Fact]
    public void ConstructorRejectsValueOverMaxLength()
    {
        var tooLong = new string('a', 65);
        var act = () => new AgentId(tooLong);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EqualityIsByValue()
    {
        new AgentId("DA").Should().Be(new AgentId("DA"));
        new AgentId("DA").Should().NotBe(new AgentId("DB"));
    }

    [Fact]
    public void ToStringReturnsValue()
    {
        new AgentId("DA").ToString().Should().Be("DA");
    }
}
