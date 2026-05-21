using AgentDashboard.TicketTracking.Domain.Boards;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Boards;

public sealed class BoardColumnIdTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void ConstructorRejectsEmpty(string value)
    {
        var act = () => new BoardColumnId(value);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConstructorRejectsNull()
    {
        var act = () => new BoardColumnId(null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConstructorAcceptsNonEmpty()
    {
        new BoardColumnId("CREATED").Value.Should().Be("CREATED");
    }

    [Fact]
    public void ConstructorAcceptsValueAtMaxLength()
    {
        var maxLength = new string('a', 64);
        new BoardColumnId(maxLength).Value.Should().Be(maxLength);
    }

    [Fact]
    public void ConstructorRejectsValueOverMaxLength()
    {
        var tooLong = new string('a', 65);
        var act = () => new BoardColumnId(tooLong);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EqualityIsByValue()
    {
        new BoardColumnId("CREATED").Should().Be(new BoardColumnId("CREATED"));
        new BoardColumnId("CREATED").Should().NotBe(new BoardColumnId("DONE"));
    }

    [Fact]
    public void ToStringReturnsValue()
    {
        new BoardColumnId("CREATED").ToString().Should().Be("CREATED");
    }
}
