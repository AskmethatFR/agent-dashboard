namespace AgentDashboard.TicketTracking.Domain.UnitTests.Tickets;

using AgentDashboard.TicketTracking.Domain.Tickets;

public sealed class GitHubIssueNumberTests
{
    [Fact]
    public void Ctor_WithPositiveValue_ReturnsGitHubIssueNumber()
    {
        var number = new GitHubIssueNumber(6);
        Assert.Equal(6L, number.Value);
    }

    [Fact]
    public void Ctor_WithZero_ThrowsArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new GitHubIssueNumber(0));
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Ctor_WithNegative_ThrowsArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new GitHubIssueNumber(-1));
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var num1 = new GitHubIssueNumber(6);
        var num2 = new GitHubIssueNumber(6);
        Assert.Equal(num1, num2);
        Assert.True(num1 == num2);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        var num1 = new GitHubIssueNumber(6);
        var num2 = new GitHubIssueNumber(7);
        Assert.NotEqual(num1, num2);
        Assert.False(num1 == num2);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var number = new GitHubIssueNumber(6);
        Assert.Equal("6", number.ToString());
    }

    [Fact]
    public void ImplicitConversion_FromLong_Works()
    {
        GitHubIssueNumber number = 6;
        Assert.Equal(6L, number.Value);
    }

    [Fact]
    public void ImplicitConversion_ToLong_Works()
    {
        var number = new GitHubIssueNumber(6);
        long result = number;
        Assert.Equal(6L, result);
    }
}
