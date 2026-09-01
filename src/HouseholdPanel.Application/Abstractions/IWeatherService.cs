using HouseholdPanel.Application.Configuration;
using HouseholdPanel.Domain.Weather;

namespace HouseholdPanel.Application.Abstractions;

public interface IWeatherService
{
    Task<WeatherForecast> GetCurrentAsync(WeatherLocationOptions location, CancellationToken cancellationToken);
}
