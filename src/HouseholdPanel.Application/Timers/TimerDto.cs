namespace HouseholdPanel.Application.Timers;

public sealed record TimerDto(
    Guid Id,
    string Name,
    DateTimeOffset StartedAt,
    TimeSpan Duration,
    DateTimeOffset EndsAt);

public sealed record CreateTimerRequest(string Name, TimeSpan Duration);