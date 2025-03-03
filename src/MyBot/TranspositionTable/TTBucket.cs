namespace ChessBot;

public class TTBucket
{
    private const int length = 4;
    public TranspositionData[] entries = new TranspositionData[length];

    public TranspositionData this[byte depth]
    {
        get
        {
            return entries[depth];
        }
        set
        {
            entries[depth] = value;
        }
    }

    public TranspositionData? Get(ulong hash)
    {
        for (int i = 0; i < length; i++)
        {
            TranspositionData entry = entries[i];
            if (entry != null && entry.ZobristHash == hash)
            {
                return entry;
            }
        }
        return null;
    }

    public void Put(ulong hash, byte depth, byte flag, int eval, Move? bestMove)
    {
        int replaceIndex = -1;
        byte shallowestDepth = 255;
        // First pass: see if there is a matching slot or an empty slot
        for (int i = 0; i < length; i++)
        {
            TranspositionData entry = entries[i];
            if (entry == null)
            {
                replaceIndex = i; // Found an empty slot
                continue;
            }
            if (entry.ZobristHash == hash && (entry.Depth <= depth))
            {
                // We have found the same position
                // Always replace if the new node has a greater depth
                replaceIndex = i;
                break;
            }
            if (entry.Depth <= shallowestDepth)
            {
                // We have found a slot with a shallower depth
                replaceIndex = i;
                shallowestDepth = entry.Depth;
            }
            // if (entry.Depth <= shallowestDepth && !(flag != TranspositionData.ExactFlag && entry.Flag == TranspositionData.ExactFlag))
            // {
            //     // We have found a slot with a shallower depth
            //     // and it is either not an exact node or the new node is an exact node
            //     replaceIndex = i;
            //     shallowestDepth = entry.Depth;
            // }
        }

        // Insert or replace
        if (replaceIndex >= 0)
        {
            entries[replaceIndex] = new TranspositionData(hash, depth, flag, eval, bestMove);
        }
    }

}