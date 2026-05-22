using AgentDashboard.TicketTracking.Domain.Boards;
using AgentDashboard.TicketTracking.Domain.UnitTests.Contracts;
using AgentDashboard.TicketTracking.TestShared.Boards;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Boards;

public sealed class BoardColumnTests : RecordEqualityContract<BoardColumn>
{
    protected override BoardColumn NewInstance() => BoardColumnFixtures.Build();

    [Fact]
    public void Should_Throw_ArgumentNullException_When_IdIsNull()
    {
        var act = () => new BoardColumn(null!, new BoardColumnLabel("Created"));

        act.Should().ThrowExactly<ArgumentNullException>()
            .WithParameterName("id");
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_LabelIsNull()
    {
        var act = () => new BoardColumn(new BoardColumnId("CREATED"), null!);

        act.Should().ThrowExactly<ArgumentNullException>()
            .WithParameterName("label");
    }

    [Fact]
    public void Should_ExposeAllProperties_When_Built()
    {
        var column = new BoardColumn(
            new BoardColumnId("CREATED"),
            new BoardColumnLabel("Created"));

        column.Id.Should().Be(new BoardColumnId("CREATED"));
        column.Label.Should().Be(new BoardColumnLabel("Created"));
    }

    [Fact]
    public void Should_NotBeEqual_When_IdsDiffer()
    {
        var first = new BoardColumn(new BoardColumnId("CREATED"), new BoardColumnLabel("Created"));
        var second = new BoardColumn(new BoardColumnId("DONE"), new BoardColumnLabel("Created"));

        first.Should().NotBe(second);
    }

    [Fact]
    public void Should_NotBeEqual_When_LabelsDiffer()
    {
        var first = new BoardColumn(new BoardColumnId("CREATED"), new BoardColumnLabel("Created"));
        var second = new BoardColumn(new BoardColumnId("CREATED"), new BoardColumnLabel("Started"));

        first.Should().NotBe(second);
    }
}
