using HouseholdPanel.Domain.AirPatrol;

namespace HouseholdPanel.Application.Abstractions;

public interface IAirPatrolService
{
    Task<AirPatrolStatus?> GetStatusAsync(CancellationToken cancellationToken);
}