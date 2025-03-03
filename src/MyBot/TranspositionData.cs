namespace ChessBot;

using System.Diagnostics.Metrics;
using ChessBot;

public class TranspositionData
{
    public const byte ExactFlag = 0;
    public const byte LowerboundFlag = 1;
    public const byte UpperboundFlag = 2;

    public TranspositionData(ulong zobristHash, byte depth, byte flag, double eval, Move? bestMove)
    {
        ZobristHash = zobristHash;
        Depth = depth;
        Flag = flag;
        Eval = eval;
        BestMove = bestMove;
    }

    public ulong ZobristHash { get; private set; }

    public byte Depth { get; private set; }

    public byte Flag { get; private set; }

    public double Eval { get; private set; }

    public Move? BestMove { get; private set; }

}