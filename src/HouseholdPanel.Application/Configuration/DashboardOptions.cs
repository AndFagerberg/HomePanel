namespace HouseholdPanel.Application.Configuration;

// Background worker cache-refresh intervals, exposed so they can be tuned per environment.
public sealed class DashboardOptions
{
    public const string SectionName = "Dashboard";

    public TimeSpan WeatherUpdateInterval { get; init; } = TimeSpan.FromMinutes(15);
    public TimeSpan TransportUpdateInterval { get; init; } = TimeSpan.FromMinutes(1);
    public TimeSpan CalendarUpdateInterval { get; init; } = TimeSpan.FromMinutes(5);
    public TimeSpan ScheduleUpdateInterval { get; init; } = TimeSpan.FromHours(24);
}
