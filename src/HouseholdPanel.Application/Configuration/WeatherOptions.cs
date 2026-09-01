namespace HouseholdPanel.Application.Configuration;

public sealed class WeatherOptions
{
    public const string SectionName = "Weather";

    // First location is primary (shown on the Home view); the rest are shown on the Weather view only.
    public IReadOnlyList<WeatherLocationOptions> Locations { get; init; } = [];
}

public sealed class WeatherLocationOptions
{
    public string Name { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}
