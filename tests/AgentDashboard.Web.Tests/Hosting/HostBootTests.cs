using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace AgentDashboard.Web.Tests.Hosting;

// Test list for host boot behavior:
//   1. GITHUB_TOKEN missing -> boot throws InvalidOperationException mentioning GITHUB_TOKEN
//   2. DATA_PATH unset -> boot succeeds, data/ created under ContentRootPath (not /data)
public sealed class HostBootTests : IDisposable
{
    private string? _createdDataDir;

    [Fact]
    public void FailFast_WhenGitHubTokenIsMissing()
    {
        using var factory = BuildFactory(token: null, dataPath: null);

        var act = () => factory.CreateClient();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*GITHUB_TOKEN*");
    }

    [Fact]
    public void Boot_WhenDataPathUnset_ResolvesWritableDataDirUnderContentRoot()
    {
        using var factory = BuildFactory(token: "ghp_1234567890", dataPath: null);

        var act = () => factory.CreateClient();

        act.Should().NotThrow();

        var env = factory.Services.GetService(typeof(IHostEnvironment)) as IHostEnvironment;
        env.Should().NotBeNull();
        var expectedDataDir = Path.Combine(env!.ContentRootPath, "data");
        _createdDataDir = expectedDataDir;

        Directory.Exists(expectedDataDir).Should().BeTrue(
            because: "the composition root should have created the data/ folder under ContentRootPath");

        Directory.Exists("/data").Should().BeFalse(
            because: "the fallback /data path must not be created when DATA_PATH is unset on a dev machine");
    }

    public void Dispose()
    {
        if (_createdDataDir is not null && Directory.Exists(_createdDataDir))
        {
            Directory.Delete(_createdDataDir, recursive: true);
        }
    }

    private static WebApplicationFactory<Program> BuildFactory(string? token, string? dataPath)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration.Sources.Clear();
                    var pairs = new Dictionary<string, string?>();
                    if (token is not null) pairs["GITHUB_TOKEN"] = token;
                    if (dataPath is not null) pairs["DATA_PATH"] = dataPath;
                    configuration.AddInMemoryCollection(pairs);
                });
            });
    }
}
