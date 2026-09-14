using HouseholdPanel.Application.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HouseholdPanel.Infrastructure.AirPatrol;

public sealed class AirPatrolPollingService(
    IAirPatrolService airPatrolService,
    TimeProvider timeProvider,
    ILogger<AirPatrolPollingService> logger) : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await PollAsync(stoppingToken);
        using var timer = new PeriodicTimer(PollingInterval, timeProvider);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await PollAsync(stoppingToken);
        }
    }

    private async Task PollAsync(CancellationToken cancellationToken)
    {
        try
        {
            await airPatrolService.GetStatusAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not update AirPatrol status.");
        }
    }
}