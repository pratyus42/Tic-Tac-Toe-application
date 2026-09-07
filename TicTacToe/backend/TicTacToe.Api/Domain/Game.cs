namespace TicTacToe.Api.Domain;

public sealed class Game
{
    public Guid GameId { get; init; }
    public Player?[,] Board { get; } = new Player?[3, 3];
    public Player CurrentPlayer { get; set; } = Player.X;
    public GameMode Mode { get; init; }
    public GameStatus Status { get; set; } = GameStatus.InProgress;
    public Player? Winner { get; set; }
    public List<CellPosition> WinningCells { get; } = [];
    public List<Move> MoveHistory { get; } = [];
    public bool CompletionCounted { get; set; }
}
