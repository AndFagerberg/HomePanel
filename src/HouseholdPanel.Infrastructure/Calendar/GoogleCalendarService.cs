using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using HouseholdPanel.Application.Abstractions;
using HouseholdPanel.Application.Configuration;
using HouseholdPanel.Domain.Calendar;
using Microsoft.Extensions.Options;

namespace HouseholdPanel.Infrastructure.Calendar;

public sealed class GoogleCalendarService(HttpClient httpClient, IOptions<CalendarOptions> options) : ICalendarService
{
    public async Task<IReadOnlyList<CalendarEvent>> GetUpcomingEventsAsync(CancellationToken cancellationToken)
    {
        var calendarOptions = options.Value;
        if (!string.Equals(calendarOptions.Provider, "Google", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(calendarOptions.CalendarId))
        {
            return [];
        }

        try
        {
            var accessToken = await GetAccessTokenAsync(calendarOptions, cancellationToken);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return [];
            }

            var now = DateTimeOffset.UtcNow;
            var lookAhead = now.AddHours(Math.Max(1, calendarOptions.LookaheadHours));
            var requestUri = new Uri(
                $"https://www.googleapis.com/calendar/v3/calendars/{Uri.EscapeDataString(calendarOptions.CalendarId)}/events?singleEvents=true&orderBy=startTime&timeMin={Uri.EscapeDataString(now.ToString("O"))}&timeMax={Uri.EscapeDataString(lookAhead.ToString("O"))}&maxResults=10");

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var calendarResponse = await response.Content.ReadFromJsonAsync<GoogleCalendarResponse>(cancellationToken: cancellationToken);
            if (calendarResponse?.Items is null || calendarResponse.Items.Count == 0)
            {
                return [];
            }

            return calendarResponse.Items
                .Where(item => !string.IsNullOrWhiteSpace(item.Summary))
                .Select(item => new CalendarEvent(ParseEventStart(item.Start), item.Summary.Trim()))
                .Where(item => item.Start >= now && item.Start <= lookAhead)
                .OrderBy(item => item.Start)
                .Take(10)
                .ToList();
        }
        catch (HttpRequestException)
        {
            return [];
        }
        catch (TaskCanceledException)
        {
            return [];
        }
        catch (InvalidOperationException)
        {
            return [];
        }
    }

    private async Task<string?> GetAccessTokenAsync(CalendarOptions calendarOptions, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(calendarOptions.GoogleRefreshToken)
            || string.IsNullOrWhiteSpace(calendarOptions.GoogleClientId)
            || string.IsNullOrWhiteSpace(calendarOptions.GoogleClientSecret))
        {
            return null;
        }

        try
        {
            var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = calendarOptions.GoogleClientId,
                ["client_secret"] = calendarOptions.GoogleClientSecret,
                ["refresh_token"] = calendarOptions.GoogleRefreshToken,
                ["grant_type"] = "refresh_token",
            });

            using var tokenResponse = await httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequest, cancellationToken);
            if (!tokenResponse.IsSuccessStatusCode)
            {
                return null;
            }

            var tokenPayload = await tokenResponse.Content.ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken: cancellationToken);
            return tokenPayload?.AccessToken;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private static DateTimeOffset ParseEventStart(GoogleCalendarEventTime? eventTime)
    {
        if (eventTime is null)
        {
            return DateTimeOffset.MinValue;
        }

        if (!string.IsNullOrWhiteSpace(eventTime.DateTime))
        {
            return DateTimeOffset.Parse(eventTime.DateTime, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
        }

        if (!string.IsNullOrWhiteSpace(eventTime.Date))
        {
            return DateTimeOffset.Parse(eventTime.Date, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
        }

        return DateTimeOffset.MinValue;
    }

    private sealed class GoogleTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; init; }
    }

    private sealed class GoogleCalendarResponse
    {
        [JsonPropertyName("items")]
        public List<GoogleCalendarEvent> Items { get; init; } = [];
    }

    private sealed class GoogleCalendarEvent
    {
        [JsonPropertyName("summary")]
        public string Summary { get; init; } = string.Empty;

        [JsonPropertyName("start")]
        public GoogleCalendarEventTime? Start { get; init; }
    }

    private sealed class GoogleCalendarEventTime
    {
        [JsonPropertyName("dateTime")]
        public string? DateTime { get; init; }

        [JsonPropertyName("date")]
        public string? Date { get; init; }
    }
}
