using HouseholdPanel.Application.Abstractions;
using HouseholdPanel.Domain.Calendar;

namespace HouseholdPanel.Infrastructure.Calendar;

// Fas 1 placeholder with no events. Replaced by a real calendar provider in Fas 4.
public sealed class TestDataCalendarService : ICalendarService
{
    public Task<IReadOnlyList<CalendarEvent>> GetUpcomingEventsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<CalendarEvent>>([]);
    }
}
