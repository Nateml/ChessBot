namespace ChessBot;

public class OpeningMoveNode
{
    public OpeningMoveNode(ulong zobrist, Move move)
    {
        Zobrist = zobrist;
        Moves = new() { move };
    }

    public void AddMove(Move child)
    {
        Moves ??= new();
        Moves.Add(child);
    }


    public ulong Zobrist { get; }

    public List<Move>? Moves { get; private set; }
}