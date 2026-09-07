using TicTacToe.Api.Domain;

namespace TicTacToe.Api.Contracts;

/// <summary>A move in chronological game history.</summary>
public sealed record MoveResponse(int MoveNumber, string Player, int Row, int Column);
/// <summary>Session totals for completed games.</summary>
public sealed record ScoreboardResponse(int XWins, int OWins, int Draws);
/// <summary>Complete backend-authoritative state used to render the game.</summary>
public sealed record GameStateResponse(
    Guid GameId, string?[][] Board, string CurrentPlayer, string Mode, string Status,
    string? Winner, CellPosition[] WinningCells, MoveResponse[] MoveHistory,
    ScoreboardResponse Scoreboard, bool CanUndo);
