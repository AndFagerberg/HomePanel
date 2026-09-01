using System.Net;
using System.Net.Http.Json;
using HouseholdPanel.Application.Abstractions;
using HouseholdPanel.Application.Configuration;
using HouseholdPanel.Application.Dashboard;
using HouseholdPanel.Domain.Weather;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HouseholdPanel.IntegrationTests.Dashboard;

public sealed class DashboardEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Get_ReturnsOkWithDashboardPayload()
    {
        // Replace the real SMHI-backed weather service so this test doesn't depend on network access.
        var client = factory
            .WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            {
                services.RemoveAll<IWeatherService>();
                services.AddSingleton<IWeatherService, FakeWeatherService>();
            }))
            .CreateClient();

        var response = await client.GetAsync("/api/dashboard");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dashboard = await response.Content.ReadFromJsonAsync<DashboardDto>();

        Assert.NotNull(dashboard);
        Assert.NotNull(dashboard!.Weather);
        Assert.NotNull(dashboard.Indoor);
    }

    private sealed class FakeWeatherService : IWeatherService
    {
        public Task<WeatherForecast> GetCurrentAsync(WeatherLocationOptions location, CancellationToken cancellationToken) =>
            Task.FromResult(new WeatherForecast(19.0m, 12.0m, 20.0m, "cloudy", 20, 4.0m));
    }
}
