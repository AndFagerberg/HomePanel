using HouseholdPanel.Application.Abstractions;
using HouseholdPanel.Application.Configuration;
using HouseholdPanel.Application.Music;
using HouseholdPanel.Application.Timers;
using Microsoft.Extensions.Options;

namespace HouseholdPanel.Application.Dashboard;

public sealed class DashboardQueryService(
    IWeatherService weatherService,
    IIndoorSensorService indoorSensorService,
    ITransportService transportService,
    ICalendarService calendarService,
    IScheduleService scheduleService,
    INewsService newsService,
    IAirPatrolService airPatrolService,
    IAirPatrolHistoryRepository airPatrolHistoryRepository,
    IMusicService musicService,
    TimerService timerService,
    IOptions<WeatherOptions> weatherOptions,
    IOptions<TransportOptions> transportOptions) : IDashboardQueryService
{
    public async Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken)
    {
        var weatherLocations = new List<WeatherDto>();
        foreach (var location in weatherOptions.Value.Locations)
        {
            var forecast = await weatherService.GetCurrentAsync(location, cancellationToken);
            var weatherDto = new WeatherDto(
                location.Name,
                forecast.Temperature,
                forecast.MinimumTemperature,
                forecast.MaximumTemperature,
                forecast.Symbol,
                forecast.PrecipitationProbability,
                forecast.WindSpeed)
            {
                TomorrowMinimumTemperature = forecast.TomorrowMinimumTemperature,
                TomorrowMaximumTemperature = forecast.TomorrowMaximumTemperature,
                TomorrowSymbol = forecast.TomorrowSymbol,
            };
            weatherLocations.Add(weatherDto);
        }

        var indoor = await indoorSensorService.GetCurrentAsync(cancellationToken);
        var departures = await transportService.GetDeparturesAsync(cancellationToken);
        var calendarEvents = await calendarService.GetUpcomingEventsAsync(cancellationToken);
        var scheduleItems = await scheduleService.GetUpcomingItemsAsync(cancellationToken);
        var nationalNews = await newsService.GetNationalAsync(cancellationToken);
        var localNews = await newsService.GetLocalAsync(cancellationToken);
        var airPatrol = await airPatrolService.GetStatusAsync(cancellationToken);
        var airPatrolHistory = await airPatrolHistoryRepository.GetSinceAsync(
            DateTimeOffset.UtcNow.AddDays(-7),
            cancellationToken);
        airPatrol ??= airPatrolHistory.LastOrDefault();
        var timers = await timerService.GetActiveAsync(cancellationToken);
        var music = await musicService.GetCurrentPlaybackAsync(cancellationToken);

        return new DashboardDto(
            Timestamp: DateTimeOffset.Now,
            Weather: weatherLocations[0],
            WeatherLocations: weatherLocations,
            Indoor: new IndoorDto(indoor.Temperature, indoor.Humidity),
            AirPatrol: airPatrol is null
                ? null
                : new AirPatrolDto(
                    airPatrol.Name,
                    airPatrol.Temperature,
                    airPatrol.Humidity,
                    airPatrol.Power,
                    airPatrol.Mode,
                    airPatrol.TargetTemperature,
                    airPatrol.FanSpeed,
                    airPatrol.Swing,
                    airPatrol.UpdatedAt,
                    airPatrolHistory
                        .Select(status => new AirPatrolHistoryPointDto(
                            status.UpdatedAt,
                            status.Temperature,
                            status.Humidity,
                            status.TargetTemperature))
                        .ToList()),
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
                .ToList(),
            NationalNews: nationalNews.Select(MapNewsArticle).ToList(),
            LocalNews: localNews.Select(MapNewsArticle).ToList(),
            Timers: timers)
        {
            Music = music is null
                ? null
                : new MusicPlaybackDto(music.Title, music.Artist, music.Album, music.ImageUrl, music.IsPlaying),
        };
    }

    private static NewsArticleDto MapNewsArticle(HouseholdPanel.Domain.News.NewsArticle article) =>
        new(article.Title, article.Summary, article.Source, article.PublishedAt, article.Url);
}
