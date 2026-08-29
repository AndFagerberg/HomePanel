using HouseholdPanel.Application.Dashboard;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdPanel.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDashboardQueryService, DashboardQueryService>();

        return services;
    }
}
