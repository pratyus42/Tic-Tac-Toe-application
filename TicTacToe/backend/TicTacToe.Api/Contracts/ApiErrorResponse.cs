namespace TicTacToe.Api.Contracts;

/// <summary>Consistent error returned for validation and game-operation failures.</summary>
public sealed record ApiErrorResponse(string Code, string Message);
