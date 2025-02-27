namespace ChessBot;

public static class ZobristTest
{
    static int nodesSearched = 0;

    public static void Test(int depth = 6)
    {
        Board board = new("3q1k2/pp1p1p2/6p1/2P4p/5P2/4P1P1/PP4QP/R3K3 w Q - 7 15");
        //Board board = new();
        ulong originalZobrist = board.ZobristHash;
        ZobristPerft(board, depth);
        if (board.ZobristHash != originalZobrist) Console.WriteLine("Zobrist changed during the perft.");
        else Console.WriteLine("Zobrist updated correctly.");

        Console.WriteLine("nodes searched: " + nodesSearched);

    }

    public static void ZobristPerft(Board board, int depth)
    {
        nodesSearched++;
        if (depth == 0) return;

        Move[] moves = board.GetLegalMoves();

        foreach (Move move in moves)
        {
            ulong prevZobrist = board.ZobristHash;
            board.MakeMove(move);
            if (prevZobrist == board.ZobristHash) Console.WriteLine("zobrist didnt change during perft.");
            ZobristPerft(board, depth-1);
            board.UnmakeMove();
            if (prevZobrist != board.ZobristHash) Console.WriteLine("zobrist didnt undo correctly.");
        }
    }


}