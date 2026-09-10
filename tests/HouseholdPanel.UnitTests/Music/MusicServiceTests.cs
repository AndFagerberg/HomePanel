using HouseholdPanel.Application.Abstractions;
using HouseholdPanel.Application.Music;
using HouseholdPanel.Domain.Music;

namespace HouseholdPanel.UnitTests.Music;

public sealed class MusicServiceTests
{
    [Fact]
    public async Task SearchSpotifyAsync_ReturnsEmptyList_WhenQueryIsTooShort()
    {
        var fakeMusicService = new FakeMusicService();
        var sut = new MusicService(fakeMusicService);

        var results = await sut.SearchSpotifyAsync(" p ", CancellationToken.None);

        Assert.Empty(results);
        Assert.False(fakeMusicService.SearchWasCalled);
    }

    [Fact]
    public async Task PlaySpotifyAsync_TrimsUriBeforeCallingProvider()
    {
        var fakeMusicService = new FakeMusicService();
        var sut = new MusicService(fakeMusicService);

        await sut.PlaySpotifyAsync(new PlaySpotifyRequest(" spotify:track:123 "), CancellationToken.None);

        Assert.Equal("spotify:track:123", fakeMusicService.PlayedSpotifyUri);
    }

    [Fact]
    public async Task GetRadioStationsAsync_MapsStationsWithoutStreamUrls()
    {
        var fakeMusicService = new FakeMusicService();
        var sut = new MusicService(fakeMusicService);

        var stations = await sut.GetRadioStationsAsync(CancellationToken.None);

        var station = Assert.Single(stations);
        Assert.Equal("p3", station.Id);
        Assert.Equal("P3", station.Name);
    }

    private sealed class FakeMusicService : IMusicService
    {
        public bool SearchWasCalled { get; private set; }

        public string? PlayedSpotifyUri { get; private set; }

        public Task<IReadOnlyList<RadioStation>> GetRadioStationsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<RadioStation>>([
                new RadioStation("p3", "P3", "https://www.sverigesradio.se/topsy/direkt/srapi/164.mp3")
            ]);

        public Task<IReadOnlyList<MusicSearchResult>> SearchSpotifyAsync(string query, CancellationToken cancellationToken)
        {
            SearchWasCalled = true;
            return Task.FromResult<IReadOnlyList<MusicSearchResult>>([]);
        }

        public Task PlaySpotifyAsync(string uri, CancellationToken cancellationToken)
        {
            PlayedSpotifyUri = uri;
            return Task.CompletedTask;
        }

        public Task PlayRadioAsync(string stationId, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}