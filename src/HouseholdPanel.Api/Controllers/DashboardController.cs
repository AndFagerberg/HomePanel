using HouseholdPanel.Application.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace HouseholdPanel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DashboardController(IDashboardQueryService dashboardQueryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get(CancellationToken cancellationToken)
    {
        var dashboard = await dashboardQueryService.GetDashboardAsync(cancellationToken);

        return Ok(dashboard);
    }

    [HttpGet("weather")]
    public async Task<ActionResult<WeatherSectionDto>> GetWeather(CancellationToken cancellationToken) =>
        Ok(await dashboardQueryService.GetWeatherAsync(cancellationToken));

    [HttpGet("transport")]
    public async Task<ActionResult<TransportDto>> GetTransport(CancellationToken cancellationToken) =>
        Ok(await dashboardQueryService.GetTransportAsync(cancellationToken));

    [HttpGet("calendar")]
    public async Task<ActionResult<CalendarSectionDto>> GetCalendar(CancellationToken cancellationToken) =>
        Ok(await dashboardQueryService.GetCalendarAsync(cancellationToken));

    [HttpGet("news")]
    public async Task<ActionResult<NewsSectionDto>> GetNews(CancellationToken cancellationToken) =>
        Ok(await dashboardQueryService.GetNewsAsync(cancellationToken));

    [HttpGet("cabin")]
    public async Task<ActionResult<AirPatrolDto?>> GetCabin(CancellationToken cancellationToken) =>
        Ok(await dashboardQueryService.GetCabinAsync(cancellationToken));

    [HttpGet("indoor")]
    public async Task<ActionResult<IndoorDto>> GetIndoor(CancellationToken cancellationToken) =>
        Ok(await dashboardQueryService.GetIndoorAsync(cancellationToken));
}
