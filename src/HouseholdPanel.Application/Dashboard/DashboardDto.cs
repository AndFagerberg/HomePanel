namespace HouseholdPanel.Application.Dashboard;

using HouseholdPanel.Application.Timers;

// Presentation-oriented DTO contract returned by GET /api/dashboard. Frontend never sees domain models.
public sealed record DashboardDto(
    DateTimeOffset Timestamp,
    WeatherDto Weather,
    IReadOnlyList<WeatherDto> WeatherLocations,
    IndoorDto Indoor,
    AirPatrolDto? AirPatrol,
    TransportDto Transport,
    IReadOnlyList<CalendarEventDto> Calendar,
    IReadOnlyList<ScheduleItemDto> Schedule,
    IReadOnlyList<NewsArticleDto> NationalNews,
    IReadOnlyList<NewsArticleDto> LocalNews,
    IReadOnlyList<TimerDto> Timers);

public sealed record WeatherDto(
    string Name,
    decimal Temperature,
    decimal MinimumTemperature,
    decimal MaximumTemperature,
    string Symbol,
    int PrecipitationProbability,
    decimal WindSpeed)
{
    public decimal? TomorrowMinimumTemperature { get; init; }
    public decimal? TomorrowMaximumTemperature { get; init; }
    public string? TomorrowSymbol { get; init; }
}

public sealed record IndoorDto(decimal Temperature, int Humidity);

public sealed record AirPatrolDto(
    string Name,
    decimal Temperature,
    int? Humidity,
    bool Power,
    string Mode,
    decimal? TargetTemperature,
    string FanSpeed,
    bool Swing,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<AirPatrolHistoryPointDto> History);

public sealed record AirPatrolHistoryPointDto(
    DateTimeOffset Timestamp,
    decimal Temperature,
    int? Humidity,
    decimal? TargetTemperature);

public sealed record TransportDto(string StopName, IReadOnlyList<DepartureDto> Departures);

public sealed record DepartureDto(string Departure, string Destination, string Line, int Minutes);

public sealed record CalendarEventDto(string Start, string Title);

public sealed record ScheduleItemDto(string Start, string Title);

public sealed record NewsArticleDto(
    string Title,
    string Summary,
    string Source,
    DateTimeOffset PublishedAt,
    string Url);
