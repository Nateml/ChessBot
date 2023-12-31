namespace ChessBot;

using System.ComponentModel;
using System.Net.NetworkInformation;

static class Perft
{

    static int nodes = 0;

    public struct PerftResult
    {
        public int moves;
        public int captures;
        public int checks;

        public PerftResult(int moves, int captures, int checks)
        {
            this.moves = moves;
            this.captures = captures;
            this.checks = checks;
        }

        public void Add(PerftResult perftResult)
        {
            moves += perftResult.moves;
            captures += perftResult.captures;
            checks += perftResult.checks;
        }
    }

    static int captures = 0;
    static int enPassants = 0;

    public static int DividePerftTest(Board board, int depth, bool verbose = true, bool capturesOnly = false)
    {
        System.Diagnostics.Stopwatch stopwatch = new();
        stopwatch.Start();

        int nodes = 0;

        Move[] legalMoves = board.GetLegalMoves(capturesOnly);

        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);
            int perft = SimplePerftTest(board, depth-1);
            nodes += perft;
            if (verbose) Console.WriteLine(move.ToString().ToLower() + ": " + perft);
            board.UnmakeMove();
        }

        stopwatch.Stop();
        Console.WriteLine("That took: " + stopwatch.Elapsed.TotalMilliseconds);

        return nodes;
    }

    public static int SimplePerftTest(Board board, int depth, bool capturesOnly = false)
    {
        if ( depth == 0 ) return 1;

        int nodes = 0;

        Move[] legalMoves = board.GetLegalMoves(capturesOnly);
        if (legalMoves.Length == 0)
        {
            return 0;
        }

        foreach (Move move in legalMoves)
        {
            if (move.IsCapture()) captures++;
            if (move.IsEnPassant()) enPassants++;
            board.MakeMove(move);
            nodes += SimplePerftTest(board, depth-1);
            board.UnmakeMove();
        }

        return nodes;
    }

    public static PerftResult PerftTest(Board board, int depth)
    {
        PerftResult perftResult = new(0, 0, 0);

        if ( depth == 0 ) return perftResult;

        Move[] legalMoves = board.GetLegalMoves();
        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);
            if (move.IsCapture()) perftResult.captures++;
            if (board.IsKingInCheck(!board.IsWhiteToMove)) perftResult.checks++;
            perftResult.moves++;
            perftResult.Add(PerftTest(board, depth-1));
            board.UnmakeMove();
            nodes++;
        }

        return perftResult;
    }
}