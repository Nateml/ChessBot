namespace ChessBot;

public class OpeningBook
{
    private List<OpeningMoveNode> openings;

    public OpeningBook()
    {
        openings = new();
    }

    public void Add(ulong zobrist, Move move)
    {
        foreach (OpeningMoveNode node in openings)
        {
            if (node.Zobrist == zobrist)
            {
                node.AddMove(move);
                return;
            }
        }
        openings.Add(new OpeningMoveNode(zobrist, move));
    }

    public List<Move>? GetMoves(ulong zobrist)
    {
        //Console.WriteLine("Searching for: " + zobrist);
        foreach (OpeningMoveNode node in openings)
        {
            //Console.WriteLine(node.Zobrist);
            if (node.Zobrist == zobrist)
            {
                //Console.WriteLine("Found an opening...");
                return node.Moves;
            }
        }

        return null;
    }



}