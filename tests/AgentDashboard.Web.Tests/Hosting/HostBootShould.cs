using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AgentDashboard.Web.Tests.Hosting;

// Test list for host fail-fast on missing GitHub ingestion env vars:
//   1. GITHUB_TOKEN missing -> boot throws InvalidOperationException mentioning GITHUB_TOKEN
//   2. GITHUB_REPO missing  -> boot throws InvalidOperationException mentioning GITHUB_REPO
public sealed class HostBootShould
{
    [Fact]
    public void FailFast_WhenGitHubTokenIsMissing()
    {
        using var factory = BuildFactory(token: null, repo: "owner/repo");

        var act = () => factory.CreateClient();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*GITHUB_TOKEN*");
    }

    [Fact]
    public void FailFast_WhenGitHubRepoIsMissing()
    {
        using var factory = BuildFactory(token: "ghp_examplePAT", repo: null);

        var act = () => factory.CreateClient();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*GITHUB_REPO*");
    }

    private static WebApplicationFactory<Program> BuildFactory(string? token, string? repo)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration.Sources.Clear();
                    var pairs = new Dictionary<string, string?>();
                    if (token is not null) pairs["GITHUB_TOKEN"] = token;
                    if (repo is not null) pairs["GITHUB_REPO"] = repo;
                    configuration.AddInMemoryCollection(pairs);
                });
            });
    }
}
