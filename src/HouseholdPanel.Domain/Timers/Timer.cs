namespace HouseholdPanel.Domain.Timers;

public sealed class Timer
{
    public Timer(Guid id, string name, DateTimeOffset startedAt, TimeSpan duration)
    {
        Id = id;
        Name = name;
        StartedAt = startedAt;
        Duration = duration;
        EndsAt = startedAt.Add(duration);
    }

    public Guid Id { get; }

    public string Name { get; }

    public DateTimeOffset StartedAt { get; }

    public TimeSpan Duration { get; }

    public DateTimeOffset EndsAt { get; }
}