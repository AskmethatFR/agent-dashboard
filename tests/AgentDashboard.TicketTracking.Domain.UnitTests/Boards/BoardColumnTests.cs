using AgentDashboard.TicketTracking.Domain.Boards;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Boards;

public sealed class BoardColumnTests
{
    [Fact]
    public void ConstructorRejectsNullId()
    {
        var act = () => new BoardColumn(null!, "Created");
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ConstructorRejectsEmptyLabel(string label)
    {
        var act = () => new BoardColumn(new BoardColumnId("CREATED"), label);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConstructorBuildsValidColumn()
    {
        var col = new BoardColumn(new BoardColumnId("CREATED"), "Created");
        col.Id.Value.Should().Be("CREATED");
        col.Label.Should().Be("Created");
    }

    [Fact]
    public void ConstructorAcceptsLabelAtMaxLength()
    {
        var maxLength = new string('a', 128);
        new BoardColumn(new BoardColumnId("CREATED"), maxLength).Label.Should().Be(maxLength);
    }

    [Fact]
    public void ConstructorRejectsLabelOverMaxLength()
    {
        var tooLong = new string('a', 129);
        var act = () => new BoardColumn(new BoardColumnId("CREATED"), tooLong);
        act.Should().Throw<ArgumentException>();
    }
}
