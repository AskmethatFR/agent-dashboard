using AgentDashboard.TicketTracking.Domain.Tickets;

namespace AgentDashboard.TicketTracking.Domain.UnitTests.Tickets;

public sealed class TicketIdTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Should_Throw_ArgumentOutOfRangeException_When_ValueIsNotPositive(int input)
    {
        var act = () => new TicketId(input);

        act.Should().Throw<ArgumentOutOfRangeException>()
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
    public void Should_BeEqual_When_TwoInstancesHaveSameValue()
    {
        new TicketId(42).Should().Be(new TicketId(42));
    }

    [Fact]
    public void Should_NotBeEqual_When_TwoInstancesHaveDifferentValues()
    {
        new TicketId(42).Should().NotBe(new TicketId(43));
    }

    [Fact]
    public void Should_ProduceEqualHashCodes_When_TwoInstancesHaveSameValue()
    {
        new TicketId(42).GetHashCode().Should().Be(new TicketId(42).GetHashCode());
    }

    [Fact]
    public void Should_BeSymmetric_When_ComparingEqualInstances()
    {
        var a = new TicketId(42);
        var b = new TicketId(42);

        a.Equals(b).Should().Be(b.Equals(a));
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Should_ReturnFalse_When_EqualsCalledWithNull()
    {
        new TicketId(42).Equals(null).Should().BeFalse();
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
