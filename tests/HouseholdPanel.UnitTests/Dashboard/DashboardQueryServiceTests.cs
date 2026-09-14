using HouseholdPanel.Application.Abstractions;
using HouseholdPanel.Application.Configuration;
using HouseholdPanel.Application.Dashboard;
using HouseholdPanel.Application.Timers;
using HouseholdPanel.Domain.AirPatrol;
using HouseholdPanel.Domain.Calendar;
using HouseholdPanel.Domain.Indoor;
using HouseholdPanel.Domain.Music;
using HouseholdPanel.Domain.News;
using HouseholdPanel.Domain.Schedule;
using HouseholdPanel.Domain.Transport;
using HouseholdPanel.Domain.Weather;
using Microsoft.Extensions.Options;

namespace HouseholdPanel.UnitTests.Dashboard;

public sealed class DashboardQueryServiceTests
{
    [Fact]
    public async Task GetDashboardAsync_MapsDomainDataOntoPresentationDto()
    {
        var weatherService = new FakeWeatherService();
        var indoorSensorService = new FakeIndoorSensorService();
        var transportService = new FakeTransportService();
        var calendarService = new FakeCalendarService();
        var scheduleService = new FakeScheduleService();
        var newsService = new FakeNewsService();
        var airPatrolService = new FakeAirPatrolService();
        var airPatrolHistoryRepository = new FakeAirPatrolHistoryRepository();
        var musicService = new FakeMusicService();
        var timerService = new TimerService(new FakeTimerService());
        var weatherOptions = Options.Create(new WeatherOptions
        {
            Locations = [new WeatherLocationOptions { Name = "Öjaby", Latitude = 56.9243, Longitude = 14.7429 }],
        });
        var transportOptions = Options.Create(new TransportOptions { StopName = "Centralen" });

        var sut = new DashboardQueryService(
            weatherService,
            indoorSensorService,
            transportService,
            calendarService,
            scheduleService,
            newsService,
            airPatrolService,
            airPatrolHistoryRepository,
            musicService,
            timerService,
            weatherOptions,
            transportOptions);

        var dashboard = await sut.GetDashboardAsync(CancellationToken.None);

        Assert.Equal(19.0m, dashboard.Weather.Temperature);
        Assert.Equal("Öjaby", dashboard.Weather.Name);
        Assert.Single(dashboard.WeatherLocations);
        Assert.Equal(20.5m, dashboard.Indoor.Temperature);
        Assert.Equal("Centralen", dashboard.Transport.StopName);
        Assert.Single(dashboard.Transport.Departures);
        Assert.Equal("3", dashboard.Transport.Departures[0].Line);
        Assert.Single(dashboard.Calendar);
        Assert.Empty(dashboard.Schedule);
        Assert.Single(dashboard.NationalNews);
        Assert.Single(dashboard.LocalNews);
        Assert.Equal("Stugan", dashboard.AirPatrol?.Name);
        Assert.Equal(12.3m, dashboard.AirPatrol?.Temperature);
        Assert.Single(dashboard.AirPatrol?.History ?? []);
        Assert.Empty(dashboard.Timers);
    }

    private sealed class FakeWeatherService : IWeatherService
    {
        public Task<WeatherForecast> GetCurrentAsync(WeatherLocationOptions location, CancellationToken cancellationToken) =>
            Task.FromResult(new WeatherForecast(19.0m, 12.0m, 20.0m, "cloudy", 20, 4.0m));
    }

    private sealed class FakeIndoorSensorService : IIndoorSensorService
    {
        public Task<IndoorReading> GetCurrentAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new IndoorReading(20.5m, 45));
    }

    private sealed class FakeMusicService : IMusicService
    {
        public Task<IReadOnlyList<RadioStation>> GetRadioStationsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<RadioStation>>([]);

        public Task<IReadOnlyList<MusicSearchResult>> SearchSpotifyAsync(string query, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<MusicSearchResult>>([]);

        public Task<MusicPlayback?> GetCurrentPlaybackAsync(CancellationToken cancellationToken) =>
            Task.FromResult<MusicPlayback?>(null);

        public Task PlaySpotifyAsync(string uri, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopPlaybackAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task PlayRadioAsync(string stationId, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeTransportService : ITransportService
    {
        public Task<IReadOnlyList<Departure>> GetDeparturesAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Departure>>([
                new Departure(DateTimeOffset.Now.AddMinutes(6), "Centrum", "3", 6)
            ]);
    }

    private sealed class FakeCalendarService : ICalendarService
    {
        public Task<IReadOnlyList<CalendarEvent>> GetUpcomingEventsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<CalendarEvent>>([
                new CalendarEvent(DateTimeOffset.Now.AddHours(1), "Middag")
            ]);
    }

    private sealed class FakeScheduleService : IScheduleService
    {
        public Task<IReadOnlyList<ScheduleItem>> GetUpcomingItemsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<ScheduleItem>>([]);
    }

    private sealed class FakeNewsService : INewsService
    {
        public Task<IReadOnlyList<NewsArticle>> GetNationalAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<NewsArticle>>([
                new NewsArticle("Riksnyhet", "Ingress", "SVT Nyheter", DateTimeOffset.Now, "https://www.svt.se/nyheter")
            ]);

        public Task<IReadOnlyList<NewsArticle>> GetLocalAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<NewsArticle>>([
                new NewsArticle("Smålandsnyhet", "Ingress", "SVT Nyheter Småland", DateTimeOffset.Now, "https://www.svt.se/nyheter/lokalt/smaland")
            ]);
    }

    private sealed class FakeAirPatrolService : IAirPatrolService
    {
        public Task<AirPatrolStatus?> GetStatusAsync(CancellationToken cancellationToken) =>
            Task.FromResult<AirPatrolStatus?>(new AirPatrolStatus(
                "Stugan",
                12.3m,
                52,
                true,
                "lowheat",
                10m,
                "auto",
                true,
                DateTimeOffset.UtcNow));
    }

    private sealed class FakeAirPatrolHistoryRepository : IAirPatrolHistoryRepository
    {
        public Task SaveAsync(AirPatrolStatus status, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<IReadOnlyList<AirPatrolStatus>> GetSinceAsync(
            DateTimeOffset since,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<AirPatrolStatus>>([
                new AirPatrolStatus(
                    "Stugan",
                    11.8m,
                    51,
                    true,
                    "lowheat",
                    10m,
                    "auto",
                    true,
                    DateTimeOffset.UtcNow.AddHours(-1)),
            ]);
    }

    private sealed class FakeTimerService : ITimerService
    {
        public Task<IReadOnlyList<HouseholdPanel.Domain.Timers.Timer>> GetActiveAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<HouseholdPanel.Domain.Timers.Timer>>([]);

        public Task<HouseholdPanel.Domain.Timers.Timer> CreateAsync(
            string name,
            TimeSpan duration,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<bool> CancelAsync(Guid id, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
