using HouseholdPanel.Application.Abstractions;
using HouseholdPanel.Domain.Transport;

namespace HouseholdPanel.Infrastructure.Transport;

// Fas 1 placeholder with no departures. Replaced by a real transport provider in Fas 3.
public sealed class TestDataTransportService : ITransportService
{
    public Task<IReadOnlyList<Departure>> GetDeparturesAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<Departure>>([]);
    }
}
