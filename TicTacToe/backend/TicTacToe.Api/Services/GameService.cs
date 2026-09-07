using TicTacToe.Api.Contracts;
using TicTacToe.Api.Domain;

namespace TicTacToe.Api.Services;

public sealed class GameService(InMemoryGameStore store, IComputerMoveSelector computer) : IGameService
{
    private static readonly CellPosition[][] WinningLines = [
        [new(0,0),new(0,1),new(0,2)], [new(1,0),new(1,1),new(1,2)], [new(2,0),new(2,1),new(2,2)],
        [new(0,0),new(1,0),new(2,0)], [new(0,1),new(1,1),new(2,1)], [new(0,2),new(1,2),new(2,2)],
        [new(0,0),new(1,1),new(2,2)], [new(0,2),new(1,1),new(2,0)]];

    public GameStateResponse Create(string mode)
    {
        if (!Enum.TryParse<GameMode>(mode, true, out var gameMode)) throw Error("InvalidMode", "Mode must be TwoPlayer or Computer.", 400);
        lock (store.Sync) { var game = new Game { GameId = Guid.NewGuid(), Mode = gameMode }; store.Save(game); return State(game); }
    }
    public GameStateResponse Get(Guid id) { lock (store.Sync) return State(Find(id)); }
    public GameStateResponse Move(Guid id, MakeMoveRequest request)
    {
        lock (store.Sync)
        {
            var game = Find(id);
            if (!Enum.TryParse<Player>(request.Player, true, out var player)) throw Error("InvalidMove", "Player must be X or O.", 400);
            Apply(game, player, request.Row, request.Column);
            if (game.Status == GameStatus.InProgress && game.Mode == GameMode.Computer && game.CurrentPlayer == Player.O)
            { var position = computer.Select(game.Board); if (position is not null) Apply(game, Player.O, position.Value.Row, position.Value.Column); }
            return State(game);
        }
    }
    public GameStateResponse Undo(Guid id)
    {
        lock (store.Sync)
        {
            var game = Find(id);
            if (game.Status != GameStatus.InProgress || game.MoveHistory.Count == 0) throw Error("UndoUnavailable", "There is no undoable move.", 409);
            var count = game.Mode == GameMode.Computer && game.MoveHistory[^1].Player == Player.O ? 2 : 1;
            game.MoveHistory.RemoveRange(Math.Max(0, game.MoveHistory.Count - count), count);
            Rebuild(game); return State(game);
        }
    }
    public GameStateResponse Reset(Guid id)
    { lock (store.Sync) { Find(id); var game = new Game { GameId = id, Mode = Find(id).Mode }; store.Save(game); return State(game); } }
    public ScoreboardResponse GetScoreboard() { lock (store.Sync) return BoardScore(store.GetScoreboard()); }
    public ScoreboardResponse ResetScoreboard() { lock (store.Sync) return BoardScore(store.ResetScoreboard()); }

    private void Apply(Game game, Player player, int row, int column)
    {
        if (game.Status != GameStatus.InProgress) throw Error("InvalidMove", "The game is complete.", 400);
        if (row is < 0 or > 2 || column is < 0 or > 2) throw Error("InvalidMove", "Cell position is out of range.", 400);
        if (player != game.CurrentPlayer) throw Error("InvalidMove", "It is not this player's turn.", 400);
        if (game.Board[row, column] is not null) throw Error("InvalidMove", "That cell is occupied.", 400);
        game.Board[row, column] = player; game.MoveHistory.Add(new(game.MoveHistory.Count + 1, player, row, column)); Recalculate(game);
    }
    private void Recalculate(Game game)
    {
        game.WinningCells.Clear(); game.Winner = null; game.Status = GameStatus.InProgress;
        foreach (var line in WinningLines) if (game.Board[line[0].Row,line[0].Column] is { } mark && line.All(p => game.Board[p.Row,p.Column] == mark)) { game.Status = GameStatus.Won; game.Winner = mark; game.WinningCells.AddRange(line); break; }
        if (game.Status == GameStatus.InProgress && game.MoveHistory.Count == 9) game.Status = GameStatus.Draw;
        if (game.Status != GameStatus.InProgress) { if (!game.CompletionCounted) { if (game.Winner == Player.X) store.GetScoreboard().XWins++; else if (game.Winner == Player.O) store.GetScoreboard().OWins++; else store.GetScoreboard().Draws++; game.CompletionCounted = true; } }
        else game.CurrentPlayer = game.MoveHistory.Count % 2 == 0 ? Player.X : Player.O;
    }
    private static void Rebuild(Game game) { for (var row=0; row<3; row++) for (var column=0; column<3; column++) game.Board[row,column] = null; game.CurrentPlayer = Player.X; game.Status = GameStatus.InProgress; game.Winner = null; game.WinningCells.Clear(); foreach (var move in game.MoveHistory) game.Board[move.Row,move.Column] = move.Player; game.CurrentPlayer = game.MoveHistory.Count % 2 == 0 ? Player.X : Player.O; }
    private Game Find(Guid id) => store.TryGet(id, out var game) && game is not null ? game : throw Error("GameNotFound", "Game was not found.", 404);
    private GameStateResponse State(Game game) => new(game.GameId, Enumerable.Range(0,3).Select(row => Enumerable.Range(0,3).Select(column => game.Board[row,column]?.ToString()).ToArray()).ToArray(), game.CurrentPlayer.ToString(), game.Mode.ToString(), game.Status.ToString(), game.Winner?.ToString(), game.WinningCells.ToArray(), game.MoveHistory.Select(m => new MoveResponse(m.MoveNumber,m.Player.ToString(),m.Row,m.Column)).ToArray(), BoardScore(store.GetScoreboard()), game.Status == GameStatus.InProgress && game.MoveHistory.Count > 0);
    private static ScoreboardResponse BoardScore(Scoreboard score) => new(score.XWins, score.OWins, score.Draws);
    private static GameServiceException Error(string code, string message, int status) => new(code, message, status);
}
