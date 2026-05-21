using System.Globalization;
using AgentDashboard.Web.Components.Pages;
using AgentDashboard.Web.Components.Pages.TeamBoard;

namespace AgentDashboard.Web.Tests.TeamBoard;

public sealed class TeamBoardPageTests : TestContext
{
    [Fact]
    public void BoardRendersSevenColumnsInCorrectOrder()
    {
        var cut = RenderComponent<Home>();

        var headings = cut.FindAll(".column-header h2");

        headings.Should().HaveCount(7);
        headings[0].TextContent.Should().Be("Created");
        headings[1].TextContent.Should().Be("Specified");
        headings[2].TextContent.Should().Be("In development");
        headings[3].TextContent.Should().Be("In review");
        headings[4].TextContent.Should().Be("In QA");
        headings[5].TextContent.Should().Be("Awaiting validation");
        headings[6].TextContent.Should().Be("Done · today");
    }

    [Fact]
    public void EachColumnShowsExpectedStubCount()
    {
        var cut = RenderComponent<Home>();

        var columnList = StubTickets.Columns.ToList();
        for (var i = 0; i < columnList.Count; i++)
        {
            var colDef = columnList[i];
            var expectedCount = StubTickets.Tickets.Count(t => t.ColumnId == colDef.Id);
            var colHeader = cut.Find($".column:nth-child({i + 1}) .column-meta");
            colHeader.TextContent.Trim().Should().Be(expectedCount.ToString(CultureInfo.InvariantCulture));
        }
    }

    [Fact]
    public void TopBarShowsBrand()
    {
        var cut = RenderComponent<TopBar>();

        cut.Markup.Should().Contain("team/");
    }
}
