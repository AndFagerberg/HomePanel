using System.Net;
using System.Text;
using HouseholdPanel.Application.Abstractions;
using HouseholdPanel.Application.Configuration;
using HouseholdPanel.Domain.AirPatrol;
using HouseholdPanel.Infrastructure.AirPatrol;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace HouseholdPanel.UnitTests.AirPatrol;

public sealed class AirPatrolServiceTests
{
    [Fact]
    public async Task GetStatusAsync_MapsResponseAndCachesItForSubsequentReads()
    {
        var handler = new AirPatrolHttpMessageHandler();
        var historyRepository = new FakeHistoryRepository();
        using var httpClient = new HttpClient(handler);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new AirPatrolService(
            httpClient,
            cache,
            historyRepository,
            Options.Create(new AirPatrolOptions
            {
                Email = "user@example.com",
                Password = "password",
                PairingId = "44223",
                Name = "Stugan",
            }),
            TimeProvider.System);

        var first = await sut.GetStatusAsync(CancellationToken.None);
        var second = await sut.GetStatusAsync(CancellationToken.None);

        Assert.NotNull(first);
        Assert.Same(first, second);
        Assert.Equal(22.09m, first.Temperature);
        Assert.Equal(54, first.Humidity);
        Assert.Equal("lowheat", first.Mode);
        Assert.Equal(10m, first.TargetTemperature);
        Assert.True(first.Power);
        Assert.Equal(2, handler.RequestCount);
        Assert.Single(historyRepository.Statuses);
    }

    private sealed class FakeHistoryRepository : IAirPatrolHistoryRepository
    {
        public List<AirPatrolStatus> Statuses { get; } = [];

        public Task SaveAsync(AirPatrolStatus status, CancellationToken cancellationToken)
        {
            Statuses.Add(status);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<AirPatrolStatus>> GetSinceAsync(
            DateTimeOffset since,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<AirPatrolStatus>>(Statuses.Where(status => status.UpdatedAt >= since).ToList());
    }

    private sealed class AirPatrolHttpMessageHandler : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            var json = request.RequestUri?.AbsolutePath.EndsWith("/login", StringComparison.Ordinal) == true
                ? """{"entities":{"users":{"list":[{"id":"user-1"}]}},"misc":{"accessToken":"token"}}"""
                : """{"ApiVersion":"12","CommandMode":"parameters","ParametersData":{"PumpPower":"on","PumpTemp":"10.000","PumpMode":"lowheat","FanSpeed":"auto","Swing":"on"},"RoomTemp":"22.090","RoomHumidity":"54"}""";

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            });
        }
    }
}