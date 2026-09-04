namespace HouseholdPanel.Application.Configuration;

public sealed class CalendarOptions
{
    public const string SectionName = "Calendar";

    public string Provider { get; init; } = string.Empty;
    public string CalendarId { get; init; } = string.Empty;

    // OAuth 2.0 for Google Calendar access.
    public string GoogleClientId { get; init; } = string.Empty;
    public string GoogleClientSecret { get; init; } = string.Empty;
    public string GoogleRefreshToken { get; init; } = string.Empty;

    public int LookaheadHours { get; init; } = 24;
}
