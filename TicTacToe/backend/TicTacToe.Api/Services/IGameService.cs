using TicTacToe.Api.Contracts;

namespace TicTacToe.Api.Services;

public interface IGameService
{
    GameStateResponse Create(string mode);
    GameStateResponse Get(Guid id);
    GameStateResponse Move(Guid id, MakeMoveRequest request);
    GameStateResponse Undo(Guid id);
    GameStateResponse Reset(Guid id);
    ScoreboardResponse GetScoreboard();
    ScoreboardResponse ResetScoreboard();
}
public sealed class GameServiceException(string code, string message, int statusCode) : Exception(message)
{ public string Code { get; } = code; public int StatusCode { get; } = statusCode; }
