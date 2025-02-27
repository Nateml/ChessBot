namespace ChessBot;

public static class ZobristTest
{
    static int nodesSearched = 0;

    public static void Test(int depth = 6)
    {
        Board board = new("3q1k2/pp1p1p2/6p1/2P4p/5P2/4P1P1/PP4QP/R3K3 w Q - 7 15");
        // Board board = new("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
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
            String prevFen = FenBuilder.BuildFen(board);
            board.MakeMove(move);
            ulong fullRecompute = Zobrist.GetZobristHash(board);
            if (board.ZobristHash != fullRecompute)
            {
                Console.WriteLine("zobrist didnt update correctly (incremental update did not match the full recompute).");
                Console.WriteLine("prev zobrist: " + Convert.ToString((long)prevZobrist, 2).PadLeft(64, '0'));
                Console.WriteLine("full recompute: " + Convert.ToString((long)fullRecompute, 2).PadLeft(64, '0'));
                Console.WriteLine("move: " + move);
                Console.WriteLine("board: " + board);
                Console.WriteLine("depth: " + depth);
                System.Environment.Exit(1);
            };

            if (prevZobrist == board.ZobristHash) Console.WriteLine("zobrist didnt change during perft.");
            ZobristPerft(board, depth-1);
            board.UnmakeMove();
            if (prevZobrist != board.ZobristHash)
            {
                Console.WriteLine("zobrist didnt undo correctly.");
                BitboardUtility.PrintBitboard(board.GetBitboardByPieceType(PieceType.WR));
                //Console.WriteLine("prev zobrist: " + Convert.ToString((long)prevZobrist, 2).PadLeft(64, '0'));
                //Console.WriteLine("current zobrist: " + Convert.ToString((long)board.ZobristHash, 2).PadLeft(64, '0'));
                Console.WriteLine("prev fen: " + prevFen);
                Console.WriteLine("current fen: " + FenBuilder.BuildFen(board));
                Console.WriteLine("move: " + move);
                Console.WriteLine("board: " + board);
                System.Environment.Exit(1);
            }
        }
    }
}