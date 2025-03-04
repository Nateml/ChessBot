namespace ChessBot;

class UCI
{

    public const string EngineName = "Nate's C# ChessBot";

    private static Board board = new();
    private static IChessBot bot = new TreestrapBot();
    private static EvaluationManager evalManager = new(board);

    private static bool hasStopBeenRequested = false;
    private static Thread? searchThread; // Thread for running the search
    private static CancellationTokenSource? cts; // Token for cancelling the search
    private static bool isPondering = false;

    public static void Main(string[] args)
    {
        // board.AttachListener(board.moveGen); // This shouldn't be necessary anymore, it is being done in the moveGen constructor
        Console.WriteLine("Welcome to Nate's ChessBot.");
        Console.WriteLine("This program uses the UCI protocol.");

        /*
        new Thread(delegate () {
            while (true)
            {
                Thread.Sleep(1000);
                if (!Console.IsInputRedirected) continue;
                string? input = Console.ReadLine();
                if (input == null) continue;
                hasStopBeenRequested = input.Contains("stop");
            }
        }).Start();
        */

        Task.Run(() => UCICommunication());

        // Keep the main thread alive
        while (true) Thread.Sleep(1000);

    }

    public static void UCICommunication()
    {
        while (true)
        {
            string? input = Console.ReadLine();

            if (input == null) continue;

            // Any input should stop the engine from pondering
            if (isPondering && searchThread != null)
            {
                cts?.Cancel();
                searchThread.Join();
                isPondering = false;
            }

            if (input == "uci") InputUCI();
            if (input.StartsWith("setoption")) InputSetOption(input);
            if (input == "isready") InputIsReady();
            if (input == "ucinewgame") InputUCINewGame();
            if (input.StartsWith("position")) InputPosition(input);
            if (input.StartsWith("go")) InputGo(input);
            if (input.StartsWith("move")) InputMove(input);
            if (input == "fen") InputFen();
            if (input == "bitboards") InputBitboards();
            if (input == "zobrist") InputZobrist();
            if (input == "test perft") InputTestPerft();
            if (input == "transposition table stats") InputTranspositionTableStats();
            if (input == "rapid engine test") InputRapidEngineTest();
            if (input.StartsWith("speed test")) InputSpeedTest(input);
            if (input.StartsWith("zobrist test")) InputZobristTest(input);
            if (input == "eval static") InputEvalStatic();
            if (input == "undo move") InputUndoMove();
            if (input == "quit") InputQuit();
            if (input == "stop") InputStop();
            if (input.StartsWith("debug")) InputDebug(input);
        }
    }

    static void InputUCI()
    {
        Console.WriteLine("id name " + EngineName);
        Console.WriteLine("id author Nathan Macdonald");
        Console.WriteLine("uciok");
    }

    static void InputDebug(String input)
    {
        if (input == "debug repetitions")
        {
            Console.WriteLine("Num ply since pawn move or capture: " + board.NumPlySincePawnMoveOrCapture);
            Console.WriteLine("Repetitions: " + TreestrapBot.CountRepetitions(board.History, board.ZobristHash, board.NumPlySincePawnMoveOrCapture, true));
        }
        else if(input == "debug ply")
        {
            Console.WriteLine("Ply: " + board.NumPlyPlayed);
            Console.WriteLine("Ply since pawn move or capture: " + board.NumPlySincePawnMoveOrCapture);
        }
    }

    static void InputSpeedTest(String input)
    {
        int time;

        try
        {
            time = int.Parse(input.Split(" ")[2]);
        }
        catch (Exception ex) when (ex is ArgumentNullException || ex is FormatException || ex is IndexOutOfRangeException)
        {
            Console.WriteLine("Invalid input. Please enter a time (ms) after 'speed test'.");
            return;
        }

        SpeedTest.Test(bot, time);
    }

    static void InputZobristTest(String input)
    {
        // Extract depth from input
        int depth;

        try
        {
            depth = int.Parse(input.Split(" ")[2]);
        }
        catch (Exception ex) when (ex is ArgumentNullException || ex is FormatException || ex is IndexOutOfRangeException)
        {
            Console.WriteLine("Invalid input. Please enter a depth after 'zobrist test'.");
            return;
        }

        ZobristTest.Test(depth);
    }

    static void InputEvalStatic()
    {
        Console.WriteLine(evalManager.GamePhase);
        Console.WriteLine(evalManager.MaterialScore);
        Console.WriteLine(evalManager.PieceSquareScore);
        int eval = Evaluation.EvaluateBoard(board, evalManager);
        Console.WriteLine(eval);
    }

    static void InputRapidEngineTest()
    {
        PositionTest.Test(bot);
    }

    static void InputZobrist()
    {
        Console.WriteLine(board.ZobristHash);
    }

    static void InputTranspositionTableStats()
    {
        Console.WriteLine("Transposition table contains " + ((TreestrapBot)bot).tTable.PopCount() + " transposition.");
        Console.WriteLine("Transposition table is " + ((TreestrapBot)bot).tTable.PercentageFull() + "% full.");
        Console.WriteLine("There have been " + ((TreestrapBot)bot).tTable.collisions + " collisions.");
    }

    static void InputTestPerft()
    {
        bool result = Tests.MoveGenTest();
        if (!result) Console.WriteLine("Test failed.");
        else Console.WriteLine("Test passed.");
    }

    static void InputBitboards()
    {
        for (int i = 0; i < board.Bitboards.Length; i++)
        {
            Console.WriteLine("Bitboard for " + (PieceType)i);
            BitboardUtility.PrintBitboard(board.Bitboards[i]);
            Console.WriteLine("");
        }
    }

    static void InputSetOption(string input)
    {

    }

    static void InputUndoMove()
    {
        board.UnmakeMove();
        evalManager.Undo();
    }

    static void InputIsReady()
    {
        if (board != null && bot != null)
        {
            Console.WriteLine("readyok");
        }
    }

    static void InputUCINewGame()
    {
        bot.ResetGame();
    }

    static void InputPosition(string input)
    {
        input = input[9..] + " ";

        string fen = "";

        if (input.Contains("startpos"))
        {
            input = input[9..];
            fen = Board.FenStartingPosition;
        }
        else if (input.Contains("fen"))
        {
            input = input[4..];
            fen = input;
        }

        board = new(fen);
        evalManager = new(board);
        board.AttachListener(board.moveGen);

        // TODO: Add logic for making moves from input string
        if (input.Contains("moves"))
        {
            input = input[(input.IndexOf("moves")+6)..];
            string[] moves = input.Split(" ");
            foreach (string algebraicMove in moves)
            {
                if (algebraicMove.Trim().Length > 0)
                {
                    Move move = MoveUtility.ConvertFromAlgebraic(algebraicMove, board);
                    board.MakeMove(move);
                    evalManager.Update(move);
                }
            }
        }
    }

    static void InputGo(string input)
    {
        cts = new();
        CancellationToken token = cts.Token;

        if (searchThread != null && searchThread.IsAlive)
        {
            Console.WriteLine("info string Search already in progress.");
            return;
        }

        searchThread = new Thread(() => {
            if (input.StartsWith("go perft"))
            {
                if (input.Contains("capturesonly"))
                {
                    input = input[3..];
                    int depth = int.Parse(input.Split(" ")[1]);
                    int nodes = Perft.DividePerftTest(board, depth, true, true);
                    Console.WriteLine("Nodes visitied: " + nodes);
                }
                else
                {
                    input = input[3..];
                    int depth = int.Parse(input.Split(" ")[1]);
                    int nodes = Perft.DividePerftTest(board, depth);
                    Console.WriteLine("Nodes visitied: " + nodes);
                }
            }
            else if (input.StartsWith("go infinite"))
            {
                Move bestMove = bot.GetBestMove(board, int.MaxValue, token);
                Console.WriteLine("bestmove " + bestMove.ToString());
            }
            else if (input.StartsWith("go ponder"))
            {
                isPondering = true;
                Move bestMove = bot.GetBestMove(board, int.MaxValue, token, false);
            }
            else
            {
                int timeLeft = 300000; // Assume 5 minute game
                if (board.IsWhiteToMove)
                {
                    if (input.Contains("wtime"))
                    {
                        string[] words = input[(input.IndexOf("wtime") + 5)..].Split(" ");
                        timeLeft = int.Parse(words[1].Trim());
                    }
                }
                else
                {
                    if (input.Contains("btime"))
                    {
                        string[] words = input[(input.IndexOf("btime") + 5)..].Split(" ");
                        timeLeft = int.Parse(words[1].Trim());
                    }
                }
                Move bestMove = bot.GetBestMove(board, timeLeft, token, false, true);
                Console.WriteLine("bestmove " + bestMove.ToString());
            }
        });

        searchThread.Start();

    }

    static void InputStop()
    {
        cts?.Cancel();   

        if (searchThread != null && searchThread.IsAlive)
        {
            searchThread.Join(); // Wait for the search to finish
        }
    }

    static void InputMove(string input)
    {
        input = input[5..];
        string[] moves = input.Split(" ");
        foreach (string algebraicMove in moves)
        {
            Move move = MoveUtility.ConvertFromAlgebraic(algebraicMove, board);
            board.MakeMove(move);
        }
    }

    static void InputFen()
    {
        Console.WriteLine(FenBuilder.BuildFen(board));
    }

    static void InputQuit()
    {
        Environment.Exit(0);
    }

}