using HouseholdPanel.Application.Music;
using Microsoft.AspNetCore.Mvc;

namespace HouseholdPanel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class MusicController(MusicService musicService) : ControllerBase
{
    [HttpGet("radio-stations")]
    public async Task<ActionResult<IReadOnlyList<RadioStationDto>>> GetRadioStations(CancellationToken cancellationToken)
    {
        return Ok(await musicService.GetRadioStationsAsync(cancellationToken));
    }

    [HttpGet("spotify/search")]
    public async Task<ActionResult<IReadOnlyList<MusicSearchResultDto>>> SearchSpotify(
        [FromQuery] string query,
        CancellationToken cancellationToken)
    {
        return Ok(await musicService.SearchSpotifyAsync(query, cancellationToken));
    }

    [HttpPost("spotify/play")]
    public async Task<IActionResult> PlaySpotify(
        PlaySpotifyRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await musicService.PlaySpotifyAsync(request, cancellationToken);
            return Accepted();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpGet("spotify/current")]
    public async Task<ActionResult<MusicPlaybackDto?>> GetCurrentPlayback(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await musicService.GetCurrentPlaybackAsync(cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPost("stop")]
    public async Task<IActionResult> StopPlayback(CancellationToken cancellationToken)
    {
        try
        {
            await musicService.StopPlaybackAsync(cancellationToken);
            return Accepted();
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPost("radio/play")]
    public async Task<IActionResult> PlayRadio(
        PlayRadioRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await musicService.PlayRadioAsync(request, cancellationToken);
            return Accepted();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }
}