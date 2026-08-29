using HouseholdPanel.Domain.Transport;

namespace HouseholdPanel.Application.Abstractions;

public interface ITransportService
{
    Task<IReadOnlyList<Departure>> GetDeparturesAsync(CancellationToken cancellationToken);
}
