using HouseholdPanel.Domain.Indoor;

namespace HouseholdPanel.Application.Abstractions;

public interface IIndoorSensorService
{
    Task<IndoorReading> GetCurrentAsync(CancellationToken cancellationToken);
}
