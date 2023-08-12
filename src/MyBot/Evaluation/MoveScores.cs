namespace ChessBot;

public class MoveScores
{

    //private (int, bool)[,] scores;
    private Dictionary<Move, (int, bool)> scores;

    public MoveScores()
    {
        //scores = new (int, bool)[64, 64];
        scores = new(48);
    }

    public void Put(Move move, int score)
    {
        scores.Add(move, (score, true));
        //scores[move.From, move.To] = (score, true);
    }

    public (int, bool) Get(Move move)
    {
        if (!scores.ContainsKey(move)) return (0, false);
        return scores[move];
    }

    private static int Hash(Move move)
    {
        return move.From | (move.To << 6);
    }


}