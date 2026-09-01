using System.Net.Http.Json;
using System.Text.Json.Serialization;
using HouseholdPanel.Application.Abstractions;
using HouseholdPanel.Application.Configuration;
using HouseholdPanel.Domain.Transport;
using Microsoft.Extensions.Options;

namespace HouseholdPanel.Infrastructure.Transport;

// Fas 3: fetches real-time departures from Trafiklab's Realtime Timetables API (covers Länstrafiken Kronoberg).
public sealed class TrafiklabTransportService(HttpClient httpClient, IOptions<TransportOptions> options) : ITransportService
{
    // Trafiklab's "scheduled"/"realtime" timestamps have no UTC offset and represent local Swedish wall-clock
    // time (per their docs), but System.Text.Json's default DateTimeOffset converter assumes UTC when no offset
    // is present - that silently shifted every departure by the CEST/CET offset. Parse against this zone instead.
    private static readonly TimeZoneInfo StockholmTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");

    public async Task<IReadOnlyList<Departure>> GetDeparturesAsync(CancellationToken cancellationToken)
    {
        var transportOptions = options.Value;
        if (string.IsNullOrWhiteSpace(transportOptions.StopId) || string.IsNullOrWhiteSpace(transportOptions.ApiKey))
        {
            return [];
        }

        var requestUri = $"v1/departures/{transportOptions.StopId}?key={transportOptions.ApiKey}";
        var response = await httpClient.GetFromJsonAsync<TrafiklabResponse>(requestUri, cancellationToken)
            ?? throw new InvalidOperationException("Trafiklab returned no departures data.");

        var now = DateTimeOffset.Now;

        return response.Departures
            .Where(d => !d.Canceled)
            .Where(d => string.IsNullOrWhiteSpace(transportOptions.Line)
                || string.Equals(d.Route.Designation, transportOptions.Line, StringComparison.OrdinalIgnoreCase))
            .Where(d => string.IsNullOrWhiteSpace(transportOptions.Direction)
                || d.Route.Direction.Contains(transportOptions.Direction, StringComparison.OrdinalIgnoreCase))
            .Select(d =>
            {
                var departureTime = ParseStockholmLocalTime(d.Realtime ?? d.Scheduled);
                return new Departure(
                    DepartureTime: departureTime,
                    Destination: d.Route.Direction,
                    Line: d.Route.Designation,
                    MinutesUntilDeparture: Math.Max(0, (int)Math.Round((departureTime - now).TotalMinutes)));
            })
            .OrderBy(d => d.DepartureTime)
            .ToList();
    }

    private static DateTimeOffset ParseStockholmLocalTime(string value)
    {
        var localDateTime = DateTime.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
        var offset = StockholmTimeZone.GetUtcOffset(localDateTime);
        return new DateTimeOffset(localDateTime, offset);
    }

    private sealed class TrafiklabResponse
    {
        [JsonPropertyName("departures")]
        public List<TrafiklabDeparture> Departures { get; init; } = [];
    }

    private sealed class TrafiklabDeparture
    {
        [JsonPropertyName("scheduled")]
        public string Scheduled { get; init; } = string.Empty;

        [JsonPropertyName("realtime")]
        public string? Realtime { get; init; }

        [JsonPropertyName("canceled")]
        public bool Canceled { get; init; }

        [JsonPropertyName("route")]
        public TrafiklabRoute Route { get; init; } = new();
    }

    private sealed class TrafiklabRoute
    {
        [JsonPropertyName("designation")]
        public string Designation { get; init; } = string.Empty;

        [JsonPropertyName("direction")]
        public string Direction { get; init; } = string.Empty;
    }
}
