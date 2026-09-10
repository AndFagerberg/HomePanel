namespace HouseholdPanel.Application.Music;

public sealed record MusicSearchResultDto(
    string Id,
    string Title,
    string Subtitle,
    string Uri,
    string Type,
    string? ImageUrl);

public sealed record RadioStationDto(
    string Id,
    string Name);

public sealed record PlaySpotifyRequest(string Uri);

public sealed record PlayRadioRequest(string StationId);