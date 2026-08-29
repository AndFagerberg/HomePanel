using HouseholdPanel.Application.Abstractions;
using HouseholdPanel.Domain.Indoor;

namespace HouseholdPanel.Infrastructure.Indoor;

// Fas 1 placeholder returning static test data. Replaced once a real indoor sensor is wired up.
public sealed class TestDataIndoorSensorService : IIndoorSensorService
{
    public Task<IndoorReading> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var reading = new IndoorReading(Temperature: 21.4m, Humidity: 45);

        return Task.FromResult(reading);
    }
}
