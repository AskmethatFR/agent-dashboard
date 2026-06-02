#pragma warning disable IDE0005 // Using directive is unnecessary.
using Microsoft.AspNetCore.Builder;
#pragma warning restore IDE0005 // Using directive is unnecessary.

namespace AgentDashboard.Web.Endpoints;

/// <summary>
/// Lightweight health check endpoint for Docker HEALTHCHECK.
/// Returns 200 OK when the application is running.
/// Does not expose any sensitive information.
/// </summary>
public static class HealthzEndpoint
{
    /// <summary>
    /// Maps the /healthz endpoint to the application.
    /// </summary>
    /// <param name="app">The web application builder.</param>
    public static IEndpointRouteBuilder MapHealthz(this IEndpointRouteBuilder app)
    {
        app.MapGet("/healthz", () => Results.Ok());
        return app;
    }
}
