namespace ChessBot;

public class Positions
{

    public const string endgamePath = "positions/endgames.csv";

    public static string[]? Fens;


    public static void LoadEndgames()
    {
        // Read all lines from the CSV file.
        string[] lines = File.ReadAllLines(endgamePath);

        // Create a list to hold the FEN strings.
        List<string> fenList = new List<string>();

        // Skip the header (first line) and process each subsequent line.
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue; // skip empty lines

            // Split the line by commas.
            // (This simple approach works if fields do not contain commas.)
            string[] fields = line.Split(',');

            if (fields.Length > 0)
            {
                // The first field is the FEN.
                string fen = fields[0].Trim();
                if (!string.IsNullOrEmpty(fen))
                    fenList.Add(fen);
            }
        }

        // Convert the list to an array.
        Fens = fenList.ToArray();
    }
}