using AgentDashboard.TicketTracking.Application.Ports;
using AgentDashboard.TicketTracking.Infrastructure.Boards;
using AgentDashboard.TicketTracking.Infrastructure.GitHub;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace AgentDashboard.TicketTracking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTicketTrackingInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<BoardSnapshotCache>();
        services.AddSingleton<IBoardReader, StubBoardReader>();
        return services;
    }

    public static IServiceCollection AddTicketTrackingGitHubIngestion(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Options are resolved lazily from the final configuration at host start.
        // This lets WebApplicationFactory inject configuration overrides via
        // ConfigureAppConfiguration before validation runs.
        services.AddSingleton(sp =>
        {
            var finalConfiguration = sp.GetRequiredService<IConfiguration>();
            var logger = sp.GetRequiredService<ILogger<GitHubPollingOptions>>();
            return GitHubPollingOptionsFactory.FromConfiguration(finalConfiguration, logger);
        });

        services.AddSingleton<IGitHubIssuesClient, OctokitGitHubIssuesClient>();
        services.AddSingleton<BoardRefreshTrigger>();
        services.AddSingleton<IBoardRefreshTrigger>(sp => sp.GetRequiredService<BoardRefreshTrigger>());
        services.TryAddSingleton(TimeProvider.System);
        services.AddHostedService<GitHubIssuesPoller>();
        
        // Override IBoardReader with GitHubBoardReader when GitHub ingestion is enabled
        services.AddSingleton<IBoardReader, GitHubBoardReader>();

        return services;
    }
}
