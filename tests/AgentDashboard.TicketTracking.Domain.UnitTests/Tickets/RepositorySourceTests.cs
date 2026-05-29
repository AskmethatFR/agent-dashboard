namespace AgentDashboard.TicketTracking.Domain.UnitTests.Tickets;

using AgentDashboard.TicketTracking.Domain.Tickets;

public sealed class RepositorySourceTests
{
    [Fact]
    public void Ctor_WithValidOwnerAndName_ReturnsRepositorySource()
    {
        var value = "AskmethatFR/agent-dashboard";
        var repo = new RepositorySource(value);
        Assert.Equal(value, repo.Value);
        Assert.Equal("AskmethatFR", repo.Owner);
        Assert.Equal("agent-dashboard", repo.Name);
    }

    [Fact]
    public void Ctor_WithNull_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new RepositorySource(null!));
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Ctor_WithEmptyString_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new RepositorySource(string.Empty));
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Ctor_WithWhiteSpace_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new RepositorySource("   "));
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Ctor_WithMissingSlash_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new RepositorySource("AskmethatFR"));
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Ctor_WithEmptyOwner_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new RepositorySource("/agent-dashboard"));
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Ctor_WithEmptyName_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new RepositorySource("AskmethatFR/"));
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var repo1 = new RepositorySource("AskmethatFR/agent-dashboard");
        var repo2 = new RepositorySource("AskmethatFR/agent-dashboard");
        Assert.Equal(repo1, repo2);
        Assert.True(repo1 == repo2);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        var repo1 = new RepositorySource("AskmethatFR/agent-dashboard");
        var repo2 = new RepositorySource("OtherOwner/agent-dashboard");
        Assert.NotEqual(repo1, repo2);
        Assert.False(repo1 == repo2);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var repo = new RepositorySource("AskmethatFR/agent-dashboard");
        Assert.Equal("AskmethatFR/agent-dashboard", repo.ToString());
    }

    [Fact]
    public void ImplicitConversion_FromString_Works()
    {
        RepositorySource repo = "AskmethatFR/agent-dashboard";
        Assert.Equal("AskmethatFR/agent-dashboard", repo.Value);
    }

    [Fact]
    public void ImplicitConversion_ToString_Works()
    {
        var repo = new RepositorySource("AskmethatFR/agent-dashboard");
        string result = repo;
        Assert.Equal("AskmethatFR/agent-dashboard", result);
    }
}
