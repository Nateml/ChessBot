using System.Diagnostics;

namespace ChessBot;

public class Game
{

    private Board board;
    private IChessBot player1;
    private IChessBot player2;
    private string startingFen;
    public const string FenStartingPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public Game(IChessBot player1, IChessBot player2, string? startingFen = null)
    {
        this.player1 = player1;
        this.player2 = player2;
        startingFen ??= FenStartingPosition;
        this.startingFen = startingFen;
        board = new(this.startingFen);
    }

    /// <summary>
    /// Play a game between two bots.
    /// The time is in milliseconds.
    /// </summary>
    /// <param name="whiteTime">Time available for white per move (in milliseconds)</param>
    /// <param name="blackTime">Time available for black per move (in milliseconds)</param>
    public void Play(int whiteTime, int blackTime)
    {
        Console.WriteLine("Starting match in position: " + startingFen);
        Console.WriteLine("White Time per move: " + whiteTime);
        Console.WriteLine("Black Time per move: " + blackTime);
        Console.WriteLine("Moves:");

        bool isGameOver = IsGameOver(board);
        while (!isGameOver)
        {
            Move move;
            if (board.IsWhiteToMove)
            {
                move = player1.GetBestMove(board, whiteTime, new CancellationToken(), fixedTime: true, printToConsole: false);
                Console.WriteLine($"White: {move}");
            }
            else
            {
                move = player2.GetBestMove(board, blackTime, new CancellationToken(), fixedTime: true, printToConsole: false);
                Console.WriteLine($"Black: {move}");
            }
            board.MakeMove(move);
            isGameOver = IsGameOver(board);
        }

        Console.WriteLine("Game Over");

        // Determine who won
        if (board.IsKingInCheck(board.IsWhiteToMove))
        {
            Console.WriteLine(board.IsWhiteToMove ? "Black wins" : "White wins");
        }
        else
        {
            Console.WriteLine("Draw");
        }
    }

    public static bool IsGameOver(Board board)
    {
        // Checkmate, stalemate, 50-move rule, or 3-fold repetition
        return (board.GetLegalMoves().Length == 0) || board.NumPlySincePawnMoveOrCapture >= 100 || CountRepetitions(board) >= 3;
    }

    /// <summary>
    /// Used to check for 3-fold repetition.
    /// Taken from https://groups.google.com/g/rec.games.chess.computer/c/ft82tUpHJn0/m/FJNPi4KWjRYJ
    /// Implemented a bit differently from the way it is in the bot classes.
    /// </summary>
    public static int CountRepetitions(Board board)
    {
        int count = 0;

        int localNumPlySincePawnMoveOrCapture = board.NumPlySincePawnMoveOrCapture;
        

        // Iterate backwards through the history
        LinkedListNode<ulong>? node = board.History.Last!;
        while (localNumPlySincePawnMoveOrCapture >= 0 && node != null)
        {
            if (node.Value == board.ZobristHash)
            {
                count++;
            }
            if (count == 3) return count;

            // I have to iterate two nodes at a time because 
            // I want to always look at it from the perspective of the player
            // who made the last move
            node = node.Previous;
            if (node == null) break;
            node = node.Previous;

            localNumPlySincePawnMoveOrCapture -= 2;
        }
        return count;
    }

    public IChessBot White => player1;
    public IChessBot Black => player2;
}