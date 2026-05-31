namespace AgentDashboard.TicketTracking.Domain.UnitTests.Tickets;

using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Tickets;

public sealed class TicketTests
{
    private static readonly DateTimeOffset FixedTimestamp = new(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
    private static readonly TimestampUtc FixedUtc = new(FixedTimestamp);

    private static Ticket CreateTestTicket(
        long issueNumber = 6,
        string title = "Test Issue",
        TicketStatusValue status = TicketStatusValue.Created,
        string agentId = "agent:dev-a",
        int retryCount = 0,
        string githubUrl = "https://github.com/AskmethatFR/agent-dashboard/issues/6")
    {
        var gitHubIssueNumber = new GitHubIssueNumber(issueNumber);
        var ticketTitle = new TicketTitle(title);
        var ticketStatus = new TicketStatus(status);
        var agent = new AgentId(agentId);
        var retry = new Retry(retryCount);
        var url = new GitHubUrl(githubUrl);

        return new Ticket(
            gitHubIssueNumber,
            ticketTitle,
            ticketStatus,
            agent,
            retry,
            url,
            FixedUtc,
            FixedUtc,
            null);
    }

    private static Ticket CreateTestTicketWithoutAgent(long issueNumber = 6)
    {
        var gitHubIssueNumber = new GitHubIssueNumber(issueNumber);
        var ticketTitle = new TicketTitle("Test Issue");
        var ticketStatus = new TicketStatus(TicketStatusValue.Created);
        var retry = new Retry(0);
        var url = new GitHubUrl("https://github.com/AskmethatFR/agent-dashboard/issues/6");

        return new Ticket(
            gitHubIssueNumber,
            ticketTitle,
            ticketStatus,
            null,
            retry,
            url,
            FixedUtc,
            FixedUtc,
            null);
    }

    [Fact]
    public void Ctor_WithValidProperties_ReturnsTicket()
    {
        var ticket = CreateTestTicket();
        Assert.NotNull(ticket);
        Assert.Equal(6L, ticket.GitHubIssueNumber.Value);
        Assert.Equal("Test Issue", ticket.TicketTitle.Value);
        Assert.Equal(TicketStatusValue.Created, ticket.TicketStatus.Value);
        Assert.NotNull(ticket.AgentId);
        Assert.Equal("agent:dev-a", ticket.AgentId.Value);
        Assert.Equal(0, ticket.RetryCount.Value);
    }

    [Fact]
    public void Ctor_WithNullAgent_ReturnsTicketWithNullAgent()
    {
        var ticket = CreateTestTicketWithoutAgent();
        Assert.Null(ticket.AgentId);
    }

    [Fact]
    public void Equals_SameIssueNumber_ReturnsTrue()
    {
        var ticket1 = CreateTestTicket();
        var ticket2 = CreateTestTicket();
        Assert.Equal(ticket1, ticket2);
        Assert.True(ticket1 == ticket2);
    }

    [Fact]
    public void Equals_DifferentIssueNumber_ReturnsFalse()
    {
        var ticket1 = CreateTestTicket(issueNumber: 6);
        var ticket2 = CreateTestTicket(issueNumber: 7);
        Assert.NotEqual(ticket1, ticket2);
        Assert.False(ticket1 == ticket2);
    }

    [Fact]
    public void Equals_DifferentOtherProperties_SameIssueNumber_ReturnsTrue()
    {
        var ticket1 = CreateTestTicket(title: "Title 1");
        var ticket2 = CreateTestTicket(title: "Title 2");
        Assert.Equal(ticket1, ticket2);
        Assert.True(ticket1 == ticket2);
    }

    [Fact]
    public void GetHashCode_SameIssueNumber_ReturnsSameHashCode()
    {
        var ticket1 = CreateTestTicket();
        var ticket2 = CreateTestTicket();
        Assert.Equal(ticket1.GetHashCode(), ticket2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentIssueNumber_ReturnsDifferentHashCode()
    {
        var ticket1 = CreateTestTicket();
        var ticket2 = CreateTestTicket(issueNumber: 7);
        Assert.NotEqual(ticket1.GetHashCode(), ticket2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsIssueNumberFormat()
    {
        var ticket = new Ticket(
            new GitHubIssueNumber(42),
            new TicketTitle("Test"),
            new TicketStatus(TicketStatusValue.Created),
            null,
            new Retry(0),
            new GitHubUrl("https://github.com/Owner/Repo/issues/42"),
            FixedUtc,
            FixedUtc,
            null);
        var str = ticket.ToString();
        Assert.Contains("#42", str);
        Assert.DoesNotContain("Owner/Repo", str);
    }

    [Fact]
    public void Equals_NullOperands_ReturnsFalse()
    {
#nullable disable
        var ticket = CreateTestTicket();

        Assert.False(ticket.Equals(null));
        Assert.False((null as Ticket) == ticket);
        Assert.False(ticket == (null as Ticket));
#nullable enable
    }

    // Test list — mutant kills
    // 1. Ticket.cs L72: null ticketTitle guard
    // 2. Ticket.cs L76: null createdAtUtc guard
    // 3. Ticket.cs L77: null updatedAtUtc guard
    // 4. Ticket.cs L109: != operator NoCoverage

    [Fact]
    public void Ctor_NullTicketTitle_ThrowsArgumentNullException()
    {
        var act = () => new Ticket(
            new GitHubIssueNumber(1),
            null!,
            new TicketStatus(TicketStatusValue.Created),
            null,
            new Retry(0),
            new GitHubUrl("https://github.com/O/R/issues/1"),
            FixedUtc,
            FixedUtc,
            null);

        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("ticketTitle");
    }

    [Fact]
    public void Ctor_NullCreatedAtUtc_ThrowsArgumentNullException()
    {
        var act = () => new Ticket(
            new GitHubIssueNumber(1),
            new TicketTitle("t"),
            new TicketStatus(TicketStatusValue.Created),
            null,
            new Retry(0),
            new GitHubUrl("https://github.com/O/R/issues/1"),
            null!,
            FixedUtc,
            null);

        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("createdAtUtc");
    }

    [Fact]
    public void Ctor_NullUpdatedAtUtc_ThrowsArgumentNullException()
    {
        var act = () => new Ticket(
            new GitHubIssueNumber(1),
            new TicketTitle("t"),
            new TicketStatus(TicketStatusValue.Created),
            null,
            new Retry(0),
            new GitHubUrl("https://github.com/O/R/issues/1"),
            FixedUtc,
            null!,
            null);

        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("updatedAtUtc");
    }

    [Fact]
    public void NotEqualOperator_DifferentIssueNumbers_ReturnsTrue()
    {
        var ticket1 = CreateTestTicket(issueNumber: 1);
        var ticket2 = CreateTestTicket(issueNumber: 2);

        (ticket1 != ticket2).Should().BeTrue();
    }

    [Fact]
    public void NotEqualOperator_SameIssueNumber_ReturnsFalse()
    {
        var ticket1 = CreateTestTicket(issueNumber: 5);
        var ticket2 = CreateTestTicket(issueNumber: 5);

        (ticket1 != ticket2).Should().BeFalse();
    }
}
