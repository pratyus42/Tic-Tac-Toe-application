using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Contracts;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController, Route("api/games")]
public sealed class GamesController(IGameService service) : ControllerBase
{
    /// <summary>Creates a new local Tic Tac Toe game.</summary>
    /// <remarks>Use TwoPlayer for two human players or Computer for a human X versus computer O.</remarks>
    /// <param name="request">The requested game mode.</param>
    /// <response code="201">The new game state.</response>
    /// <response code="400">The mode is missing or is not TwoPlayer or Computer.</response>
    [ProducesResponseType(typeof(GameStateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [HttpPost] public IActionResult Create(CreateGameRequest request) => Execute(() => Created("", service.Create(request.Mode)));

    /// <summary>Returns the complete authoritative state for a game.</summary>
    /// <param name="gameId">The game identifier returned when the game was created.</param>
    /// <response code="200">The current board, turn, history, status, and scoreboard.</response>
    /// <response code="404">No game exists for the supplied identifier.</response>
    [ProducesResponseType(typeof(GameStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [HttpGet("{gameId:guid}")] public IActionResult Get(Guid gameId) => Execute(() => Ok(service.Get(gameId)));

    /// <summary>Applies a player move and, in Computer mode, performs the computer response.</summary>
    /// <param name="gameId">The game identifier.</param>
    /// <param name="request">The player and zero-based row and column to play.</param>
    /// <response code="200">The updated authoritative game state.</response>
    /// <response code="400">The move is malformed, out of range, occupied, out of turn, or the game is complete.</response>
    /// <response code="404">No game exists for the supplied identifier.</response>
    [ProducesResponseType(typeof(GameStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [HttpPost("{gameId:guid}/moves")] public IActionResult Move(Guid gameId, MakeMoveRequest request) => Execute(() => Ok(service.Move(gameId, request)));

    /// <summary>Undoes the latest valid action according to the game mode.</summary>
    /// <param name="gameId">The game identifier.</param>
    /// <remarks>TwoPlayer removes one move. Computer removes the latest O move and preceding X move. Option A disables undo after a win or draw.</remarks>
    /// <response code="200">The restored authoritative game state.</response>
    /// <response code="404">No game exists for the supplied identifier.</response>
    /// <response code="409">No undo is available, including after game completion.</response>
    [ProducesResponseType(typeof(GameStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [HttpPost("{gameId:guid}/undo")] public IActionResult Undo(Guid gameId) => Execute(() => Ok(service.Undo(gameId)));

    /// <summary>Resets a game while preserving the session scoreboard.</summary>
    /// <param name="gameId">The game identifier.</param>
    /// <response code="200">A fresh in-progress game state with the same game identifier.</response>
    /// <response code="404">No game exists for the supplied identifier.</response>
    [ProducesResponseType(typeof(GameStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [HttpPost("{gameId:guid}/reset")] public IActionResult Reset(Guid gameId) => Execute(() => Ok(service.Reset(gameId)));
    private IActionResult Execute(Func<IActionResult> action) { try { return action(); } catch (GameServiceException error) { return StatusCode(error.StatusCode, new ApiErrorResponse(error.Code, error.Message)); } }
}
