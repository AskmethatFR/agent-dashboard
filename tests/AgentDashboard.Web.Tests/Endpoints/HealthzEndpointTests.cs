using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AgentDashboard.Web.Tests.Endpoints;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.Sources.Clear();
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GITHUB_TOKEN"] = "ghp_1234567890",
                ["GITHUB_REPO"] = "AskmethatFR/agent-dashboard"
            });
        });
    }
}

/// <summary>
/// Tests for HealthzEndpoint.
/// Verifies the health check endpoint returns 200 OK with no body.
/// </summary>
public class HealthzEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public HealthzEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Healthz_Returns200Ok_WithNoBody()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/healthz");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentLength.Should().Be(0);
    }

    [Fact]
    public async Task Healthz_DoesNotLeakData_InResponse()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/healthz");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // No body
        var body = await response.Content.ReadAsStringAsync();
        body.Should().BeEmpty();

        // No sensitive headers
        response.Headers.Should().NotContain(h => h.Key.Equals("Server", StringComparison.OrdinalIgnoreCase));
    }
}
