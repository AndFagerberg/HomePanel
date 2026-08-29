using HouseholdPanel.Domain.Weather;

namespace HouseholdPanel.Application.Abstractions;

public interface IWeatherService
{
    Task<WeatherForecast> GetCurrentAsync(CancellationToken cancellationToken);
}
