using TicTacToe.Api.Domain;

namespace TicTacToe.Api.Services;

public interface IComputerMoveSelector { CellPosition? Select(Player?[,] board); }
