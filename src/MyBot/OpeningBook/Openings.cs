namespace ChessBot;

public class Openings
{
    private static readonly string? path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

    private static readonly OpeningBook openingBook;

    static Openings()
    {
        openingBook = new();
        InitOpeningBook();
    }

    private static void InitOpeningBook()
    {
        Board board = new();
        string[] textFromFile = new string[0];
        try
        {
            textFromFile = File.ReadAllLines(Path.Combine(path, "openings.txt"));
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("Openings file not found.");
            return;
        }
        catch (DirectoryNotFoundException)
        {
            Console.WriteLine("Openings file not found.");
            return;
        }
        int depth = 0;
        foreach (string line in textFromFile)
        {
            if (line.StartsWith("#")) continue;
            if (line == "") continue;

            int newDepth = line.Count(x => x == '-');

            if (newDepth <= depth && board.NumPlyPlayed != 0)
            {
                for (int i = newDepth; i <= depth; i++)
                {
                    board.UnmakeMove();
                }
            }


            string trimmedLine = line[(line.Length-4)..];

            Move move = MoveUtility.ConvertFromAlgebraic(trimmedLine, board);

            openingBook.Add(board.ZobristHash, move);

            board.MakeMove(move);
            depth = newDepth;
        }
    }

    public static OpeningBook GetOpeningBook()
    {
        return openingBook;
    }

}