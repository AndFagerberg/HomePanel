using HouseholdPanel.Domain.Timers;
using TimerEntity = HouseholdPanel.Domain.Timers.Timer;

namespace HouseholdPanel.Application.Abstractions;

public interface ITimerService
{
    Task<IReadOnlyList<TimerEntity>> GetActiveAsync(CancellationToken cancellationToken);

    Task<TimerEntity> CreateAsync(string name, TimeSpan duration, CancellationToken cancellationToken);
}