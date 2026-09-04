using System.Net;
using System.Net.Http;
using System.Text;
using HouseholdPanel.Application.Configuration;
using HouseholdPanel.Infrastructure.Calendar;
using Microsoft.Extensions.Options;

namespace HouseholdPanel.UnitTests.Calendar;

public sealed class GoogleCalendarServiceTests
{
    [Fact]
    public async Task GetUpcomingEventsAsync_ReturnsMappedEvents_WhenGoogleCalendarIsConfigured()
    {
        var now = DateTimeOffset.UtcNow;
        var handler = new StubHttpMessageHandler(
            $"{{\n  \"items\": [\n    {{\n      \"summary\": \"Middag\",\n      \"start\": {{\n        \"dateTime\": \"{now.AddHours(1):yyyy-MM-ddTHH:mm:ssK}\"\n      }}\n    }},\n    {{\n      \"summary\": \"Löpning\",\n      \"start\": {{\n        \"dateTime\": \"{now.AddHours(2):yyyy-MM-ddTHH:mm:ssK}\"\n      }}\n    }}\n  ]\n}}");

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://www.googleapis.com/")
        };
        var options = Options.Create(new CalendarOptions
        {
            Provider = "Google",
            CalendarId = "familj%40group.calendar.google.com",
            GoogleClientId = "test-client-id",
            GoogleClientSecret = "test-client-secret",
            GoogleRefreshToken = "test-refresh-token",
        });

        var sut = new GoogleCalendarService(httpClient, options);

        var events = await sut.GetUpcomingEventsAsync(CancellationToken.None);

        Assert.Equal(2, events.Count);
        Assert.Equal("Middag", events[0].Title);
        Assert.Equal(now.AddHours(1).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssK"), events[0].Start.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssK"));
        Assert.Equal("Löpning", events[1].Title);
    }

    [Fact]
    public async Task GetUpcomingEventsAsync_FiltersToUpcomingEventsWithinLookaheadWindow()
    {
        var now = DateTimeOffset.UtcNow;
        var handler = new StubHttpMessageHandler(
            $"{{\n  \"items\": [\n    {{\n      \"summary\": \"Past\",\n      \"start\": {{\n        \"dateTime\": \"{now.AddHours(-2):yyyy-MM-ddTHH:mm:ssK}\"\n      }}\n    }},\n    {{\n      \"summary\": \"InWindow\",\n      \"start\": {{\n        \"dateTime\": \"{now.AddHours(1):yyyy-MM-ddTHH:mm:ssK}\"\n      }}\n    }},\n    {{\n      \"summary\": \"TooFarAhead\",\n      \"start\": {{\n        \"dateTime\": \"{now.AddDays(3):yyyy-MM-ddTHH:mm:ssK}\"\n      }}\n    }}\n  ]\n}}");

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://www.googleapis.com/")
        };
        var options = Options.Create(new CalendarOptions
        {
            Provider = "Google",
            CalendarId = "familj%40group.calendar.google.com",
            GoogleClientId = "test-client-id",
            GoogleClientSecret = "test-client-secret",
            GoogleRefreshToken = "test-refresh-token",
            LookaheadHours = 24,
        });

        var sut = new GoogleCalendarService(httpClient, options);

        var events = await sut.GetUpcomingEventsAsync(CancellationToken.None);

        var titles = events.Select(e => e.Title).ToList();
        Assert.Equal(["InWindow"], titles);
    }

    [Fact]
    public async Task GetUpcomingEventsAsync_ReturnsEmpty_WhenCalendarProviderIsNotConfigured()
    {
        var httpClient = new HttpClient(new StubHttpMessageHandler());
        var options = Options.Create(new CalendarOptions
        {
            Provider = string.Empty,
            CalendarId = string.Empty,
            GoogleClientId = string.Empty,
            GoogleClientSecret = string.Empty,
            GoogleRefreshToken = string.Empty
        });

        var sut = new GoogleCalendarService(httpClient, options);

        var events = await sut.GetUpcomingEventsAsync(CancellationToken.None);

        Assert.Empty(events);
    }

    [Fact]
    public async Task GetUpcomingEventsAsync_ReturnsEmpty_WhenGoogleReturnsInvalidPayload()
    {
        var httpClient = new HttpClient(new StubHttpMessageHandler("{\"items\": null}"));
        var options = Options.Create(new CalendarOptions
        {
            Provider = "Google",
            CalendarId = "familj%40group.calendar.google.com",
            GoogleClientId = "test-client-id",
            GoogleClientSecret = "test-client-secret",
            GoogleRefreshToken = "test-refresh-token"
        });

        var sut = new GoogleCalendarService(httpClient, options);

        var events = await sut.GetUpcomingEventsAsync(CancellationToken.None);

        Assert.Empty(events);
    }

    private sealed class StubHttpMessageHandler(string responseBody = "{}") : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri is not null && request.RequestUri.AbsoluteUri.Contains("oauth2.googleapis.com/token"))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"access_token\":\"test-access-token\"}", Encoding.UTF8, "application/json")
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            });
        }
    }
}
