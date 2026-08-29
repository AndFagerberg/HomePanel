namespace HouseholdPanel.Application.Configuration;

public sealed class WeatherOptions
{
    public const string SectionName = "Weather";

    public double Latitude { get; init; }
    public double Longitude { get; init; }
}
