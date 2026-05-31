namespace AgentDashboard.TicketTracking.Domain.UnitTests.Tickets;

using AgentDashboard.TicketTracking.Domain.Tickets;

public sealed class GitHubUrlTests
{
    // Test List:
    // 1. Happy path: valid HTTPS GitHub URL
    // 2. Invalid: null URL
    // 3. Invalid: empty string
    // 4. Invalid: whitespace only
    // 5. Invalid: not HTTPS (HTTP)
    // 6. Invalid: not a URL (no scheme)
    // 7. Invalid: non-GitHub domain
    // 8. Invalid: missing path/issue
    // 9. Equality: same value equals
    // 10. Equality: different value not equals
    // 11. Equality: equals null returns false
    // 12. ToString: returns URL string

    [Fact]
    public void Ctor_WithValidHttpsGitHubIssueUrl_ReturnsGitHubUrl()
    {
        // Arrange
        var url = "https://github.com/AskmethatFR/agent-dashboard/issues/6";

        // Act
        var gitHubUrl = new GitHubUrl(url);

        // Assert
        Assert.Equal(url, gitHubUrl.Value);
    }

    [Fact]
    public void Ctor_WithValidHttpsGitHubUrl_ReturnsGitHubUrl()
    {
        // Arrange
        var url = "https://github.com/AskmethatFR/agent-dashboard";

        // Act
        var gitHubUrl = new GitHubUrl(url);

        // Assert
        Assert.Equal(url, gitHubUrl.Value);
    }

    [Fact]
    public void Ctor_WithNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new GitHubUrl(null!));
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Ctor_WithEmptyString_ThrowsArgumentException()
    {
        // Arrange
        var url = string.Empty;

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new GitHubUrl(url));
        Assert.Equal("value", ex.ParamName);
        Assert.Contains("empty", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Ctor_WithWhiteSpaceOnly_ThrowsArgumentException()
    {
        // Arrange
        var url = "   ";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new GitHubUrl(url));
        Assert.Equal("value", ex.ParamName);
        Assert.Contains("empty", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Ctor_WithHttpInsteadOfHttps_ThrowsArgumentException()
    {
        // Arrange
        var url = "http://github.com/AskmethatFR/agent-dashboard/issues/6";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new GitHubUrl(url));
        Assert.Equal("value", ex.ParamName);
        Assert.Contains("HTTPS", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Ctor_WithNonGitHubDomain_ThrowsArgumentException()
    {
        // Arrange
        var url = "https://gitlab.com/AskmethatFR/agent-dashboard/issues/6";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new GitHubUrl(url));
        Assert.Equal("value", ex.ParamName);
        Assert.Contains("GitHub", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Ctor_WithMissingPath_ThrowsArgumentException()
    {
        // Arrange
        var url = "https://github.com";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new GitHubUrl(url));
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        // Arrange
        var url = "https://github.com/AskmethatFR/agent-dashboard/issues/6";
        var gitHubUrl1 = new GitHubUrl(url);
        var gitHubUrl2 = new GitHubUrl(url);

        // Act & Assert
        Assert.Equal(gitHubUrl1, gitHubUrl2);
        Assert.True(gitHubUrl1 == gitHubUrl2);
        Assert.False(gitHubUrl1 != gitHubUrl2);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var url1 = "https://github.com/AskmethatFR/agent-dashboard/issues/6";
        var url2 = "https://github.com/AskmethatFR/agent-dashboard/issues/7";
        var gitHubUrl1 = new GitHubUrl(url1);
        var gitHubUrl2 = new GitHubUrl(url2);

        // Act & Assert
        Assert.NotEqual(gitHubUrl1, gitHubUrl2);
        Assert.False(gitHubUrl1 == gitHubUrl2);
        Assert.True(gitHubUrl1 != gitHubUrl2);
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        // Arrange
        var url = "https://github.com/AskmethatFR/agent-dashboard/issues/6";
        var gitHubUrl = new GitHubUrl(url);

        // Act & Assert
        Assert.NotNull(gitHubUrl);
        Assert.False(gitHubUrl.Equals(null));
        Assert.False(null == gitHubUrl);
        Assert.False(gitHubUrl == null);
    }

    [Fact]
    public void ToString_ReturnsUrlString()
    {
        // Arrange
        var url = "https://github.com/AskmethatFR/agent-dashboard/issues/6";
        var gitHubUrl = new GitHubUrl(url);

        // Act
        var result = gitHubUrl.ToString();

        // Assert
        Assert.Equal(url, result);
    }

    [Fact]
    public void ImplicitConversion_FromString_ToGitHubUrl_Works()
    {
        // Arrange
        var url = "https://github.com/AskmethatFR/agent-dashboard/issues/6";

        // Act
        GitHubUrl gitHubUrl = url;

        // Assert
        Assert.Equal(url, gitHubUrl.Value);
    }

    [Fact]
    public void ImplicitConversion_FromGitHubUrl_ToString_Works()
    {
        // Arrange
        var url = "https://github.com/AskmethatFR/agent-dashboard/issues/6";
        var gitHubUrl = new GitHubUrl(url);

        // Act
        string result = gitHubUrl;

        // Assert
        Assert.Equal(url, result);
    }
}
