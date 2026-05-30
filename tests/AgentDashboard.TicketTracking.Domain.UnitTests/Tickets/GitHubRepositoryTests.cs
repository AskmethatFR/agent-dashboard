namespace AgentDashboard.TicketTracking.Domain.UnitTests.Tickets;

using AgentDashboard.TicketTracking.Domain.Tickets;

public sealed class GitHubRepositoryTests
{
    [Fact]
    public void Ctor_WithValidOwnerAndName_ReturnsGitHubRepository()
    {
        var value = "AskmethatFR/agent-dashboard";
        var repo = new GitHubRepository(value);
        Assert.Equal(value, repo.Value);
        Assert.Equal("AskmethatFR", repo.Owner);
        Assert.Equal("agent-dashboard", repo.Name);
    }

    [Fact]
    public void Ctor_WithNull_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new GitHubRepository(null!));
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Ctor_WithEmptyString_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new GitHubRepository(string.Empty));
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Ctor_WithWhiteSpace_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new GitHubRepository("   "));
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Ctor_WithMissingSlash_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new GitHubRepository("AskmethatFR"));
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Ctor_WithEmptyOwner_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new GitHubRepository("/agent-dashboard"));
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Ctor_WithEmptyName_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new GitHubRepository("AskmethatFR/"));
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var repo1 = new GitHubRepository("AskmethatFR/agent-dashboard");
        var repo2 = new GitHubRepository("AskmethatFR/agent-dashboard");
        Assert.Equal(repo1, repo2);
        Assert.True(repo1 == repo2);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        var repo1 = new GitHubRepository("AskmethatFR/agent-dashboard");
        var repo2 = new GitHubRepository("OtherOwner/agent-dashboard");
        Assert.NotEqual(repo1, repo2);
        Assert.False(repo1 == repo2);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var repo = new GitHubRepository("AskmethatFR/agent-dashboard");
        Assert.Equal("AskmethatFR/agent-dashboard", repo.ToString());
    }

    [Fact]
    public void ImplicitConversion_FromString_Works()
    {
        GitHubRepository repo = "AskmethatFR/agent-dashboard";
        Assert.Equal("AskmethatFR/agent-dashboard", repo.Value);
    }

    [Fact]
    public void ImplicitConversion_ToString_Works()
    {
        var repo = new GitHubRepository("AskmethatFR/agent-dashboard");
        string result = repo;
        Assert.Equal("AskmethatFR/agent-dashboard", result);
    }
}
