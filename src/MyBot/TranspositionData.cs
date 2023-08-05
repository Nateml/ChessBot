namespace ChessBot;

using System.Diagnostics.Metrics;
using ChessBot;

public class TranspositionData
{
    public const int ExactFlag = 0;
    public const int LowerboundFlag = 1;
    public const int UpperboundFlag = 2;

    public TranspositionData(ulong zobristHash, int depth, int flag, int eval, Move? bestMove)
    {
        ZobristHash = zobristHash;
        Depth = depth;
        Flag = flag;
        Eval = eval;
        BestMove = bestMove;
    }

    public ulong ZobristHash { get; private set; }

    public int Depth { get; private set; }

    public int Flag { get; private set; }

    public int Eval { get; private set; }

    public Move? BestMove { get; private set; }

}