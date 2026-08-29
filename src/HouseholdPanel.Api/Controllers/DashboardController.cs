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
}
