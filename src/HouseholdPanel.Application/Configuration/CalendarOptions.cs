namespace HouseholdPanel.Application.Configuration;

public sealed class CalendarOptions
{
    public const string SectionName = "Calendar";

    public string Provider { get; init; } = string.Empty;
    public string CalendarId { get; init; } = string.Empty;
}
