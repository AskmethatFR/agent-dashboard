using AgentDashboard.TicketTracking.Application;
using AgentDashboard.TicketTracking.Infrastructure;
using AgentDashboard.TicketTracking.Infrastructure.Boards;
using AgentDashboard.Web.Components.Pages;
using Bunit;
using FluentAssertions;
using Xunit;

namespace AgentDashboard.Web.Tests.Pages;

public class HomeShould
{
    [Fact]
    public void render_created_column_with_one_ticket_card()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddTicketTrackingApplication();
        ctx.Services.AddTicketTrackingInfrastructure();

        var cut = ctx.Render<Home>();

        cut.WaitForState(() => cut.FindAll("header").Count > 0);

        cut.Find("header").TextContent.Should().Contain("Created");
        var cards = cut.FindAll(".card");
        cards.Should().HaveCount(1);
        cards[0].TextContent.Should().Contain(StubBoardReader.SeedTitle);
    }
}
