namespace ChessBot;

class UCI
{

    public const string EngineName = "Nate's C# ChessBot";

    private static Board board = new();

    public static void Main(string[] args)
    {
        board.AttachListener(board.moveGen);
        Console.WriteLine("Welcome to Nate's ChessBot.");
        Console.WriteLine("This program uses the UCI protocol.");
        UCICommunication();
    }

    public static void UCICommunication()
    {
        while (true)
        {
            string? input = Console.ReadLine();

            if (input == null) continue;
            if (input == "uci") InputUCI();
            if (input.StartsWith("setoption")) InputSetOption(input);
            if (input == "isready") InputIsReady();
            if (input == "ucinewgame") InputUCINewGame();
            if (input.StartsWith("position")) InputPosition(input);
            if (input.StartsWith("go")) InputGo(input);
            if (input.StartsWith("move")) InputMove(input);
            if (input == "fen") InputFen();
            if (input == "bitboards") InputBitboards();
            if (input == "test perft") InputTestPerft();
            if (input == "quit") InputQuit();
        }
    }

    static void InputUCI()
    {
        Console.WriteLine("id name " + EngineName);
        Console.WriteLine("id author Nathan Macdonald");
        Console.WriteLine("uci ok");
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

    static void InputIsReady()
    {
        Console.WriteLine("ready ok");
    }

    static void InputUCINewGame()
    {
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
        board.AttachListener(board.moveGen);

        // TODO: Add logic for making moves from input string
        if (input.Contains("moves"))
        {
            input = input[(input.IndexOf("moves")+6)..];
            Console.WriteLine(input);
            Console.WriteLine(input.Length);
            for (int i = 0; i < input.Length; i+=5)
            {
               string algebraicMove = input.Substring(i, 4);
               Console.WriteLine(algebraicMove);
               Move move = MoveUtility.ConvertFromAlgebraic(algebraicMove, board);
               board.MakeMove(move);
            }
        }
    }

    static void InputGo(string input)
    {
        input = input[3..];

        if (input.StartsWith("perft"))
        {
            int depth = int.Parse(input.Split(" ")[1]);
            int nodes = Perft.DividePerftTest(board, depth);
            Console.WriteLine("Nodes visitied: " + nodes);
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