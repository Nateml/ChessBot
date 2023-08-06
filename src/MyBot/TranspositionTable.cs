namespace ChessBot;

using System.ComponentModel;
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

    public void Put(Board board, byte depth, byte flag, int eval, Move? bestMove)
    {
        int index = (int) (board.ZobristHash % (ulong)size);
        if (table[index] == null)
        {
            // Insert into index
            table[index] = new(board.ZobristHash, depth, flag, eval, bestMove);
        }
        else
        {
            collisions++;

            /*
            if (depth >= table[index].Depth)
            {
                table[index] = new(board.ZobristHash, depth, flag, eval, bestMove);
            }
            */

            if (flag == TranspositionData.ExactFlag && table[index].Flag != TranspositionData.ExactFlag)
            {
                // Always replace cut nodes with exact nodes
                table[index] = new(board.ZobristHash, depth, flag, eval, bestMove);
            }
            else if (!(flag != TranspositionData.ExactFlag && table[index].Flag == TranspositionData.ExactFlag) && table[index].Depth <= depth)
            {
                // Never replace an exact node with a non-exact node
                // Only replace if the new node does not have a shallower depth than the table node
                table[index] = new(board.ZobristHash, depth, flag, eval, bestMove);
            }
        }
    }

    public TranspositionData? Get(Board board)
    {
        int index = (int) (board.ZobristHash % (ulong)size);
        if (table[index] != null && table[index].ZobristHash == board.ZobristHash)
        {
            return table[index];
        }
        else
        {
            return null;
        }
    }

    /*
    public bool Contains(Board board)
    {
        int index = (int) (board.ZobristHash % (ulong)size);
        if (table[index] != null && table[index].ZobristHash != board.ZobristHash) collisions++;
        return (table[index] != null && table[index].ZobristHash == board.ZobristHash) || (table[index+1 % size] != null && table[(index+1) % size].ZobristHash == board.ZobristHash);
    }
    */

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