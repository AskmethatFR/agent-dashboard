using AgentDashboard.TicketTracking.Application.Ports;
using AgentDashboard.TicketTracking.Infrastructure.Boards;
using AgentDashboard.TicketTracking.Infrastructure.GitHub;
using AgentDashboard.TicketTracking.Infrastructure.Tickets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace AgentDashboard.TicketTracking.Infrastructure;

public static class DependencyInjection
{
    private const string DataPathEnvVar = "DATA_PATH";
    private const string DefaultDataPath = "/data";
    private const string DatabaseFileName = "tickets.db";

    public static IServiceCollection AddTicketTrackingInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<BoardSnapshotCache>();
        services.AddSingleton<IBoardReader, CachedBoardReader>();
        return services;
    }

    public static IServiceCollection AddTicketTrackingGitHubIngestion(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Options are resolved lazily from the final configuration at host start.
        services.AddSingleton(sp =>
        {
            var finalConfiguration = sp.GetRequiredService<IConfiguration>();
            var logger = sp.GetRequiredService<ILogger<GitHubPollingOptions>>();
            return GitHubPollingOptionsFactory.FromConfiguration(finalConfiguration, logger);
        });

        services.AddSingleton<IGitHubIssuesClient>(sp =>
            new OctokitGitHubIssuesClient(
                sp.GetRequiredService<GitHubPollingOptions>(),
                sp.GetRequiredService<ILogger<OctokitGitHubIssuesClient>>()));
        services.AddSingleton<BoardRefreshTrigger>();
        services.AddSingleton<IBoardRefreshTrigger>(sp => sp.GetRequiredService<BoardRefreshTrigger>());
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<IBoardSnapshotUpdater, BoardSnapshotUpdater>();

        // Register SQLite ticket write repository
        var dataPath = configuration[DataPathEnvVar] ?? DefaultDataPath;
        var connectionString = $"Data Source={System.IO.Path.Combine(dataPath, DatabaseFileName)}";
        services.AddSingleton<ITicketWriteRepository>(_ =>
            new SqliteTicketWriteRepository(connectionString));

        // Override IBoardReader with GitHubBoardReader when GitHub ingestion is enabled
        services.AddSingleton<IBoardReader, GitHubBoardReader>(sp =>
            new GitHubBoardReader(
                sp.GetRequiredService<BoardSnapshotCache>(),
                sp.GetRequiredService<IGitHubIssuesClient>(),
                sp.GetRequiredService<IBoardSnapshotUpdater>(),
                sp.GetRequiredService<GitHubPollingOptions>().PollInterval,
                sp.GetRequiredService<TimeProvider>(),
                sp.GetRequiredService<ILogger<GitHubBoardReader>>()));
        services.AddHostedService<GitHubIssuesPoller>();

        return services;
    }
}
