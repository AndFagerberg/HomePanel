namespace HouseholdPanel.Application.Dashboard;

public interface IDashboardQueryService
{
    Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken);
}
