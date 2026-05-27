#pragma warning disable IDE0005 // Using directive is unnecessary
using AgentDashboard.TicketTracking.Application.Queries.GetBoard.Dtos;
using FluentAssertions;
using Xunit;
#pragma warning restore IDE0005

namespace AgentDashboard.TicketTracking.Application.UnitTests.Queries.GetBoard.Dtos;

public sealed class TicketDtoTests
{
    [Fact]
    public void HaveAllRequiredFields()
    {
        var ticket = new TicketDto(
            Title: "Test ticket",
            Id: 1,
            Age: TimeSpan.FromHours(2),
            Freshness: "Fresh",
            IsEscalated: true,
            EscalationTargetId: "dev-b",
            IsInCrossReview: true,
            CoAgentId: "dev-c");

        ticket.Id.Should().Be(1);
        ticket.Title.Should().Be("Test ticket");
        ticket.AgentId.Should().Be("");
        ticket.IsThinking.Should().BeFalse();
        ticket.RetryCount.Should().Be(0);
        ticket.Age.Should().Be(TimeSpan.FromHours(2));
        ticket.Freshness.Should().Be("Fresh");
        ticket.IsEscalated.Should().BeTrue();
        ticket.EscalationTargetId.Should().Be("dev-b");
        ticket.IsInCrossReview.Should().BeTrue();
        ticket.CoAgentId.Should().Be("dev-c");
    }

    [Fact]
    public void HaveDefaultValuesForNewFields_WhenCreatedWithTitleOnly()
    {
        var ticket = new TicketDto("Test ticket");

        ticket.Title.Should().Be("Test ticket");
        ticket.AgentId.Should().Be("");
        ticket.IsThinking.Should().BeFalse();
        ticket.RetryCount.Should().Be(0);
        
        ticket.Id.Should().Be(0);
        ticket.Age.Should().Be(TimeSpan.Zero);
        ticket.Freshness.Should().Be("Neutral");
        ticket.IsEscalated.Should().BeFalse();
        ticket.EscalationTargetId.Should().BeNull();
        ticket.IsInCrossReview.Should().BeFalse();
        ticket.CoAgentId.Should().BeNull();
    }

    [Fact]
    public void HaveCorrectAgeFormatted_WhenAgeIsZero()
    {
        var ticket = new TicketDto("Test", Age: TimeSpan.Zero);
        
        ticket.AgeFormatted.Should().Be("00:00:00");
    }

    [Fact]
    public void HaveCorrectAgeFormatted_WhenAgeIsNonZero()
    {
        var ticket = new TicketDto("Test", Age: TimeSpan.FromHours(2).Add(TimeSpan.FromMinutes(30)));
        
        ticket.AgeFormatted.Should().Be("02:30:00");
    }

    [Fact]
    public void MaintainBackwardCompatibility_WithExistingPositionalSyntax()
    {
        var ticket = new TicketDto("Fix the bug", "dev-a", false);

        ticket.Title.Should().Be("Fix the bug");
        ticket.AgentId.Should().Be("dev-a");
        ticket.IsThinking.Should().BeFalse();
        ticket.RetryCount.Should().Be(0);
    }

    [Fact]
    public void MaintainBackwardCompatibility_WithExistingNamedSyntax()
    {
        var ticket = new TicketDto("Fix the bug", "dev-a", false, RetryCount: 2);

        ticket.Title.Should().Be("Fix the bug");
        ticket.AgentId.Should().Be("dev-a");
        ticket.IsThinking.Should().BeFalse();
        ticket.RetryCount.Should().Be(2);
    }
}
