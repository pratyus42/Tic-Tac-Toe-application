using TicTacToe.Api.Domain;

namespace TicTacToe.Api.Services;

public sealed class ComputerMoveSelector : IComputerMoveSelector
{
    private static readonly CellPosition[] Lines = [new(0,0), new(0,1), new(0,2), new(1,0), new(1,1), new(1,2), new(2,0), new(2,1), new(2,2)];
    private static readonly CellPosition[] Corners = [new(0,0), new(0,2), new(2,0), new(2,2)];

    public CellPosition? Select(Player?[,] board)
    {
        foreach (var position in Lines.Where(p => board[p.Row, p.Column] is null))
            if (Wins(board, position, Player.O)) return position;
        foreach (var position in Lines.Where(p => board[p.Row, p.Column] is null))
            if (Wins(board, position, Player.X)) return position;
        if (board[1, 1] is null) return new(1, 1);
        foreach (var position in Corners) if (board[position.Row, position.Column] is null) return position;
        return Lines.FirstOrDefault(p => board[p.Row, p.Column] is null);
    }

    private static bool Wins(Player?[,] board, CellPosition position, Player player)
    {
        board[position.Row, position.Column] = player;
        var result = Enumerable.Range(0, 3).Any(row => Enumerable.Range(0, 3).All(column => board[row, column] == player))
            || Enumerable.Range(0, 3).Any(column => Enumerable.Range(0, 3).All(row => board[row, column] == player))
            || board[0,0] == player && board[1,1] == player && board[2,2] == player
            || board[0,2] == player && board[1,1] == player && board[2,0] == player;
        board[position.Row, position.Column] = null;
        return result;
    }
}
