using AgentDashboard.TicketTracking.Domain.Tickets;
using AgentDashboard.TicketTracking.Domain.UnitTests.Contracts;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Tickets;

public sealed class TicketIdTests : RecordEqualityContract<TicketId>
{
    protected override TicketId NewInstance() => new(42);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Should_Throw_ArgumentOutOfRangeException_When_ValueIsNotPositive(int input)
    {
        var act = () => new TicketId(input);

        act.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .WithParameterName("value");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(502)]
    [InlineData(int.MaxValue)]
    public void Should_Accept_When_ValueIsPositive(int input)
    {
        new TicketId(input).Value.Should().Be(input);
    }

    [Fact]
    public void Should_NotBeEqual_When_TwoInstancesHaveDifferentValues()
    {
        new TicketId(42).Should().NotBe(new TicketId(43));
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithBoxedInt()
    {
        new TicketId(42).Equals(42).Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnValueAsString_When_ToStringIsCalled()
    {
        new TicketId(1234).ToString().Should().Be("1234");
    }

    [Fact]
    public void Should_UseInvariantCulture_When_ToStringIsCalled()
    {
        var previous = System.Threading.Thread.CurrentThread.CurrentCulture;
        try
        {
            System.Threading.Thread.CurrentThread.CurrentCulture =
                System.Globalization.CultureInfo.GetCultureInfo("fr-FR");

            new TicketId(1234).ToString().Should().Be("1234");
        }
        finally
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = previous;
        }
    }
}
