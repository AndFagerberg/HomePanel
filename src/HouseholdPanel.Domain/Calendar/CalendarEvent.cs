namespace HouseholdPanel.Domain.Calendar;

public sealed record CalendarEvent(DateTimeOffset Start, string Title);
