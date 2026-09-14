using HouseholdPanel.Application.Configuration;
using HouseholdPanel.Domain.AirPatrol;
using HouseholdPanel.Infrastructure.AirPatrol;
using Microsoft.Extensions.Options;

namespace HouseholdPanel.UnitTests.AirPatrol;

public sealed class SqliteAirPatrolHistoryRepositoryTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"homepanel-{Guid.NewGuid():N}");

    [Fact]
    public async Task SaveAsync_RemovesReadingsOlderThanSevenDays()
    {
        var now = DateTimeOffset.UtcNow;
        var repository = new SqliteAirPatrolHistoryRepository(
            Options.Create(new StorageOptions { DatabasePath = Path.Combine(_directory, "test.db") }),
            TimeProvider.System);

        await repository.SaveAsync(CreateStatus(now.AddDays(-8)), CancellationToken.None);
        await repository.SaveAsync(CreateStatus(now.AddDays(-1)), CancellationToken.None);

        var history = await repository.GetSinceAsync(now.AddDays(-10), CancellationToken.None);

        var reading = Assert.Single(history);
        Assert.Equal(now.AddDays(-1), reading.UpdatedAt);
        Assert.Equal(12.3m, reading.Temperature);
        Assert.Equal(52, reading.Humidity);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    private static AirPatrolStatus CreateStatus(DateTimeOffset updatedAt) =>
        new("Stugan", 12.3m, 52, true, "lowheat", 10m, "auto", true, updatedAt);
}