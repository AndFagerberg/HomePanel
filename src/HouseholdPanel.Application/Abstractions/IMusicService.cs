using HouseholdPanel.Domain.Music;

namespace HouseholdPanel.Application.Abstractions;

public interface IMusicService
{
    Task<IReadOnlyList<RadioStation>> GetRadioStationsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<MusicSearchResult>> SearchSpotifyAsync(string query, CancellationToken cancellationToken);

    Task PlaySpotifyAsync(string uri, CancellationToken cancellationToken);

    Task PlayRadioAsync(string stationId, CancellationToken cancellationToken);
}