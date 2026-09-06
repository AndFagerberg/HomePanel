using HouseholdPanel.Application.Abstractions;

namespace HouseholdPanel.Application.Timers;

public sealed class TimerService(ITimerService timerService)
{
    public async Task<IReadOnlyList<TimerDto>> GetActiveAsync(CancellationToken cancellationToken)
    {
        var timers = await timerService.GetActiveAsync(cancellationToken);

        return timers.Select(ToDto).ToList();
    }

    public async Task<TimerDto> CreateAsync(
        CreateTimerRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Timer name is required.", nameof(request));
        }

        if (request.Duration <= TimeSpan.Zero || request.Duration > TimeSpan.FromDays(7))
        {
            throw new ArgumentException("Timer duration must be between one second and seven days.", nameof(request));
        }

        var timer = await timerService.CreateAsync(request.Name.Trim(), request.Duration, cancellationToken);

        return ToDto(timer);
    }

    private static TimerDto ToDto(Domain.Timers.Timer timer) => new(
        timer.Id,
        timer.Name,
        timer.StartedAt,
        timer.Duration,
        timer.EndsAt);
}