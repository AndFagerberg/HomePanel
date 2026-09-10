using HouseholdPanel.Application.Abstractions;

namespace HouseholdPanel.Application.Music;

public sealed class MusicService(IMusicService musicService)
{
    public async Task<IReadOnlyList<RadioStationDto>> GetRadioStationsAsync(CancellationToken cancellationToken)
    {
        var stations = await musicService.GetRadioStationsAsync(cancellationToken);

        return stations.Select(station => new RadioStationDto(station.Id, station.Name)).ToList();
    }

    public async Task<IReadOnlyList<MusicSearchResultDto>> SearchSpotifyAsync(
        string query,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
        {
            return [];
        }

        var results = await musicService.SearchSpotifyAsync(query.Trim(), cancellationToken);

        return results
            .Select(result => new MusicSearchResultDto(
                result.Id,
                result.Title,
                result.Subtitle,
                result.Uri,
                result.Type,
                result.ImageUrl))
            .ToList();
    }

    public Task PlaySpotifyAsync(PlaySpotifyRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Uri))
        {
            throw new ArgumentException("Spotify URI is required.", nameof(request));
        }

        return musicService.PlaySpotifyAsync(request.Uri.Trim(), cancellationToken);
    }

    public Task PlayRadioAsync(PlayRadioRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.StationId))
        {
            throw new ArgumentException("Radio station id is required.", nameof(request));
        }

        return musicService.PlayRadioAsync(request.StationId.Trim(), cancellationToken);
    }
}