using AgentDashboard.TicketTracking.Infrastructure.GitHub;
using AgentDashboard.TicketTracking.Infrastructure.UnitTests.GitHub.Fakes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgentDashboard.TicketTracking.Infrastructure.UnitTests.GitHub;

// Test list for GitHubPollingOptionsFactory.FromConfiguration:
//   1. GITHUB_TOKEN missing -> InvalidOperationException mentioning GITHUB_TOKEN
//   2. GITHUB_TOKEN empty/whitespace -> InvalidOperationException mentioning GITHUB_TOKEN
//   3. POLL_INTERVAL_SECONDS below 300 -> clamped to 300 + WARNING log
//   4. POLL_INTERVAL_SECONDS == 300 -> stays 300, no warning
//   5. POLL_INTERVAL_SECONDS == 600 -> stays 600, no warning
//   6. POLL_INTERVAL_SECONDS missing -> default 600, no warning
//   7. Options carries the hardcoded dogfooding repo identity
//      (AskmethatFR / agent-dashboard) regardless of configuration input
//      — see ADR-005.
//   8. An arbitrary GITHUB_REPO entry in configuration is silently ignored
//      and the dogfooding constants are still exposed — pins the v1.0
//      decision that GITHUB_REPO has no effect (ADR-005, anti-regression).
public sealed class GitHubPollingOptionsFactoryShould
{
    private const string ValidToken = "ghp_examplePAT12345";

    [Fact]
    public void Throw_WhenGitHubTokenIsMissing()
    {
        var configuration = BuildConfiguration(token: null);
        var logger = new RecordingLogger();

        var act = () => GitHubPollingOptionsFactory.FromConfiguration(configuration, logger);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*GITHUB_TOKEN*");
    }

    [Fact]
    public void Throw_WhenGitHubTokenIsWhitespace()
    {
        var configuration = BuildConfiguration(token: "   ");
        var logger = new RecordingLogger();

        var act = () => GitHubPollingOptionsFactory.FromConfiguration(configuration, logger);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*GITHUB_TOKEN*");
    }

    [Fact]
    public void ClampInterval_WhenBelowMinimum_AndEmitWarning()
    {
        var configuration = BuildConfiguration(token: ValidToken, pollIntervalSeconds: "100");
        var logger = new RecordingLogger();

        var options = GitHubPollingOptionsFactory.FromConfiguration(configuration, logger);

        options.PollInterval.Should().Be(TimeSpan.FromSeconds(300));
        logger.Entries.Should().ContainSingle(e => e.Level == LogLevel.Warning);
    }

    [Fact]
    public void KeepInterval_WhenAtMinimum()
    {
        var configuration = BuildConfiguration(token: ValidToken, pollIntervalSeconds: "300");
        var logger = new RecordingLogger();

        var options = GitHubPollingOptionsFactory.FromConfiguration(configuration, logger);

        options.PollInterval.Should().Be(TimeSpan.FromSeconds(300));
        logger.Entries.Should().NotContain(e => e.Level == LogLevel.Warning);
    }

    [Fact]
    public void KeepInterval_WhenAtDefault()
    {
        var configuration = BuildConfiguration(token: ValidToken, pollIntervalSeconds: "600");
        var logger = new RecordingLogger();

        var options = GitHubPollingOptionsFactory.FromConfiguration(configuration, logger);

        options.PollInterval.Should().Be(TimeSpan.FromSeconds(600));
        logger.Entries.Should().NotContain(e => e.Level == LogLevel.Warning);
    }

    [Fact]
    public void UseDefaultInterval_WhenIntervalIsMissing()
    {
        var configuration = BuildConfiguration(token: ValidToken, pollIntervalSeconds: null);
        var logger = new RecordingLogger();

        var options = GitHubPollingOptionsFactory.FromConfiguration(configuration, logger);

        options.PollInterval.Should().Be(TimeSpan.FromSeconds(600));
        logger.Entries.Should().NotContain(e => e.Level == LogLevel.Warning);
    }

    [Fact]
    public void ExposeDogfoodingRepositoryConstants()
    {
        var configuration = BuildConfiguration(token: ValidToken);
        var logger = new RecordingLogger();

        var options = GitHubPollingOptionsFactory.FromConfiguration(configuration, logger);

        options.Token.Should().Be(ValidToken);
        options.RepositoryOwner.Should().Be("AskmethatFR");
        options.RepositoryName.Should().Be("agent-dashboard");
    }

    [Fact]
    public void ExposeDogfoodingRepositoryConstants_WhenConfigContainsArbitraryGitHubRepoKey()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GITHUB_TOKEN"] = ValidToken,
                ["GITHUB_REPO"] = "evil/repo",
            })
            .Build();
        var logger = new RecordingLogger();

        var options = GitHubPollingOptionsFactory.FromConfiguration(configuration, logger);

        options.RepositoryOwner.Should().Be("AskmethatFR");
        options.RepositoryName.Should().Be("agent-dashboard");
    }

    private static IConfiguration BuildConfiguration(
        string? token,
        string? pollIntervalSeconds = null)
    {
        var pairs = new Dictionary<string, string?>();
        if (token is not null) pairs["GITHUB_TOKEN"] = token;
        if (pollIntervalSeconds is not null) pairs["POLL_INTERVAL_SECONDS"] = pollIntervalSeconds;

        return new ConfigurationBuilder()
            .AddInMemoryCollection(pairs)
            .Build();
    }
}
