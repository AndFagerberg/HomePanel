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
        var weatherLocations = await GetWeatherLocationsAsync(cancellationToken);
        var indoor = await GetIndoorAsync(cancellationToken);
        var transport = await GetTransportAsync(cancellationToken);
        var calendarSection = await GetCalendarAsync(cancellationToken);
        var newsSection = await GetNewsAsync(cancellationToken);
        var (airPatrol, airPatrolHistory) = await GetCabinWithHistoryAsync(cancellationToken);
        var timers = await timerService.GetActiveAsync(cancellationToken);
        var music = await musicService.GetCurrentPlaybackAsync(cancellationToken);

        return new DashboardDto(
            Timestamp: DateTimeOffset.Now,
            Weather: weatherLocations[0],
            WeatherLocations: weatherLocations,
            Indoor: indoor,
            AirPatrol: airPatrol,
            Transport: transport,
            Calendar: calendarSection.Calendar,
            Schedule: calendarSection.Schedule,
            NationalNews: newsSection.NationalNews,
            LocalNews: newsSection.LocalNews,
            Timers: timers)
        {
            Music = music is null
                ? null
                : new MusicPlaybackDto(music.Title, music.Artist, music.Album, music.ImageUrl, music.IsPlaying),
        };
    }

    public async Task<WeatherSectionDto> GetWeatherAsync(CancellationToken cancellationToken)
    {
        var locations = await GetWeatherLocationsAsync(cancellationToken);
        return new WeatherSectionDto(locations[0], locations);
    }

    public async Task<TransportDto> GetTransportAsync(CancellationToken cancellationToken)
    {
        var departures = await transportService.GetDeparturesAsync(cancellationToken);
        return new TransportDto(
            transportOptions.Value.StopName,
            departures
                .Select(d => new DepartureDto(
                    d.DepartureTime.ToString("HH:mm"),
                    d.Destination,
                    d.Line,
                    d.MinutesUntilDeparture))
                .ToList());
    }

    public async Task<CalendarSectionDto> GetCalendarAsync(CancellationToken cancellationToken)
    {
        var calendarEvents = await calendarService.GetUpcomingEventsAsync(cancellationToken);
        var scheduleItems = await scheduleService.GetUpcomingItemsAsync(cancellationToken);

        return new CalendarSectionDto(
            calendarEvents.Select(e => new CalendarEventDto(e.Start.ToString("HH:mm"), e.Title)).ToList(),
            scheduleItems.Select(s => new ScheduleItemDto(s.Start.ToString("HH:mm"), s.Title)).ToList());
    }

    public async Task<NewsSectionDto> GetNewsAsync(CancellationToken cancellationToken)
    {
        var nationalNews = await newsService.GetNationalAsync(cancellationToken);
        var localNews = await newsService.GetLocalAsync(cancellationToken);

        return new NewsSectionDto(
            nationalNews.Select(MapNewsArticle).ToList(),
            localNews.Select(MapNewsArticle).ToList());
    }

    public async Task<AirPatrolDto?> GetCabinAsync(CancellationToken cancellationToken)
    {
        var (airPatrol, _) = await GetCabinWithHistoryAsync(cancellationToken);
        return airPatrol;
    }

    public async Task<IndoorDto> GetIndoorAsync(CancellationToken cancellationToken)
    {
        var indoor = await indoorSensorService.GetCurrentAsync(cancellationToken);
        return new IndoorDto(indoor.Temperature, indoor.Humidity);
    }

    private async Task<List<WeatherDto>> GetWeatherLocationsAsync(CancellationToken cancellationToken)
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

        return weatherLocations;
    }

    private async Task<(AirPatrolDto? AirPatrol, IReadOnlyList<HouseholdPanel.Domain.AirPatrol.AirPatrolStatus> History)> GetCabinWithHistoryAsync(
        CancellationToken cancellationToken)
    {
        var airPatrol = await airPatrolService.GetStatusAsync(cancellationToken);
        var airPatrolHistory = await airPatrolHistoryRepository.GetSinceAsync(
            DateTimeOffset.UtcNow.AddDays(-7),
            cancellationToken);
        airPatrol ??= airPatrolHistory.LastOrDefault();

        var dto = airPatrol is null
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
                    .ToList());

        return (dto, airPatrolHistory);
    }

    private static NewsArticleDto MapNewsArticle(HouseholdPanel.Domain.News.NewsArticle article) =>
        new(article.Title, article.Summary, article.Source, article.PublishedAt, article.Url);
}
