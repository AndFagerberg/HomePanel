using HouseholdPanel.Application.Abstractions;
using HouseholdPanel.Domain.Weather;

namespace HouseholdPanel.Infrastructure.Weather;

// Fas 1 placeholder returning static test data. Replaced by an SMHI-backed implementation in Fas 2.
public sealed class TestDataWeatherService : IWeatherService
{
    public Task<WeatherForecast> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var forecast = new WeatherForecast(
            Temperature: 19.0m,
            MinimumTemperature: 12.0m,
            MaximumTemperature: 20.0m,
            Symbol: "cloudy",
            PrecipitationProbability: 20,
            WindSpeed: 4.0m);

        return Task.FromResult(forecast);
    }
}
