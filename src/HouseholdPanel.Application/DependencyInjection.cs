using HouseholdPanel.Application.Dashboard;
using HouseholdPanel.Application.Timers;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdPanel.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDashboardQueryService, DashboardQueryService>();
        services.AddScoped<TimerService>();

        return services;
    }
}
