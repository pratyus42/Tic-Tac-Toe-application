namespace TicTacToe.Api.Contracts;

/// <summary>Request to place a mark on the board.</summary>
/// <param name="Player">The player making the move, X or O.</param>
/// <param name="Row">Zero-based board row from 0 through 2.</param>
/// <param name="Column">Zero-based board column from 0 through 2.</param>
public sealed record MakeMoveRequest(string Player, int Row, int Column);
