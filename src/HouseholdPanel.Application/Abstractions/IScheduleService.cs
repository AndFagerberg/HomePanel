using HouseholdPanel.Domain.Schedule;

namespace HouseholdPanel.Application.Abstractions;

public interface IScheduleService
{
    Task<IReadOnlyList<ScheduleItem>> GetUpcomingItemsAsync(CancellationToken cancellationToken);
}
