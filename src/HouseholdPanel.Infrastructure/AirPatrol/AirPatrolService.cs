using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HouseholdPanel.Application.Abstractions;
using HouseholdPanel.Application.Configuration;
using HouseholdPanel.Domain.AirPatrol;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace HouseholdPanel.Infrastructure.AirPatrol;

public sealed class AirPatrolService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    IAirPatrolHistoryRepository historyRepository,
    IOptions<AirPatrolOptions> options,
    TimeProvider timeProvider) : IAirPatrolService
{
    private const string AuthBaseUrl = "https://auth.apsrvd.io/v1";
    private const string ApiBaseUrl = "https://api.apsrvd.io/12";
    private const string CacheKey = "airpatrol-status";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public async Task<AirPatrolStatus?> GetStatusAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.Email) || string.IsNullOrWhiteSpace(settings.Password))
        {
            return null;
        }

        if (memoryCache.TryGetValue<AirPatrolStatus>(CacheKey, out var cached))
        {
            return cached;
        }

        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            if (memoryCache.TryGetValue<AirPatrolStatus>(CacheKey, out cached))
            {
                return cached;
            }

            var status = await FetchStatusAsync(settings, cancellationToken);
            await historyRepository.SaveAsync(status, cancellationToken);
            memoryCache.Set(CacheKey, status, CacheDuration);
            return status;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private async Task<AirPatrolStatus> FetchStatusAsync(
        AirPatrolOptions settings,
        CancellationToken cancellationToken)
    {
        using var loginResponse = await httpClient.PostAsJsonAsync(
            $"{AuthBaseUrl}/login",
            new { email = settings.Email, password = settings.Password },
            cancellationToken);
        loginResponse.EnsureSuccessStatusCode();

        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("AirPatrol returned an empty authentication response.");
        var accessToken = login.Misc?.AccessToken
            ?? throw new InvalidOperationException("AirPatrol did not return an access token.");

        var pairingId = settings.PairingId;
        if (string.IsNullOrWhiteSpace(pairingId))
        {
            pairingId = await GetFirstPairingIdAsync(accessToken, login.UserId, cancellationToken);
        }

        using var request = CreateAuthorizedRequest(HttpMethod.Get, $"{ApiBaseUrl}/command", accessToken);
        request.Headers.Add("X-Pairing-Id", pairingId);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);
        var root = document.RootElement;

        return new AirPatrolStatus(
            settings.Name,
            GetRequiredDecimal(root, "RoomTemp"),
            GetDecimal(root, "RoomHumidity") is { } humidity ? decimal.ToInt32(humidity) : null,
            string.Equals(GetString(root, "PumpPower"), "on", StringComparison.OrdinalIgnoreCase),
            GetString(root, "PumpMode") ?? "unknown",
            GetDecimal(root, "PumpTemp"),
            GetString(root, "FanSpeed") ?? "unknown",
            string.Equals(GetString(root, "Swing"), "on", StringComparison.OrdinalIgnoreCase),
            timeProvider.GetUtcNow());
    }

    private async Task<string> GetFirstPairingIdAsync(
        string accessToken,
        string? userId,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, $"{AuthBaseUrl}/pairings", accessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var pairings = await response.Content.ReadFromJsonAsync<PairingResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("AirPatrol returned an empty pairings response.");
        var pairingId = pairings.Entities?.PairingUsers?.List
            ?.FirstOrDefault(item => string.IsNullOrWhiteSpace(userId)
                || string.Equals(item.UserId, userId, StringComparison.OrdinalIgnoreCase))
            ?.PairingId;

        return pairingId ?? throw new InvalidOperationException("No AirPatrol device is paired with the configured user.");
    }

    private static HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static decimal GetRequiredDecimal(JsonElement root, string propertyName) =>
        GetDecimal(root, propertyName)
        ?? throw new InvalidOperationException($"AirPatrol response did not contain {propertyName}.");

    private static decimal? GetDecimal(JsonElement root, string propertyName)
    {
        var value = FindProperty(root, propertyName);
        if (value is null)
        {
            return null;
        }

        return value.Value.ValueKind switch
        {
            JsonValueKind.Number when value.Value.TryGetDecimal(out var number) => number,
            JsonValueKind.String when decimal.TryParse(
                value.Value.GetString(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var number) => number,
            _ => null,
        };
    }

    private static string? GetString(JsonElement root, string propertyName)
    {
        var value = FindProperty(root, propertyName);
        return value?.ValueKind == JsonValueKind.String ? value.Value.GetString() : null;
    }

    private static JsonElement? FindProperty(JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    return property.Value;
                }

                if (FindProperty(property.Value, propertyName) is { } nested)
                {
                    return nested;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (FindProperty(item, propertyName) is { } nested)
                {
                    return nested;
                }
            }
        }

        return null;
    }

    private sealed class LoginResponse
    {
        [JsonPropertyName("entities")]
        public LoginEntities? Entities { get; init; }

        [JsonPropertyName("misc")]
        public LoginMisc? Misc { get; init; }

        public string? UserId => Entities?.Users?.List?.FirstOrDefault()?.Id;
    }

    private sealed class LoginEntities
    {
        [JsonPropertyName("users")]
        public EntityList<User>? Users { get; init; }
    }

    private sealed class LoginMisc
    {
        [JsonPropertyName("accessToken")]
        public string? AccessToken { get; init; }
    }

    private sealed class User
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }
    }

    private sealed class PairingResponse
    {
        [JsonPropertyName("entities")]
        public PairingEntities? Entities { get; init; }
    }

    private sealed class PairingEntities
    {
        [JsonPropertyName("pairingUser")]
        public EntityList<PairingUser>? PairingUsers { get; init; }
    }

    private sealed class PairingUser
    {
        [JsonPropertyName("userId")]
        public string? UserId { get; init; }

        [JsonPropertyName("pairingId")]
        public string? PairingId { get; init; }
    }

    private sealed class EntityList<T>
    {
        [JsonPropertyName("list")]
        public List<T>? List { get; init; }
    }
}