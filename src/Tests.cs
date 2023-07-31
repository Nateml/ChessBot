namespace ChessBot;

using System.Globalization;
using System.Linq.Expressions;

class Tests
{


    public static bool UndoMoveTest(bool verbose = false)
    {
        if (verbose) Console.WriteLine("Undo move test:");
        Board referenceBoard = new();
        Board testBoard = new();

        Move[] legalMoves = testBoard.GetLegalMoves();
        foreach (Move move in legalMoves)
        {
            testBoard.MakeMove(move);
            testBoard.UnmakeMove();

            for (int i = 0; i < testBoard.Bitboards.Length; i++)
            {
                if (testBoard.Bitboards[i] != referenceBoard.Bitboards[i])
                {
                    if (verbose) Console.WriteLine("\tBitboards of piece type " + ((PieceType)i) + " do not match");
                    return false;
                }
            }
            if (verbose) Console.WriteLine("\tAll bitboards match.");

            if (testBoard.EpFile != referenceBoard.EpFile)
            {
                if (verbose) Console.WriteLine("EP files do not match.");
                return false;
            }
            else if (verbose) Console.WriteLine("EP files match.");

            if (testBoard.IsWhiteToMove != referenceBoard.IsWhiteToMove)
            {
                if (verbose) Console.WriteLine("IsWhiteToMove does not match");
                return false;
            }
            else
            {
                if (verbose) Console.WriteLine("IsWhiteToMove matches");
            }

        }
        return true;
    }


    public static bool MoveGenTest()
    {
        Board board = new("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        board.AttachListener(board.moveGen);
        int nodes = Perft.DividePerftTest(board, 5, false);
        if (nodes != 4865609) 
        {
            Console.WriteLine("Failed perft from initial position, depth 5.");
            Console.WriteLine("Starting fen: rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1" );
            Console.WriteLine("Expected to reach 4865609 nodes but instead reached " + nodes + ".");
            return false;
        } 
        Console.WriteLine("Passed initial position at depth 5 succesfully.");

        board = new("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1");
        board.AttachListener(board.moveGen);
        nodes = Perft.DividePerftTest(board, 4, false);
        if (nodes != 4085603)
        {
            Console.WriteLine("Failed perft from position 2, depth 4");
            Console.WriteLine("Starting fen: r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1");
            Console.WriteLine("Expected to reach 4085603 nodes but instead reached " + nodes + ".");
            return false;
        }
        Console.WriteLine("Passed position 2 at depth 5 succesfully.");

        board = new("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1");
        board.AttachListener(board.moveGen);
        nodes = Perft.DividePerftTest(board, 5, false);
        if (nodes != 674624)
        {
            Console.WriteLine("Failed perft from position 3, depth 5");
            Console.WriteLine("Starting fen: 8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1");
            Console.WriteLine("Expected to reach 674624 nodes but instead reached " + nodes + ".");
            return false;
        }
        Console.WriteLine("Passed position 3 at depth 5 succesfully.");

        board = new("r2q1rk1/pP1p2pp/Q4n2/bbp1p3/Np6/1B3NBn/pPPP1PPP/R3K2R b KQ - 0 1");
        board.AttachListener(board.moveGen);
        nodes = Perft.DividePerftTest(board, 5, false);
        if (nodes != 15833292)
        {
            Console.WriteLine("Failed perft from position 4, depth 5");
            Console.WriteLine("Starting fen: r2q1rk1/pP1p2pp/Q4n2/bbp1p3/Np6/1B3NBn/pPPP1PPP/R3K2R b KQ - 0 1");
            Console.WriteLine("Expected to reach 15833292 nodes but instead reached " + nodes + ".");
            return false;
        }
        Console.WriteLine("Passed position 4 at depth 5 succesfully.");

        board = new("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8");
        board.AttachListener(board.moveGen);
        nodes = Perft.DividePerftTest(board, 3, false);
        if (nodes != 62379)
        {
            Console.WriteLine("Failed perft from position 5, depth 3");
            Console.WriteLine("Starting fen: rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8");
            Console.WriteLine("Expected to reach 62379 nodes but instead reached " + nodes + ".");
            return false;
        }
        Console.WriteLine("Passed position 5 at depth 3 succesfully.");

        Console.WriteLine("Passed ALL POSITIONS succesfully.");

        return true;
    }

}