#pragma warning disable IDE0005
using FluentAssertions;
using Xunit;
#pragma warning restore IDE0005

namespace AgentDashboard.TicketTracking.Application.UnitTests.Queries.GetBoard.Dtos;

public sealed class TicketDtoTests
{
    [Fact]
    public void Should_AcceptRetryCount_When_WithinValidRange()
    {
        var dto = new global::AgentDashboard.TicketTracking.Application.Queries.GetBoard.Dtos.TicketDto("Test", "", false, 2);
        dto.RetryCount.Should().Be(2);
    }

    [Fact]
    public void Should_AcceptRetryCount_When_Zero()
    {
        var dto = new global::AgentDashboard.TicketTracking.Application.Queries.GetBoard.Dtos.TicketDto("Test", "", false, 0);
        dto.RetryCount.Should().Be(0);
    }

    [Fact]
    public void Should_AcceptRetryCount_When_AtMax()
    {
        var dto = new global::AgentDashboard.TicketTracking.Application.Queries.GetBoard.Dtos.TicketDto("Test", "", false, 3);
        dto.RetryCount.Should().Be(3);
    }

    [Fact]
    public void Should_HaveDefaultRetryCount_When_NotSpecified()
    {
        var dto = new global::AgentDashboard.TicketTracking.Application.Queries.GetBoard.Dtos.TicketDto("Test");
        dto.RetryCount.Should().Be(0);
    }
}
