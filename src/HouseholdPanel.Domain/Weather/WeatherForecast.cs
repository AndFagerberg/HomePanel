namespace HouseholdPanel.Domain.Weather;

public sealed record WeatherForecast(
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
