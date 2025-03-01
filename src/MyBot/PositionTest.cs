namespace ChessBot;

public class PositionTest
{
    public static void Test(IChessBot bot)
    {
        // Test positions taken from https://www.chessprogramming.org/Eigenmann_Rapid_Engine_Test
        string position = "r1bqk1r1/1p1p1n2/p1n2pN1/2p1b2Q/2P1Pp2/1PN5/PB4PP/R4RK1 w q - bm f1f4;" + 
                            "r1n2N1k/2n2K1p/3pp3/5Pp1/b5R1/8/1PPP4/8 w - - bm f8g6;" +
                            "r1b1r1k1/1pqn1pbp/p2pp1p1/P7/1n1NPP1Q/2NBBR2/1PP3PP/R6K w - - bm f4f5;" +
                            "5b2/p2k1p2/P3pP1p/n2pP1p1/1p1P2P1/1P1KBN2/7P/8 w - - bm f3g5;" +
                            "r3kbnr/1b3ppp/pqn5/1pp1P3/3p4/1BN2N2/PP2QPPP/R1BR2K1 w kq - bm b3f7;" +
                            "r2r2k1/1p1n1pp1/4pnp1/8/PpBRqP2/1Q2B1P1/1P5P/R5K1 b - - bm d7c5;" +
                            "2rq1rk1/pb1n1ppN/4p3/1pb5/3P1Pn1/P1N5/1PQ1B1PP/R1B2RK1 b - - bm d7e5;" +
                            "r2qk2r/ppp1bppp/2n5/3p1b2/3P1Bn1/1QN1P3/PP3P1P/R3KBNR w KQkq - bm b3d5;" +
                            "rnb1kb1r/p4p2/1qp1pn2/1p2N2p/2p1P1p1/2N3B1/PPQ1BPPP/3RK2R w Kkq - bm e5g6;" +
                            "5rk1/pp1b4/4pqp1/2Ppb2p/1P2p3/4Q2P/P3BPP1/1R3R1K b - - bm d5d4;" +
                            "r1b2r1k/ppp2ppp/8/4p3/2BPQ3/P3P1K1/1B3PPP/n3q1NR w - - bm d4e5, g1f3;" +
                            "1nkr1b1r/5p2/1q2p2p/1ppbP1p1/2pP4/2N3B1/1P1QBPPP/R4RK1 w - - bm c3d5;" +
                            "1nrq1rk1/p4pp1/bp2pn1p/3p4/2PP1B2/P1PB2N1/4QPPP/1R2R1K1 w - - bm e2d2, d3c2;" +
                            "5k2/1rn2p2/3pb1p1/7p/p3PP2/PnNBK2P/3N2P1/1R6 w - - bm d2f3;" +
                            "8/p2p4/r7/1k6/8/pK5Q/P7/b7 w - - bm h3d3;" +
                            "1b1rr1k1/pp1q1pp1/8/NP1p1b1p/1B1Pp1n1/PQR1P1P1/4BP1P/5RK1 w - - bm a5c6;" +
                            "1r3rk1/6p1/p1pb1qPp/3p4/4nPR1/2N4Q/PPP4P/2K1BR2 b - - bm b8b2;" +
                            "r1b1kb1r/1p1n1p2/p3pP1p/q7/3N3p/2N5/P1PQB1PP/1R3R1K b kq - bm a5g5;" +
                            "3kB3/5K2/7p/3p4/3pn3/4NN2/8/1b4B1 w - - bm e3f5;" +
                            "1nrrb1k1/1qn1bppp/pp2p3/3pP3/N2P3P/1P1B1NP1/PBR1QPK1/2R5 w - - bm d3h7;";
                            /*
                            "3rr1k1/1pq2b1p/2pp2p1/4bp2/pPPN4/4P1PP/P1QR1PB1/1R4K1 b - - bm Rc8;" +
                            "r4rk1/p2nbpp1/2p2np1/q7/Np1PPB2/8/PPQ1N1PP/1K1R3R w - - bm h4;" +
                            "r3r2k/1bq1nppp/p2b4/1pn1p2P/2p1P1QN/2P1N1P1/PPBB1P1R/2KR4 w - - bm Ng6;" +
                            "r2q1r1k/3bppbp/pp1p4/2pPn1Bp/P1P1P2P/2N2P2/1P1Q2P1/R3KB1R w KQ - am b3;" +
                            "2kb4/p7/r1p3p1/p1P2pBp/R2P3P/2K3P1/5P2/8 w - - bm Bxd8;" +
                            "rqn2rk1/pp2b2p/2n2pp1/1N2p3/5P1N/1PP1B3/4Q1PP/R4RK1 w - - bm Nxg6;" +
                            "8/3Pk1p1/1p2P1K1/1P1Bb3/7p/7P/6P1/8 w - - bm g4;" +
                            "4rrk1/Rpp3pp/6q1/2PPn3/4p3/2N5/1P2QPPP/5RK1 w - - am Rxb7;" +
                            "2q2rk1/2p2pb1/PpP1p1pp/2n5/5B1P/3Q2P1/4PPN1/2R3K1 w - - bm Rxc5;" +
                            "rnbq1r1k/4p1bP/p3p3/1pn5/8/2Np1N2/PPQ2PP1/R1B1KB1R w KQ - bm Nh4;" +
                            "4b1k1/1p3p2/4pPp1/p2pP1P1/P2P4/1P1B4/8/2K5 w - - bm b4;" +
                            "8/7p/5P1k/1p5P/5p2/2p1p3/P1P1P1P1/1K3Nb1 w - - bm Ng3;" +
                            "r3kb1r/ppnq2pp/2n5/4pp2/1P1PN3/P4N2/4QPPP/R1B1K2R w KQkq - bm Nxe5;" +
                            "b4r1k/6bp/3q1ppN/1p2p3/3nP1Q1/3BB2P/1P3PP1/2R3K1 w - - bm Rc8;" +
                            "r3k2r/5ppp/3pbb2/qp1Np3/2BnP3/N7/PP1Q1PPP/R3K2R w KQkq - bm Nxb5;" +
                            "r1k1n2n/8/pP6/5R2/8/1b1B4/4N3/1K5N w - - bm b7;" +
                            "1k6/bPN2pp1/Pp2p3/p1p5/2pn4/3P4/PPR5/1K6 w - - bm Na8;" +
                            "8/6N1/3kNKp1/3p4/4P3/p7/P6b/8 w - - bm exd5;" +
                            "r1b1k2r/pp3ppp/1qn1p3/2bn4/8/6P1/PPN1PPBP/RNBQ1RK1 w kq - bm a3;" +
                            "r3kb1r/3n1ppp/p3p3/1p1pP2P/P3PBP1/4P3/1q2B3/R2Q1K1R b kq - bm Bc5;" +
                            "3q1rk1/2nbppb1/pr1p1n1p/2pP1Pp1/2P1P2Q/2N2N2/1P2B1PP/R1B2RK1 w - - bm Nxg5;" +
                            "8/2k5/N3p1p1/2KpP1P1/b2P4/8/8/8 b - - bm Kb7;" +
                            "2r1rbk1/1pqb1p1p/p2p1np1/P4p2/3NP1P1/2NP1R1Q/1P5P/R5BK w - - bm Nxf5;" +
                            "rnb2rk1/pp2q2p/3p4/2pP2p1/2P1Pp2/2N5/PP1QBRPP/R5K1 w - - bm h4;" +
                            "5rk1/p1p1rpb1/q1Pp2p1/3Pp2p/4Pn2/1R4N1/P1BQ1PPP/R5K1 w - - bm Rb4;" +
                            "8/4nk2/1p3p2/1r1p2pp/1P1R1N1P/6P1/3KPP2/8 w - - bm Nd3;" +
                            "4kbr1/1b1nqp2/2p1p3/2N4p/1p1PP1pP/1PpQ2B1/4BPP1/r4RK1 w - - bm Nxb7;" +
                            "r1b2rk1/p2nqppp/1ppbpn2/3p4/2P5/1PN1PN2/PBQPBPPP/R4RK1 w - - bm cxd5;" +
                            "r1b1kq1r/1p1n2bp/p2p2p1/3PppB1/Q1P1N3/8/PP2BPPP/R4RK1 w kq - bm f4;" +
                            "r4r1k/p1p3bp/2pp2p1/4nb2/N1P4q/1P5P/PBNQ1PP1/R4RK1 b - - bm Nf3;" +
                            "6k1/pb1r1qbp/3p1p2/2p2p2/2P1rN2/1P1R3P/PB3QP1/3R2K1 b - - bm Bh6;" +
                            "2r2r2/1p1qbkpp/p2ppn2/P1n1p3/4P3/2N1BB2/QPP2PPP/R4RK1 w - - bm b4;" +
                            "r1bq1rk1/p4ppp/3p2n1/1PpPp2n/4P2P/P1PB1PP1/2Q1N3/R1B1K2R b KQ - bm c4;" +
                            "2b1r3/5pkp/6p1/4P3/QppqPP2/5RPP/6BK/8 b - - bm c3;" +
                            "r2q1rk1/1p2bpp1/p1b2n1p/8/5B2/2NB4/PP1Q1PPP/3R1RK1 w - - bm Bxh6;"
                            */

        string[] positionArray = position.Split(";");
        string log = "";
        int numSuccess = 0;
        int counter = 0;
        foreach (string currentPosition in positionArray)
        {
            if (currentPosition.Length < 10) continue;
            counter++;
            string fen = currentPosition.Split("bm")[0].Trim() + " 0 1";
            Board board = new(fen);
            Move bestMove = bot.GetBestMove(board, 10000, new CancellationToken(false), true, false); 
            string bestMoveAlgebraic = bestMove.ToString();
            bool foundBestMove = false;
            foreach (string bm in currentPosition.Split("bm")[1].Split(","))
            {
                if (bm.Trim() == bestMoveAlgebraic)
                {
                    foundBestMove = true;
                    break;
                }
            }

            if (foundBestMove) 
            {
                log += "Found best move for position " + counter + ": " + bestMoveAlgebraic + "\n";
                Console.WriteLine("Found best move for position " + counter + ": " + bestMoveAlgebraic);
                numSuccess++;
            }
            else 
            {
                log += "Did not find best move for position " + counter + ", expected " + currentPosition.Split("bm")[1].Trim() + " but got " + bestMoveAlgebraic + "\n";
                Console.WriteLine("Did not find best move for position " + counter + ", expected " + currentPosition.Split("bm")[1].Trim() + " but got " + bestMoveAlgebraic);
            }
        }

        // Output results
        string results = "----------------------------------------\n" +
                        "\t\tPositions tested: " + counter + "\n" +
                        "\t\tPositions passed: " + numSuccess + "\n" +
                        "\t\tPass rate: " + numSuccess/counter*100 + "%.\n" +
                        "----------------------------------------";

        log += results;
        Console.WriteLine(results);

        bool success = false;

        while (!success)
        {
            Console.Write("\nSave results to a log file? (Y/n): ");

            ConsoleKeyInfo keyInfo = Console.ReadKey();

            if (keyInfo.Key == ConsoleKey.N)
            {
                return;
            }

            Console.Write("Enter the path to the log folder (default: '" + Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\chessbot\\logs\\'): ");
            string? path = Console.ReadLine();
            path ??= Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\chessbot\\logs\\";

            string filePath = path + "\\rapid-engine-test-" + DateTime.Now.ToString() + ".txt";
            try
            {
                using StreamWriter writer = new(filePath);
                writer.Write(log);

                Console.WriteLine("Log file saved to: " + filePath);
                success = true;
            }
            catch (IOException)
            {
                Console.WriteLine("Failed to write to log file... Does the directory exist?");
                success = false;
            }
        }

    }
}