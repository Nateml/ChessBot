namespace ChessBot;
using System.Dynamic;
using ChessBot;

public class TranspositionTable
{

    int size;
    TranspositionData[] table;

    public int collisions = 0;

    public TranspositionTable(int size)
    {
        this.size = size;
        table = new TranspositionData[size];
    }

    public void Put(Board board, int depth, int flag, int eval, Move? bestMove)
    {
        int index = (int) (board.ZobristHash % (ulong)size);
        table[index] = new TranspositionData(board.ZobristHash, depth, flag, eval, bestMove);
    }

    public TranspositionData Get(Board board)
    {
        int index = (int) (board.ZobristHash % (ulong)size);
        return table[index];
    }

    public bool Contains(Board board)
    {
        int index = (int) (board.ZobristHash % (ulong)size);
        if (table[index] != null && table[index].ZobristHash != board.ZobristHash) collisions++;
        return table[index] != null && table[index].ZobristHash == board.ZobristHash;
    }

    public void Clear()
    {
        table = new TranspositionData[size];
    }

    public int PopCount()
    {
        int count = 0;
        for (int i = 0; i < size; i++)
        {
            if (table[i] != null) count++;
        }
        return count;
    }

    public int PercentageFull()
    {
        int count = PopCount();
        return (int)(((float)count)/ size * 100);
    }
}