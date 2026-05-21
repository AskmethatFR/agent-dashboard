using AgentDashboard.TicketTracking.Domain.Agents;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Agents;

public sealed class AgentTests
{
    [Fact]
    public void Should_Throw_ArgumentNullException_When_IdIsNull()
    {
        var act = () => new Agent(
            null!,
            new AgentName("DevA"),
            new AgentGlyph("Da"),
            new AgentRole("Developer A"));

        act.Should().ThrowExactly<ArgumentNullException>()
            .WithParameterName("id");
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_NameIsNull()
    {
        var act = () => new Agent(
            new AgentId("DA"),
            null!,
            new AgentGlyph("Da"),
            new AgentRole("Developer A"));

        act.Should().ThrowExactly<ArgumentNullException>()
            .WithParameterName("name");
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_GlyphIsNull()
    {
        var act = () => new Agent(
            new AgentId("DA"),
            new AgentName("DevA"),
            null!,
            new AgentRole("Developer A"));

        act.Should().ThrowExactly<ArgumentNullException>()
            .WithParameterName("glyph");
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_RoleIsNull()
    {
        var act = () => new Agent(
            new AgentId("DA"),
            new AgentName("DevA"),
            new AgentGlyph("Da"),
            null!);

        act.Should().ThrowExactly<ArgumentNullException>()
            .WithParameterName("role");
    }

    [Fact]
    public void Should_ExposeAllProperties_When_Built()
    {
        var agent = new AgentBuilder()
            .WithId("DA")
            .WithName("DevA")
            .WithGlyph("Da")
            .WithRole("Developer A")
            .Build();

        agent.Id.Should().Be(new AgentId("DA"));
        agent.Name.Should().Be(new AgentName("DevA"));
        agent.Glyph.Should().Be(new AgentGlyph("Da"));
        agent.Role.Should().Be(new AgentRole("Developer A"));
    }

    [Fact]
    public void Should_BeEqual_When_TwoAgentsHaveSameProperties()
    {
        var first = new AgentBuilder().Build();
        var second = new AgentBuilder().Build();

        first.Should().Be(second);
    }

    [Fact]
    public void Should_NotBeEqual_When_IdsDiffer()
    {
        var first = new AgentBuilder().WithId("DA").Build();
        var second = new AgentBuilder().WithId("DB").Build();

        first.Should().NotBe(second);
    }

    [Fact]
    public void Should_NotBeEqual_When_NamesDiffer()
    {
        var first = new AgentBuilder().WithName("DevA").Build();
        var second = new AgentBuilder().WithName("DevX").Build();

        first.Should().NotBe(second);
    }

    [Fact]
    public void Should_ProduceEqualHashCodes_When_TwoAgentsHaveSameProperties()
    {
        var first = new AgentBuilder().Build();
        var second = new AgentBuilder().Build();

        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    [Fact]
    public void Should_BeSymmetric_When_ComparingTwoEqualAgents()
    {
        var first = new AgentBuilder().Build();
        var second = new AgentBuilder().Build();

        first.Equals(second).Should().Be(second.Equals(first));
        first.Equals(second).Should().BeTrue();
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithNull()
    {
        var agent = new AgentBuilder().Build();

        agent.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithDifferentType()
    {
        var agent = new AgentBuilder().Build();

        agent.Equals("DevA").Should().BeFalse();
    }
}
