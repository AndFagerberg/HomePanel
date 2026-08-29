namespace HouseholdPanel.Application.Dashboard;

// Presentation-oriented DTO contract returned by GET /api/dashboard. Frontend never sees domain models.
public sealed record DashboardDto(
    DateTimeOffset Timestamp,
    WeatherDto Weather,
    IndoorDto Indoor,
    TransportDto Transport,
    IReadOnlyList<CalendarEventDto> Calendar,
    IReadOnlyList<ScheduleItemDto> Schedule);

public sealed record WeatherDto(
    decimal Temperature,
    decimal MinimumTemperature,
    decimal MaximumTemperature,
    string Symbol,
    int PrecipitationProbability,
    decimal WindSpeed);

public sealed record IndoorDto(decimal Temperature, int Humidity);

public sealed record TransportDto(string StopName, IReadOnlyList<DepartureDto> Departures);

public sealed record DepartureDto(string Departure, string Destination, string Line, int Minutes);

public sealed record CalendarEventDto(string Start, string Title);

public sealed record ScheduleItemDto(string Start, string Title);
