using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AgentDashboard.Web.Tests.Hosting;

// Test list for host fail-fast on missing GitHub ingestion env vars:
//   1. GITHUB_TOKEN missing -> boot throws InvalidOperationException mentioning GITHUB_TOKEN
public sealed class HostBootTests
{
    [Fact]
    public void FailFast_WhenGitHubTokenIsMissing()
    {
        using var factory = BuildFactory(token: null);

        var act = () => factory.CreateClient();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*GITHUB_TOKEN*");
    }

    private static WebApplicationFactory<Program> BuildFactory(string? token)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration.Sources.Clear();
                    var pairs = new Dictionary<string, string?>();
                    if (token is not null) pairs["GITHUB_TOKEN"] = token;
                    configuration.AddInMemoryCollection(pairs);
                });
            });
    }
}
