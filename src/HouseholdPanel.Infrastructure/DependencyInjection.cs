using HouseholdPanel.Application.Abstractions;
using HouseholdPanel.Application.Configuration;
using HouseholdPanel.Infrastructure.Calendar;
using HouseholdPanel.Infrastructure.Indoor;
using HouseholdPanel.Infrastructure.Schedule;
using HouseholdPanel.Infrastructure.Transport;
using HouseholdPanel.Infrastructure.Weather;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        services.AddHttpClient<ITransportService, TrafiklabTransportService>(client =>
        {
            client.BaseAddress = new Uri("https://realtime-api.trafiklab.se/");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddHttpClient<GoogleCalendarService>(client =>
        {
            client.BaseAddress = new Uri("https://www.googleapis.com/");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddSingleton<ICalendarService>(serviceProvider =>
        {
            var calendarOptions = serviceProvider.GetRequiredService<IOptions<CalendarOptions>>().Value;
            var providerName = calendarOptions.Provider ?? string.Empty;

            if (string.Equals(providerName, "Google", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(calendarOptions.CalendarId)
                && !string.IsNullOrWhiteSpace(calendarOptions.GoogleClientId)
                && !string.IsNullOrWhiteSpace(calendarOptions.GoogleClientSecret)
                && !string.IsNullOrWhiteSpace(calendarOptions.GoogleRefreshToken))
            {
                return serviceProvider.GetRequiredService<GoogleCalendarService>();
            }

            return new TestDataCalendarService();
        });

        services.AddSingleton<IScheduleService, TestDataScheduleService>();

        return services;
    }
}
