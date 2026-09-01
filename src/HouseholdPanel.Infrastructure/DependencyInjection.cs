using HouseholdPanel.Application.Abstractions;
using HouseholdPanel.Application.Configuration;
using HouseholdPanel.Infrastructure.Calendar;
using HouseholdPanel.Infrastructure.Indoor;
using HouseholdPanel.Infrastructure.Schedule;
using HouseholdPanel.Infrastructure.Transport;
using HouseholdPanel.Infrastructure.Weather;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdPanel.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<WeatherOptions>(configuration.GetSection(WeatherOptions.SectionName));
        services.Configure<TransportOptions>(configuration.GetSection(TransportOptions.SectionName));
        services.Configure<CalendarOptions>(configuration.GetSection(CalendarOptions.SectionName));
        services.Configure<DashboardOptions>(configuration.GetSection(DashboardOptions.SectionName));

        services.AddHttpClient<IWeatherService, SmhiWeatherService>(client =>
        {
            client.BaseAddress = new Uri("https://opendata-download-metfcst.smhi.se/");
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        services.AddSingleton<IIndoorSensorService, TestDataIndoorSensorService>();
        services.AddSingleton<ITransportService, TestDataTransportService>();
        services.AddSingleton<ICalendarService, TestDataCalendarService>();
        services.AddSingleton<IScheduleService, TestDataScheduleService>();

        return services;
    }
}
