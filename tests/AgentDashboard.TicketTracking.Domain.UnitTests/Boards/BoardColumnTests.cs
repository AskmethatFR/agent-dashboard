using AgentDashboard.TicketTracking.Domain.Boards;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Boards;

public sealed class BoardColumnTests
{
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
    public void Should_BeEqual_When_TwoColumnsHaveSameProperties()
    {
        var first = new BoardColumn(new BoardColumnId("CREATED"), new BoardColumnLabel("Created"));
        var second = new BoardColumn(new BoardColumnId("CREATED"), new BoardColumnLabel("Created"));

        first.Should().Be(second);
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

    [Fact]
    public void Should_ProduceEqualHashCodes_When_TwoColumnsHaveSameProperties()
    {
        var first = new BoardColumn(new BoardColumnId("CREATED"), new BoardColumnLabel("Created"));
        var second = new BoardColumn(new BoardColumnId("CREATED"), new BoardColumnLabel("Created"));

        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    [Fact]
    public void Should_BeSymmetric_When_ComparingTwoEqualColumns()
    {
        var first = new BoardColumn(new BoardColumnId("CREATED"), new BoardColumnLabel("Created"));
        var second = new BoardColumn(new BoardColumnId("CREATED"), new BoardColumnLabel("Created"));

        first.Equals(second).Should().Be(second.Equals(first));
        first.Equals(second).Should().BeTrue();
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithNull()
    {
        var column = new BoardColumn(new BoardColumnId("CREATED"), new BoardColumnLabel("Created"));

        column.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithDifferentType()
    {
        var column = new BoardColumn(new BoardColumnId("CREATED"), new BoardColumnLabel("Created"));

        column.Equals("CREATED").Should().BeFalse();
    }
}
