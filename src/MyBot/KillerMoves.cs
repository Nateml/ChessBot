namespace ChessBot;

using System.Configuration.Assemblies;
using ChessBot;

public class KillerMoves
{
    private Move[][] moves;

    private const int numKillerMoves = 2;

    private int maxDistanceFromRoot;

    public KillerMoves(int maxDistanceFromRoot)
    {
        moves = new Move[maxDistanceFromRoot][];
        for (int i = 0; i < maxDistanceFromRoot; i++)
        {
            moves[i] = new Move[numKillerMoves];
        }
        this.maxDistanceFromRoot = maxDistanceFromRoot;
    }

    public void Insert(Move move, int distanceFromRoot)
    {
        moves[distanceFromRoot][1] = moves[distanceFromRoot][0];
        moves[distanceFromRoot][0] = move;
    }

    public bool Contains(Move? move, int distanceFromRoot)
    {
        return (moves[distanceFromRoot][0]!= null && moves[distanceFromRoot][0].Equals(move)) || (moves[distanceFromRoot][1] != null && moves[distanceFromRoot][1].Equals(move));
    }

    public void Clear()
    {
        moves = new Move[maxDistanceFromRoot][];
        for (int i = 0; i < maxDistanceFromRoot; i++)
        {
            moves[i] = new Move[numKillerMoves];
        }
    }

}