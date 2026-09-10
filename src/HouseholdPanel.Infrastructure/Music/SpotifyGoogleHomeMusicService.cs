using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HouseholdPanel.Application.Abstractions;
using HouseholdPanel.Application.Configuration;
using HouseholdPanel.Domain.Music;
using Microsoft.Extensions.Options;

namespace HouseholdPanel.Infrastructure.Music;

public sealed class SpotifyGoogleHomeMusicService(
    HttpClient httpClient,
    IOptions<MusicOptions> options) : IMusicService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Task<IReadOnlyList<RadioStation>> GetRadioStationsAsync(CancellationToken cancellationToken)
    {
        var stations = options.Value.RadioStations
            .Where(station => !string.IsNullOrWhiteSpace(station.Id)
                && !string.IsNullOrWhiteSpace(station.Name)
                && !string.IsNullOrWhiteSpace(station.StreamUrl))
            .Select(station => new RadioStation(
                station.Id.Trim(),
                station.Name.Trim(),
                station.StreamUrl.Trim()))
            .DistinctBy(station => station.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Task.FromResult<IReadOnlyList<RadioStation>>(stations);
    }

    public async Task<IReadOnlyList<MusicSearchResult>> SearchSpotifyAsync(
        string query,
        CancellationToken cancellationToken)
    {
        var spotifyOptions = options.Value.Spotify;
        if (!IsSpotifyConfigured(spotifyOptions))
        {
            return [];
        }

        var accessToken = await GetSpotifyAccessTokenAsync(spotifyOptions, cancellationToken);
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://api.spotify.com/v1/search?type=track,album,artist,playlist&limit={Math.Clamp(spotifyOptions.SearchLimit, 1, 20)}&q={Uri.EscapeDataString(query)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<SpotifySearchResponse>(responseStream, JsonOptions, cancellationToken);

        return MapSearchResults(payload);
    }

    public async Task PlaySpotifyAsync(string uri, CancellationToken cancellationToken)
    {
        var spotifyOptions = options.Value.Spotify;
        if (!IsSpotifyConfigured(spotifyOptions) || string.IsNullOrWhiteSpace(spotifyOptions.DeviceId))
        {
            throw new InvalidOperationException("Spotify playback is not configured.");
        }

        var accessToken = await GetSpotifyAccessTokenAsync(spotifyOptions, cancellationToken);
        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"https://api.spotify.com/v1/me/player/play?device_id={Uri.EscapeDataString(spotifyOptions.DeviceId)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var body = uri.StartsWith("spotify:track:", StringComparison.OrdinalIgnoreCase)
            ? JsonSerializer.Serialize(new { uris = new[] { uri } }, JsonOptions)
            : JsonSerializer.Serialize(new { context_uri = uri }, JsonOptions);

        request.Content = new StringContent(body, Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task PlayRadioAsync(string stationId, CancellationToken cancellationToken)
    {
        var station = (await GetRadioStationsAsync(cancellationToken))
            .FirstOrDefault(candidate => string.Equals(candidate.Id, stationId, StringComparison.OrdinalIgnoreCase));
        if (station is null)
        {
            throw new InvalidOperationException("Radio station was not found.");
        }

        var googleHomeOptions = options.Value.GoogleHome;
        if (string.IsNullOrWhiteSpace(googleHomeOptions.CastExecutable)
            || string.IsNullOrWhiteSpace(googleHomeOptions.DeviceName))
        {
            throw new InvalidOperationException("Google Home casting is not configured.");
        }

        using var process = new Process();
        process.StartInfo.FileName = googleHomeOptions.CastExecutable;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;

        foreach (var argument in googleHomeOptions.CastArguments)
        {
            process.StartInfo.ArgumentList.Add(argument
                .Replace("{DeviceName}", googleHomeOptions.DeviceName, StringComparison.Ordinal)
                .Replace("{Url}", station.StreamUrl, StringComparison.Ordinal)
                .Replace("{Title}", station.Name, StringComparison.Ordinal));
        }

        try
        {
            if (!process.Start())
            {
                throw new InvalidOperationException("Could not start Google Home cast command.");
            }
        }
        catch (System.ComponentModel.Win32Exception exception)
        {
            throw new InvalidOperationException(
                $"The cast executable '{googleHomeOptions.CastExecutable}' was not found. Install catt or configure Music:GoogleHome:CastExecutable.",
                exception);
        }

        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            throw new InvalidOperationException($"Google Home cast command failed: {error}");
        }
    }

    private static bool IsSpotifyConfigured(SpotifyOptions spotifyOptions) =>
        !string.IsNullOrWhiteSpace(spotifyOptions.ClientId)
        && !string.IsNullOrWhiteSpace(spotifyOptions.ClientSecret)
        && !string.IsNullOrWhiteSpace(spotifyOptions.RefreshToken);

    private async Task<string> GetSpotifyAccessTokenAsync(
        SpotifyOptions spotifyOptions,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{spotifyOptions.ClientId}:{spotifyOptions.ClientSecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = spotifyOptions.RefreshToken,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var tokenResponse = await JsonSerializer.DeserializeAsync<SpotifyTokenResponse>(responseStream, JsonOptions, cancellationToken);

        return tokenResponse?.AccessToken ?? throw new InvalidOperationException("Spotify token response did not include an access token.");
    }

    private static IReadOnlyList<MusicSearchResult> MapSearchResults(SpotifySearchResponse? response)
    {
        if (response is null)
        {
            return [];
        }

        var results = new List<MusicSearchResult>();
        results.AddRange(response.Tracks?.Items?.Select(track => new MusicSearchResult(
            track.Id,
            track.Name,
            ArtistsText(track.Artists),
            track.Uri,
            "track",
            FirstImage(track.Album?.Images))) ?? []);
        results.AddRange(response.Albums?.Items?.Select(album => new MusicSearchResult(
            album.Id,
            album.Name,
            ArtistsText(album.Artists),
            album.Uri,
            "album",
            FirstImage(album.Images))) ?? []);
        results.AddRange(response.Artists?.Items?.Select(artist => new MusicSearchResult(
            artist.Id,
            artist.Name,
            "Artist",
            artist.Uri,
            "artist",
            FirstImage(artist.Images))) ?? []);
        results.AddRange(response.Playlists?.Items?.Where(playlist => playlist is not null).Select(playlist => new MusicSearchResult(
            playlist!.Id,
            playlist.Name,
            "Playlist",
            playlist.Uri,
            "playlist",
            FirstImage(playlist.Images))) ?? []);

        return results;
    }

    private static string ArtistsText(IReadOnlyList<SpotifyArtist>? artists) =>
        artists is { Count: > 0 }
            ? string.Join(", ", artists.Select(artist => artist.Name))
            : string.Empty;

    private static string? FirstImage(IReadOnlyList<SpotifyImage>? images) =>
        images?.FirstOrDefault()?.Url;

    private sealed record SpotifyTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken);

    private sealed record SpotifySearchResponse(
        SpotifyCollection<SpotifyTrack>? Tracks,
        SpotifyCollection<SpotifyAlbum>? Albums,
        SpotifyCollection<SpotifyArtist>? Artists,
        SpotifyCollection<SpotifyPlaylist?>? Playlists);

    private sealed record SpotifyCollection<T>(IReadOnlyList<T> Items);

    private sealed record SpotifyTrack(
        string Id,
        string Name,
        string Uri,
        IReadOnlyList<SpotifyArtist>? Artists,
        SpotifyAlbum? Album);

    private sealed record SpotifyAlbum(
        string Id,
        string Name,
        string Uri,
        IReadOnlyList<SpotifyArtist>? Artists,
        IReadOnlyList<SpotifyImage>? Images);

    private sealed record SpotifyArtist(
        string Id,
        string Name,
        string Uri,
        IReadOnlyList<SpotifyImage>? Images);

    private sealed record SpotifyPlaylist(
        string Id,
        string Name,
        string Uri,
        IReadOnlyList<SpotifyImage>? Images);

    private sealed record SpotifyImage(string Url);
}