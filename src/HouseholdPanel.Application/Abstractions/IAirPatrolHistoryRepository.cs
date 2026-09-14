using HouseholdPanel.Domain.AirPatrol;

namespace HouseholdPanel.Application.Abstractions;

public interface IAirPatrolHistoryRepository
{
    Task SaveAsync(AirPatrolStatus status, CancellationToken cancellationToken);

    Task<IReadOnlyList<AirPatrolStatus>> GetSinceAsync(
        DateTimeOffset since,
        CancellationToken cancellationToken);
}