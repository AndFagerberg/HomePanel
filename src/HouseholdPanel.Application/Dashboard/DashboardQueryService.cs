using HouseholdPanel.Application.Abstractions;
using HouseholdPanel.Application.Configuration;
using Microsoft.Extensions.Options;

namespace HouseholdPanel.Application.Dashboard;

public sealed class DashboardQueryService(
    IWeatherService weatherService,
    IIndoorSensorService indoorSensorService,
    ITransportService transportService,
    ICalendarService calendarService,
    IScheduleService scheduleService,
    IOptions<TransportOptions> transportOptions) : IDashboardQueryService
{
    public async Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken)
    {
        var weather = await weatherService.GetCurrentAsync(cancellationToken);
        var indoor = await indoorSensorService.GetCurrentAsync(cancellationToken);
        var departures = await transportService.GetDeparturesAsync(cancellationToken);
        var calendarEvents = await calendarService.GetUpcomingEventsAsync(cancellationToken);
        var scheduleItems = await scheduleService.GetUpcomingItemsAsync(cancellationToken);

        return new DashboardDto(
            Timestamp: DateTimeOffset.Now,
            Weather: new WeatherDto(
                weather.Temperature,
                weather.MinimumTemperature,
                weather.MaximumTemperature,
                weather.Symbol,
                weather.PrecipitationProbability,
                weather.WindSpeed),
            Indoor: new IndoorDto(indoor.Temperature, indoor.Humidity),
            Transport: new TransportDto(
                transportOptions.Value.StopName,
                departures
                    .Select(d => new DepartureDto(
                        d.DepartureTime.ToString("HH:mm"),
                        d.Destination,
                        d.Line,
                        d.MinutesUntilDeparture))
                    .ToList()),
            Calendar: calendarEvents
                .Select(e => new CalendarEventDto(e.Start.ToString("HH:mm"), e.Title))
                .ToList(),
            Schedule: scheduleItems
                .Select(s => new ScheduleItemDto(s.Start.ToString("HH:mm"), s.Title))
                .ToList());
    }
}
