using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace AgentDashboard.Web.Tests.Hosting;

// Test list for host boot behavior:
//   1. GITHUB_TOKEN missing -> boot throws InvalidOperationException mentioning GITHUB_TOKEN
//   2. DATA_PATH unset -> boot succeeds, data/ created under ContentRootPath (not /data)
//   3. DATA_PATH already set -> boot succeeds, explicit path preserved (if-block NOT entered)
public sealed class HostBootTests : IDisposable
{
    private string? _createdDataDir;
    private string? _explicitTempDataDir;

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
    }

    [Fact]
    public void Boot_WhenDataPathSet_PreservesExplicitPath()
    {
        var explicitPath = Path.Combine(Path.GetTempPath(), $"agent-dashboard-test-{Guid.NewGuid():N}");
        _explicitTempDataDir = explicitPath;

        // Set DATA_PATH as an environment variable so it is visible to Program.cs at service-
        // registration time (matching the Docker override path). ConfigureAppConfiguration with
        // Sources.Clear() injects configuration too late for the connection-string capture in
        // AddTicketTrackingGitHubIngestion; environment variables are read first.
        Environment.SetEnvironmentVariable("DATA_PATH", explicitPath);
        try
        {
            using var factory = BuildFactory(token: "ghp_1234567890", dataPath: null);

            var act = () => factory.CreateClient();

            act.Should().NotThrow();

            Directory.Exists(explicitPath).Should().BeTrue(
                because: "the SQLite repository must create the data directory under the explicit DATA_PATH");

            var dbFile = Path.Combine(explicitPath, "tickets.db");
            File.Exists(dbFile).Should().BeTrue(
                because: "the SQLite database file must be created under the explicit DATA_PATH, not under ContentRootPath/data");
        }
        finally
        {
            Environment.SetEnvironmentVariable("DATA_PATH", null);
        }
    }

    public void Dispose()
    {
        if (_createdDataDir is not null && Directory.Exists(_createdDataDir))
        {
            Directory.Delete(_createdDataDir, recursive: true);
        }

        if (_explicitTempDataDir is not null && Directory.Exists(_explicitTempDataDir))
        {
            Directory.Delete(_explicitTempDataDir, recursive: true);
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
