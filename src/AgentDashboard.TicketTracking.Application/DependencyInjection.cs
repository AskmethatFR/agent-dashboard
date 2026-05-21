using Cortex.Mediator.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace AgentDashboard.TicketTracking.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTicketTrackingApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddCortexMediator(
            new[] { typeof(DependencyInjection) },
            _ => { });
        return services;
    }
}
