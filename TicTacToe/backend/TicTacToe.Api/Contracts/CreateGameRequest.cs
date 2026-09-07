namespace TicTacToe.Api.Contracts;

/// <summary>Request to create a game.</summary>
/// <param name="Mode">Either TwoPlayer or Computer.</param>
public sealed record CreateGameRequest(string Mode);
