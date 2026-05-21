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
    [InlineData(null)]
    public void ConstructorRejectsEmptyName(string? name)
    {
        var act = () => new Agent(AnyId(), name!, "Da", "Developer A");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ConstructorRejectsEmptyGlyph(string? glyph)
    {
        var act = () => new Agent(AnyId(), "DevA", glyph!, "Developer A");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ConstructorRejectsEmptyRole(string? role)
    {
        var act = () => new Agent(AnyId(), "DevA", "Da", role!);
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

    [Fact]
    public void ConstructorAcceptsFieldsAtMaxLength()
    {
        var name = new string('n', Agent.MaxNameLength);
        var glyph = new string('g', Agent.MaxGlyphLength);
        var role = new string('r', Agent.MaxRoleLength);
        var agent = new Agent(AnyId(), name, glyph, role);
        agent.Name.Should().Be(name);
        agent.Glyph.Should().Be(glyph);
        agent.Role.Should().Be(role);
    }

    [Fact]
    public void ConstructorRejectsNameOverMaxLength()
    {
        var tooLong = new string('n', Agent.MaxNameLength + 1);
        var act = () => new Agent(AnyId(), tooLong, "Da", "Developer A");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConstructorRejectsGlyphOverMaxLength()
    {
        var tooLong = new string('g', Agent.MaxGlyphLength + 1);
        var act = () => new Agent(AnyId(), "DevA", tooLong, "Developer A");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConstructorRejectsRoleOverMaxLength()
    {
        var tooLong = new string('r', Agent.MaxRoleLength + 1);
        var act = () => new Agent(AnyId(), "DevA", "Da", tooLong);
        act.Should().Throw<ArgumentException>();
    }
}
