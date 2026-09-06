using HouseholdPanel.Application.Timers;
using Microsoft.AspNetCore.Mvc;

namespace HouseholdPanel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TimersController(TimerService timerService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TimerDto>>> Get(CancellationToken cancellationToken)
    {
        return Ok(await timerService.GetActiveAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<TimerDto>> Create(
        CreateTimerRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var timer = await timerService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = timer.Id }, timer);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var cancelled = await timerService.CancelAsync(id, cancellationToken);

        return cancelled ? NoContent() : NotFound();
    }
}