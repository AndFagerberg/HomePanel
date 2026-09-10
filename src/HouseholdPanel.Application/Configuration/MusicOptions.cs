namespace HouseholdPanel.Application.Configuration;

public sealed class MusicOptions
{
    public const string SectionName = "Music";

    public SpotifyOptions Spotify { get; init; } = new();

    public GoogleHomeOptions GoogleHome { get; init; } = new();

    public IReadOnlyList<RadioStationOptions> RadioStations { get; init; } = [
        new RadioStationOptions
        {
            Id = "p3",
            Name = "P3",
            StreamUrl = "https://www.sverigesradio.se/topsy/direkt/srapi/164.mp3",
        },
        new RadioStationOptions
        {
            Id = "p4-kronoberg",
            Name = "P4 Kronoberg",
            StreamUrl = "https://www.sverigesradio.se/topsy/direkt/srapi/214.mp3",
        },
    ];
}

public sealed class SpotifyOptions
{
    public string ClientId { get; init; } = string.Empty;

    public string ClientSecret { get; init; } = string.Empty;

    public string RefreshToken { get; init; } = string.Empty;

    public string DeviceId { get; init; } = string.Empty;

    public int SearchLimit { get; init; } = 8;
}

public sealed class GoogleHomeOptions
{
    public string DeviceName { get; init; } = string.Empty;

    public string CastExecutable { get; init; } = "catt";

    public IReadOnlyList<string> CastArguments { get; init; } = ["-d", "{DeviceName}", "cast", "{Url}"];
}

public sealed class RadioStationOptions
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string StreamUrl { get; init; } = string.Empty;
}