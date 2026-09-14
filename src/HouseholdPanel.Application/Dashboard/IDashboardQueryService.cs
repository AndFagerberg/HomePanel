namespace HouseholdPanel.Application.Dashboard;

public interface IDashboardQueryService
{
    Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken);

    Task<WeatherSectionDto> GetWeatherAsync(CancellationToken cancellationToken);

    Task<TransportDto> GetTransportAsync(CancellationToken cancellationToken);

    Task<CalendarSectionDto> GetCalendarAsync(CancellationToken cancellationToken);

    Task<NewsSectionDto> GetNewsAsync(CancellationToken cancellationToken);

    Task<AirPatrolDto?> GetCabinAsync(CancellationToken cancellationToken);

    Task<IndoorDto> GetIndoorAsync(CancellationToken cancellationToken);
}
