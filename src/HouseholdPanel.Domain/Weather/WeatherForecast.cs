namespace HouseholdPanel.Domain.Weather;

public sealed record WeatherForecast(
    decimal Temperature,
    decimal MinimumTemperature,
    decimal MaximumTemperature,
    string Symbol,
    int PrecipitationProbability,
    decimal WindSpeed);
