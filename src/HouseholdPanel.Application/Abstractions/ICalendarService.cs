using HouseholdPanel.Domain.Calendar;

namespace HouseholdPanel.Application.Abstractions;

public interface ICalendarService
{
    Task<IReadOnlyList<CalendarEvent>> GetUpcomingEventsAsync(CancellationToken cancellationToken);
}
