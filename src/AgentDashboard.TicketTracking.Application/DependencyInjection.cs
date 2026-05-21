using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AgentDashboard.TicketTracking.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTicketTrackingApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }
}
