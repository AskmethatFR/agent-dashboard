using AgentDashboard.TicketTracking.Application.Ports;
using AgentDashboard.TicketTracking.Infrastructure.Boards;
using Microsoft.Extensions.DependencyInjection;

namespace AgentDashboard.TicketTracking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTicketTrackingInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IBoardReader, StubBoardReader>();
        return services;
    }
}
