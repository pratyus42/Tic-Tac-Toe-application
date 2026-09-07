namespace TicTacToe.Api.Domain;

public enum Player { X, O }
public enum GameMode { TwoPlayer, Computer }
public enum GameStatus { InProgress, Won, Draw }
public readonly record struct CellPosition(int Row, int Column);
