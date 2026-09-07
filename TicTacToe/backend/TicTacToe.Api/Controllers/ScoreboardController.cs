using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Contracts;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController, Route("api/scoreboard")]
public sealed class ScoreboardController(IGameService service) : ControllerBase
{
    /// <summary>Returns the session scoreboard.</summary>
    /// <response code="200">The current X wins, O wins, and draw totals.</response>
    [ProducesResponseType(typeof(ScoreboardResponse), StatusCodes.Status200OK)]
    [HttpGet] public IActionResult Get() => Execute(() => Ok(service.GetScoreboard()));

    /// <summary>Resets all scoreboard totals without changing any active game.</summary>
    /// <response code="200">A zeroed scoreboard.</response>
    [ProducesResponseType(typeof(ScoreboardResponse), StatusCodes.Status200OK)]
    [HttpPost("reset")] public IActionResult Reset() => Execute(() => Ok(service.ResetScoreboard()));
    private IActionResult Execute(Func<IActionResult> action) { try { return action(); } catch (GameServiceException error) { return StatusCode(error.StatusCode, new ApiErrorResponse(error.Code, error.Message)); } }
}
