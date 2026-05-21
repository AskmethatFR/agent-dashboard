using AgentDashboard.TicketTracking.Domain.Agents;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Agents;

public sealed class AgentTests
{
    private static AgentId AnyId() => new("DA");

    [Fact]
    public void ConstructorRejectsNullId()
    {
        var act = () => new Agent(null!, "DevA", "Da", "Developer A");
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ConstructorRejectsEmptyName(string name)
    {
        var act = () => new Agent(AnyId(), name, "Da", "Developer A");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ConstructorRejectsEmptyGlyph(string glyph)
    {
        var act = () => new Agent(AnyId(), "DevA", glyph, "Developer A");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ConstructorRejectsEmptyRole(string role)
    {
        var act = () => new Agent(AnyId(), "DevA", "Da", role);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConstructorBuildsValidAgent()
    {
        var agent = new Agent(new AgentId("DA"), "DevA", "Da", "Developer A");
        agent.Id.Value.Should().Be("DA");
        agent.Name.Should().Be("DevA");
        agent.Glyph.Should().Be("Da");
        agent.Role.Should().Be("Developer A");
    }
}
