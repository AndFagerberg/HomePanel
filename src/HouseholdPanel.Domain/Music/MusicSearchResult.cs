namespace HouseholdPanel.Domain.Music;

public sealed record MusicSearchResult(
    string Id,
    string Title,
    string Subtitle,
    string Uri,
    string Type,
    string? ImageUrl);