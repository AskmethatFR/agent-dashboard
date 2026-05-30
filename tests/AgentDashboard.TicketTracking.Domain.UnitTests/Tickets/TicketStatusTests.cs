namespace AgentDashboard.TicketTracking.Domain.UnitTests.Tickets;

using AgentDashboard.TicketTracking.Domain.Tickets;

public sealed class TicketStatusTests
{
    [Fact]
    public void Ctor_WithCreated_ReturnsTicketStatus()
    {
        var status = new TicketStatus(TicketStatusValue.Created);
        Assert.Equal(TicketStatusValue.Created, status.Value);
    }

    [Fact]
    public void Parse_WithCreatedString_ReturnsCreated()
    {
        var status = TicketStatus.Parse("Created");
        Assert.Equal(TicketStatusValue.Created, status.Value);
    }

    [Fact]
    public void Parse_WithStatusPrefix_ReturnsCorrectStatus()
    {
        var status = TicketStatus.Parse("status:specified");
        Assert.Equal(TicketStatusValue.Specified, status.Value);
    }

    [Fact]
    public void Parse_WithNull_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => TicketStatus.Parse(null!));
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Parse_WithEmpty_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => TicketStatus.Parse(string.Empty));
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Parse_WithInvalidValue_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => TicketStatus.Parse("InvalidStatus"));
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var status1 = new TicketStatus(TicketStatusValue.Created);
        var status2 = new TicketStatus(TicketStatusValue.Created);
        Assert.Equal(status1, status2);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        var status1 = new TicketStatus(TicketStatusValue.Created);
        var status2 = new TicketStatus(TicketStatusValue.Done);
        Assert.NotEqual(status1, status2);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var status = new TicketStatus(TicketStatusValue.Created);
        Assert.Equal("Created", status.ToString());
    }

    [Fact]
    public void ImplicitConversion_FromEnum_Works()
    {
        TicketStatus status = TicketStatusValue.Created;
        Assert.Equal(TicketStatusValue.Created, status.Value);
    }

    [Fact]
    public void ImplicitConversion_ToEnum_Works()
    {
        var status = new TicketStatus(TicketStatusValue.Created);
        TicketStatusValue result = status;
        Assert.Equal(TicketStatusValue.Created, result);
    }
}
