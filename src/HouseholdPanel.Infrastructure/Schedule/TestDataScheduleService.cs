using HouseholdPanel.Application.Abstractions;
using HouseholdPanel.Domain.Schedule;

namespace HouseholdPanel.Infrastructure.Schedule;

// Fas 1 placeholder with no items. Configurable schedule source can be added later.
public sealed class TestDataScheduleService : IScheduleService
{
    public Task<IReadOnlyList<ScheduleItem>> GetUpcomingItemsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<ScheduleItem>>([]);
    }
}
