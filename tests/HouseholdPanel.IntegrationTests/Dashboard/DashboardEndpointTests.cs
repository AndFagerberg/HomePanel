using System.Net;
using System.Net.Http.Json;
using HouseholdPanel.Application.Dashboard;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HouseholdPanel.IntegrationTests.Dashboard;

public sealed class DashboardEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Get_ReturnsOkWithDashboardPayload()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/dashboard");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dashboard = await response.Content.ReadFromJsonAsync<DashboardDto>();

        Assert.NotNull(dashboard);
        Assert.NotNull(dashboard!.Weather);
        Assert.NotNull(dashboard.Indoor);
    }
}
