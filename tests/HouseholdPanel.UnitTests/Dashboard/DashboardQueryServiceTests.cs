using HouseholdPanel.Application.Abstractions;
using HouseholdPanel.Application.Configuration;
using HouseholdPanel.Application.Dashboard;
using HouseholdPanel.Domain.Calendar;
using HouseholdPanel.Domain.Indoor;
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
        var transportOptions = Options.Create(new TransportOptions { StopName = "Centralen" });

        var sut = new DashboardQueryService(
            weatherService,
            indoorSensorService,
            transportService,
            calendarService,
            scheduleService,
            transportOptions);

        var dashboard = await sut.GetDashboardAsync(CancellationToken.None);

        Assert.Equal(19.0m, dashboard.Weather.Temperature);
        Assert.Equal(20.5m, dashboard.Indoor.Temperature);
        Assert.Equal("Centralen", dashboard.Transport.StopName);
        Assert.Single(dashboard.Transport.Departures);
        Assert.Equal("3", dashboard.Transport.Departures[0].Line);
        Assert.Single(dashboard.Calendar);
        Assert.Empty(dashboard.Schedule);
    }

    private sealed class FakeWeatherService : IWeatherService
    {
        public Task<WeatherForecast> GetCurrentAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new WeatherForecast(19.0m, 12.0m, 20.0m, "cloudy", 20, 4.0m));
    }

    private sealed class FakeIndoorSensorService : IIndoorSensorService
    {
        public Task<IndoorReading> GetCurrentAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new IndoorReading(20.5m, 45));
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
}
