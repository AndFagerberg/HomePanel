using System.Collections.Concurrent;
using HouseholdPanel.Application.Abstractions;
using HouseholdPanel.Domain.Timers;
using TimerEntity = HouseholdPanel.Domain.Timers.Timer;

namespace HouseholdPanel.Infrastructure.Timers;

public sealed class InMemoryTimerService(TimeProvider timeProvider) : ITimerService
{
    private readonly ConcurrentDictionary<Guid, TimerEntity> timers = new();

    public Task<IReadOnlyList<TimerEntity>> GetActiveAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var now = timeProvider.GetUtcNow();
        var activeTimers = timers.Values
            .Where(timer => timer.EndsAt > now)
            .OrderBy(timer => timer.EndsAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<TimerEntity>>(activeTimers);
    }

    public Task<TimerEntity> CreateAsync(string name, TimeSpan duration, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var timer = new TimerEntity(Guid.NewGuid(), name, timeProvider.GetUtcNow(), duration);
        timers[timer.Id] = timer;

        return Task.FromResult(timer);
    }
}