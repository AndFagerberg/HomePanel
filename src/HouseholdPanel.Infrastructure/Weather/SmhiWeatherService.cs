using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using HouseholdPanel.Application.Abstractions;
using HouseholdPanel.Application.Configuration;
using HouseholdPanel.Domain.Weather;

namespace HouseholdPanel.Infrastructure.Weather;

// Fas 2: fetches point forecasts from SMHI's open meteorological forecast API (metfcst).
public sealed class SmhiWeatherService(HttpClient httpClient) : IWeatherService
{
    public async Task<WeatherForecast> GetCurrentAsync(WeatherLocationOptions location, CancellationToken cancellationToken)
    {
        var latitude = location.Latitude.ToString(CultureInfo.InvariantCulture);
        var longitude = location.Longitude.ToString(CultureInfo.InvariantCulture);
        var requestUri = $"api/category/snow1g/version/1/geotype/point/lon/{longitude}/lat/{latitude}/data.json";

        var response = await httpClient.GetFromJsonAsync<SmhiResponse>(requestUri, cancellationToken)
            ?? throw new InvalidOperationException("SMHI returned no forecast data.");

        var now = DateTimeOffset.UtcNow;
        var currentEntry = response.TimeSeries
            .OrderBy(entry => Math.Abs((entry.Time - now).Ticks))
            .First();

        var todaysTemperatures = response.TimeSeries
            .Where(entry => entry.Time.UtcDateTime.Date == now.UtcDateTime.Date)
            .Select(entry => entry.Data.Temperature)
            .ToList();

        return new WeatherForecast(
            Temperature: (decimal)currentEntry.Data.Temperature,
            MinimumTemperature: (decimal)(todaysTemperatures.Count > 0 ? todaysTemperatures.Min() : currentEntry.Data.Temperature),
            MaximumTemperature: (decimal)(todaysTemperatures.Count > 0 ? todaysTemperatures.Max() : currentEntry.Data.Temperature),
            Symbol: MapSymbol(currentEntry.Data.SymbolCode),
            // SMHI doesn't expose a precipitation probability directly; approximate it from expected mean precipitation.
            PrecipitationProbability: Math.Clamp((int)Math.Round(currentEntry.Data.PrecipitationAmountMean * 40), 0, 100),
            WindSpeed: (decimal)currentEntry.Data.WindSpeed);
    }

    // Maps SMHI's symbol_code weather symbol codes (1-27) to icon keys used by the frontend.
    private static string MapSymbol(int code) => code switch
    {
        1 => "clear",
        2 => "mostly-clear",
        3 or 4 => "partly-cloudy",
        5 => "cloudy",
        6 => "overcast",
        7 => "fog",
        8 => "rain-showers-light",
        9 => "rain-showers",
        10 => "rain-showers-heavy",
        11 or 21 => "thunder",
        12 => "sleet-showers-light",
        13 => "sleet-showers",
        14 => "sleet-showers-heavy",
        15 => "snow-showers-light",
        16 => "snow-showers",
        17 => "snow-showers-heavy",
        18 => "rain-light",
        19 => "rain",
        20 => "rain-heavy",
        22 => "sleet-light",
        23 => "sleet",
        24 => "sleet-heavy",
        25 => "snow-light",
        26 => "snow",
        27 => "snow-heavy",
        _ => "cloudy",
    };

    private sealed class SmhiResponse
    {
        [JsonPropertyName("timeSeries")]
        public List<SmhiTimeSeriesEntry> TimeSeries { get; init; } = [];
    }

    private sealed class SmhiTimeSeriesEntry
    {
        [JsonPropertyName("time")]
        public DateTimeOffset Time { get; init; }

        [JsonPropertyName("data")]
        public SmhiData Data { get; init; } = new();
    }

    private sealed class SmhiData
    {
        [JsonPropertyName("air_temperature")]
        public double Temperature { get; init; }

        [JsonPropertyName("wind_speed")]
        public double WindSpeed { get; init; }

        [JsonPropertyName("symbol_code")]
        public int SymbolCode { get; init; } = 1;

        [JsonPropertyName("precipitation_amount_mean")]
        public double PrecipitationAmountMean { get; init; }
    }
}
