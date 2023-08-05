using ChessBot;

public static class PGNReader
{

    public static List<List<Move>> games = new();

    public static void LoadGames(String path)
    {
        string[] lines = File.ReadAllLines(path);
        bool newGame = false;
        foreach (string line in lines)
        {
            bool lastLine = false;

            if (line.StartsWith("1."))
            {
                newGame = true;
            }

            if (line.Contains("1-1") || line.Contains("1-0") || line.Contains("1/2-1/2"))
            {
                lastLine = true;                
            }

            string[] moves = line.Split(" ");
            foreach (string move in moves)
            {

            }



        }
    }
}