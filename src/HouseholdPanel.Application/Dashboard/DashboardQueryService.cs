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
    IOptions<WeatherOptions> weatherOptions,
    IOptions<TransportOptions> transportOptions) : IDashboardQueryService
{
    public async Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken)
    {
        var weatherLocations = new List<WeatherDto>();
        foreach (var location in weatherOptions.Value.Locations)
        {
            var forecast = await weatherService.GetCurrentAsync(location, cancellationToken);
            weatherLocations.Add(new WeatherDto(
                location.Name,
                forecast.Temperature,
                forecast.MinimumTemperature,
                forecast.MaximumTemperature,
                forecast.Symbol,
                forecast.PrecipitationProbability,
                forecast.WindSpeed));
        }

        var indoor = await indoorSensorService.GetCurrentAsync(cancellationToken);
        var departures = await transportService.GetDeparturesAsync(cancellationToken);
        var calendarEvents = await calendarService.GetUpcomingEventsAsync(cancellationToken);
        var scheduleItems = await scheduleService.GetUpcomingItemsAsync(cancellationToken);

        return new DashboardDto(
            Timestamp: DateTimeOffset.Now,
            Weather: weatherLocations[0],
            WeatherLocations: weatherLocations,
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
