using TicTacToe.Api.Domain;
using TicTacToe.Api.Services;

namespace TicTacToe.Tests;

public sealed class ComputerMoveSelectorTests
{
    private readonly ComputerMoveSelector selector = new();
    [Fact] public void TakesWinningMoveBeforeCenter() { var board = new Player?[3,3]; board[0,0]=Player.O; board[0,1]=Player.O; Assert.Equal(new CellPosition(0,2), selector.Select(board)); }
    [Fact] public void BlocksImmediateXWin() { var board = new Player?[3,3]; board[1,0]=Player.X; board[1,1]=Player.X; Assert.Equal(new CellPosition(1,2), selector.Select(board)); }
    [Fact] public void PrefersCenterThenCornerThenRowMajor() { var board = new Player?[3,3]; Assert.Equal(new CellPosition(1,1), selector.Select(board)); board[1,1]=Player.X; Assert.Equal(new CellPosition(0,0), selector.Select(board)); board[0,0]=Player.X; board[0,2]=Player.X; board[2,0]=Player.X; board[2,2]=Player.X; Assert.Equal(new CellPosition(0,1), selector.Select(board)); }
}
