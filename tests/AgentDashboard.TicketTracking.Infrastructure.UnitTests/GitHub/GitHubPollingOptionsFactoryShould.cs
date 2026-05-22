using AgentDashboard.TicketTracking.Infrastructure.GitHub;
using AgentDashboard.TicketTracking.Infrastructure.UnitTests.GitHub.Fakes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgentDashboard.TicketTracking.Infrastructure.UnitTests.GitHub;

// Test list for GitHubPollingOptionsFactory.FromConfiguration:
//   1. GITHUB_TOKEN missing -> InvalidOperationException mentioning GITHUB_TOKEN
//   2. GITHUB_TOKEN empty/whitespace -> InvalidOperationException mentioning GITHUB_TOKEN
//   3. GITHUB_REPO missing -> InvalidOperationException mentioning GITHUB_REPO
//   4. GITHUB_REPO malformed (e.g. "no-slash", "/repo", "owner/", "a b/c") -> throws
//   5. POLL_INTERVAL_SECONDS below 300 -> clamped to 300 + WARNING log
//   6. POLL_INTERVAL_SECONDS == 300 -> stays 300, no warning
//   7. POLL_INTERVAL_SECONDS == 600 -> stays 600, no warning
//   8. POLL_INTERVAL_SECONDS missing -> default 600, no warning
//   9. Valid configuration -> options carries Token, RepositoryOwner, RepositoryName, PollInterval
public sealed class GitHubPollingOptionsFactoryShould
{
    private const string ValidToken = "ghp_examplePAT12345";
    private const string ValidRepo = "owner/repo";

    [Fact]
    public void Throw_WhenGitHubTokenIsMissing()
    {
        var configuration = BuildConfiguration(token: null, repo: ValidRepo);
        var logger = new RecordingLogger();

        var act = () => GitHubPollingOptionsFactory.FromConfiguration(configuration, logger);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*GITHUB_TOKEN*");
    }

    [Fact]
    public void Throw_WhenGitHubTokenIsWhitespace()
    {
        var configuration = BuildConfiguration(token: "   ", repo: ValidRepo);
        var logger = new RecordingLogger();

        var act = () => GitHubPollingOptionsFactory.FromConfiguration(configuration, logger);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*GITHUB_TOKEN*");
    }

    [Fact]
    public void Throw_WhenGitHubRepoIsMissing()
    {
        var configuration = BuildConfiguration(token: ValidToken, repo: null);
        var logger = new RecordingLogger();

        var act = () => GitHubPollingOptionsFactory.FromConfiguration(configuration, logger);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*GITHUB_REPO*");
    }

    [Theory]
    [InlineData("no-slash")]
    [InlineData("/repo")]
    [InlineData("owner/")]
    [InlineData("owner/repo/extra")]
    [InlineData("a b/c")]
    [InlineData(" owner/repo ")]
    public void Throw_WhenGitHubRepoIsMalformed(string repo)
    {
        var configuration = BuildConfiguration(token: ValidToken, repo: repo);
        var logger = new RecordingLogger();

        var act = () => GitHubPollingOptionsFactory.FromConfiguration(configuration, logger);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*GITHUB_REPO*");
    }

    [Fact]
    public void ClampInterval_WhenBelowMinimum_AndEmitWarning()
    {
        var configuration = BuildConfiguration(token: ValidToken, repo: ValidRepo, pollIntervalSeconds: "100");
        var logger = new RecordingLogger();

        var options = GitHubPollingOptionsFactory.FromConfiguration(configuration, logger);

        options.PollInterval.Should().Be(TimeSpan.FromSeconds(300));
        logger.Entries.Should().ContainSingle(e => e.Level == LogLevel.Warning);
    }

    [Fact]
    public void KeepInterval_WhenAtMinimum()
    {
        var configuration = BuildConfiguration(token: ValidToken, repo: ValidRepo, pollIntervalSeconds: "300");
        var logger = new RecordingLogger();

        var options = GitHubPollingOptionsFactory.FromConfiguration(configuration, logger);

        options.PollInterval.Should().Be(TimeSpan.FromSeconds(300));
        logger.Entries.Should().NotContain(e => e.Level == LogLevel.Warning);
    }

    [Fact]
    public void KeepInterval_WhenAtDefault()
    {
        var configuration = BuildConfiguration(token: ValidToken, repo: ValidRepo, pollIntervalSeconds: "600");
        var logger = new RecordingLogger();

        var options = GitHubPollingOptionsFactory.FromConfiguration(configuration, logger);

        options.PollInterval.Should().Be(TimeSpan.FromSeconds(600));
        logger.Entries.Should().NotContain(e => e.Level == LogLevel.Warning);
    }

    [Fact]
    public void UseDefaultInterval_WhenIntervalIsMissing()
    {
        var configuration = BuildConfiguration(token: ValidToken, repo: ValidRepo, pollIntervalSeconds: null);
        var logger = new RecordingLogger();

        var options = GitHubPollingOptionsFactory.FromConfiguration(configuration, logger);

        options.PollInterval.Should().Be(TimeSpan.FromSeconds(600));
        logger.Entries.Should().NotContain(e => e.Level == LogLevel.Warning);
    }

    [Fact]
    public void ParseOwnerAndRepository_FromGitHubRepo()
    {
        var configuration = BuildConfiguration(token: ValidToken, repo: "askmethatfr/agent-dashboard");
        var logger = new RecordingLogger();

        var options = GitHubPollingOptionsFactory.FromConfiguration(configuration, logger);

        options.Token.Should().Be(ValidToken);
        options.RepositoryOwner.Should().Be("askmethatfr");
        options.RepositoryName.Should().Be("agent-dashboard");
    }

    private static IConfiguration BuildConfiguration(
        string? token,
        string? repo,
        string? pollIntervalSeconds = null)
    {
        var pairs = new Dictionary<string, string?>();
        if (token is not null) pairs["GITHUB_TOKEN"] = token;
        if (repo is not null) pairs["GITHUB_REPO"] = repo;
        if (pollIntervalSeconds is not null) pairs["POLL_INTERVAL_SECONDS"] = pollIntervalSeconds;

        return new ConfigurationBuilder()
            .AddInMemoryCollection(pairs)
            .Build();
    }
}
