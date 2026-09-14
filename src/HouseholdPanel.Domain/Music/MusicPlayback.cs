namespace HouseholdPanel.Domain.Music;

public sealed record MusicPlayback(
    string Title,
    string Artist,
    string? Album,
    string? ImageUrl,
    bool IsPlaying);