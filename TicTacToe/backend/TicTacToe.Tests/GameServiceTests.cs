using TicTacToe.Api.Contracts;
using TicTacToe.Api.Services;

namespace TicTacToe.Tests;

public sealed class GameServiceTests
{
    private static GameService Service() => new(new InMemoryGameStore(), new ComputerMoveSelector());

    [Fact] public void NewGameStartsWithEmptyBoardAndX() { var state = Service().Create("TwoPlayer"); Assert.Equal("X", state.CurrentPlayer); Assert.Equal("InProgress", state.Status); Assert.Empty(state.MoveHistory); }
    [Fact] public void ValidMoveSwitchesTurnAndHistory() { var service = Service(); var state = service.Create("TwoPlayer"); state = service.Move(state.GameId, new("X", 0, 0)); Assert.Equal("O", state.CurrentPlayer); Assert.Equal("X", state.Board[0][0]); Assert.Single(state.MoveHistory); }
    [Fact] public void OccupiedMoveDoesNotChangeState() { var service = Service(); var state = service.Create("TwoPlayer"); service.Move(state.GameId, new("X", 0, 0)); Assert.Throws<GameServiceException>(() => service.Move(state.GameId, new("O", 0, 0))); var current = service.Get(state.GameId); Assert.Single(current.MoveHistory); Assert.Equal("O", current.CurrentPlayer); }
    [Fact] public void RowWinUpdatesScoreOnceAndDisablesUndo() { var service = Service(); var state = service.Create("TwoPlayer"); foreach (var move in new[] { new MakeMoveRequest("X",0,0),new("O",1,0),new("X",0,1),new("O",1,1),new("X",0,2) }) state = service.Move(state.GameId, move); Assert.Equal("Won", state.Status); Assert.Equal(1, state.Scoreboard.XWins); Assert.False(state.CanUndo); Assert.Throws<GameServiceException>(() => service.Undo(state.GameId)); }
    [Fact] public void ComputerMoveAddsOAndUndoRemovesPair() { var service = Service(); var state = service.Create("Computer"); state = service.Move(state.GameId, new("X",0,0)); Assert.Equal(2, state.MoveHistory.Length); state = service.Undo(state.GameId); Assert.Empty(state.MoveHistory); Assert.Equal("X", state.CurrentPlayer); }
    [Fact] public void ResetPreservesScoreboard() { var service = Service(); var state = service.Create("TwoPlayer"); foreach (var move in new[] { new MakeMoveRequest("X",0,0),new("O",1,0),new("X",0,1),new("O",1,1),new("X",0,2) }) state = service.Move(state.GameId, move); state = service.Reset(state.GameId); Assert.Equal(1, state.Scoreboard.XWins); Assert.Empty(state.MoveHistory); }
}
