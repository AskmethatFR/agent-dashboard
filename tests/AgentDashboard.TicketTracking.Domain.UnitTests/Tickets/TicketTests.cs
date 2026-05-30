namespace AgentDashboard.TicketTracking.Domain.UnitTests.Tickets;

using AgentDashboard.TicketTracking.Domain.Agents;
using AgentDashboard.TicketTracking.Domain.Tickets;

public sealed class TicketTests
{
    private static readonly DateTimeOffset FixedTimestamp = new(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
    private static readonly TimestampUtc FixedUtc = new(FixedTimestamp);

    private static Ticket CreateTestTicket(
        string repo = "AskmethatFR/agent-dashboard",
        long issueNumber = 6,
        string title = "Test Issue",
        TicketStatusValue status = TicketStatusValue.Created,
        string agentId = "agent:dev-a",
        int retryCount = 0,
        string githubUrl = "https://github.com/AskmethatFR/agent-dashboard/issues/6")
    {
        var repositorySource = new RepositorySource(repo);
        var gitHubIssueNumber = new GitHubIssueNumber(issueNumber);
        var ticketTitle = new TicketTitle(title);
        var ticketStatus = new TicketStatus(status);
        var agent = new AgentId(agentId);
        var retry = new Retry(retryCount);
        var url = new GitHubUrl(githubUrl);
        
        return new Ticket(
            repositorySource,
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

    private static Ticket CreateTestTicketWithoutAgent(
        string repo = "AskmethatFR/agent-dashboard",
        long issueNumber = 6)
    {
        var repositorySource = new RepositorySource(repo);
        var gitHubIssueNumber = new GitHubIssueNumber(issueNumber);
        var ticketTitle = new TicketTitle("Test Issue");
        var ticketStatus = new TicketStatus(TicketStatusValue.Created);
        var retry = new Retry(0);
        var url = new GitHubUrl("https://github.com/AskmethatFR/agent-dashboard/issues/6");
        
        return new Ticket(
            repositorySource,
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
        Assert.Equal("AskmethatFR/agent-dashboard", ticket.RepositorySource.Value);
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
    public void Ctor_WithNullRepositorySource_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new Ticket(
            null!,
            new GitHubIssueNumber(6),
            new TicketTitle("Test"),
            new TicketStatus(TicketStatusValue.Created),
            null,
            new Retry(0),
            new GitHubUrl("https://github.com/AskmethatFR/agent-dashboard/issues/6"),
            FixedUtc,
            FixedUtc,
            null));
        Assert.Equal("repositorySource", ex.ParamName);
    }

    [Fact]
    public void Equals_SameCompositeKey_ReturnsTrue()
    {
        var ticket1 = CreateTestTicket();
        var ticket2 = CreateTestTicket();
        Assert.Equal(ticket1, ticket2);
        Assert.True(ticket1 == ticket2);
    }

    [Fact]
    public void Equals_DifferentRepository_ReturnsFalse()
    {
        var ticket1 = CreateTestTicket(repo: "AskmethatFR/agent-dashboard");
        var ticket2 = CreateTestTicket(repo: "OtherOwner/agent-dashboard");
        Assert.NotEqual(ticket1, ticket2);
        Assert.False(ticket1 == ticket2);
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
    public void Equals_DifferentOtherProperties_SameKey_ReturnsTrue()
    {
        var ticket1 = CreateTestTicket(title: "Title 1");
        var ticket2 = CreateTestTicket(title: "Title 2");
        Assert.Equal(ticket1, ticket2);
        Assert.True(ticket1 == ticket2);
    }

    [Fact]
    public void GetHashCode_SameCompositeKey_ReturnsSameHashCode()
    {
        var ticket1 = CreateTestTicket();
        var ticket2 = CreateTestTicket();
        Assert.Equal(ticket1.GetHashCode(), ticket2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentCompositeKey_ReturnsDifferentHashCode()
    {
        var ticket1 = CreateTestTicket();
        var ticket2 = CreateTestTicket(issueNumber: 7);
        Assert.NotEqual(ticket1.GetHashCode(), ticket2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var ticket = new Ticket(
            new RepositorySource("Owner/Repo"),
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
        Assert.Contains("Owner/Repo", str);
        Assert.Contains("42", str);
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
}
