namespace ChessBot;

using static PrecomputedMagics;
using static MoveGenData;

public static class Magic
{

    /// <summary>
    /// Diagonal movement mask.
    /// SHOULD ONLY BE USED WHEN CREATING BLOCKER BITBOARDS.
    /// </summary>
    private static ulong[] diagonalMovementMasksForBlockers = new ulong[64];

    /// <summary>
    /// Orthogonal movement mask.
    /// SHOULD ONLY BE USED WHEN CREATING BLOCKER BITBOARDS.
    /// </summary>
    private static ulong[] orthogonalMovementMasksForBlockers = new ulong[64];


    public static void CreateMovementMasks()
    {
        for (int i = 0; i < 64; i++)
        {
            diagonalMovementMasksForBlockers[i] = BitboardUtility.ExcludeBitsUsingMask(DiagPathToTopRight(i), FileMasks[7]|RankMasks[7]) | BitboardUtility.ExcludeBitsUsingMask(DiagPathToBottomRight(i), FileMasks[7]|RankMasks[0]) | BitboardUtility.ExcludeBitsUsingMask(DiagPathToBottomLeft(i), FileMasks[0]|RankMasks[0]) | BitboardUtility.ExcludeBitsUsingMask(DiagPathToTopLeft(i), FileMasks[0]|RankMasks[7]);
            orthogonalMovementMasksForBlockers[i] = BitboardUtility.ExcludeBitsUsingMask(OrthogPathUp(i), RankMasks[7]) | BitboardUtility.ExcludeBitsUsingMask(OrthogPathRight(i), FileMasks[7]) | BitboardUtility.ExcludeBitsUsingMask(OrthogPathDown(i), RankMasks[0]) | BitboardUtility.ExcludeBitsUsingMask(OrthogPathLeft(i), FileMasks[0]);
        } 
    }

    public static ulong GetOrthogonalAttackTableKey(int square, ulong blockerBitboard)
    {
        return (blockerBitboard * RookMagics[square]) >> RookShifts[square];
    }

    public static ulong GetDiagonalAttackTableKey(int square, ulong blockerBitboard)
    {
        return (blockerBitboard * BishopMagics[square]) >> BishopShifts[square];
    }

    public static ulong[] CreateOrthogonalAttackTable(int square, ulong[] blockerConfigurations)
    {
        ulong magic = RookMagics[square];
        int shift = RookShifts[square];

        int numBits = 64 - shift;
        int tableSize = 1 << numBits; // 2^n

        ulong[] table = new ulong[tableSize];

        foreach (ulong blockerBitboard in blockerConfigurations)
        {
            ulong index = (blockerBitboard * magic) >> shift;
            ulong targets = ComputeLegalOrthogonalMovesBitboard(square, blockerBitboard);
            /*
            if (square == 48)
            {
                Console.WriteLine("Blocker bitboard:");
                Console.WriteLine("");
                BitboardUtility.PrintBitboard(blockerBitboard);
                Console.WriteLine("Target bitboard:");
                Console.WriteLine("");
                BitboardUtility.PrintBitboard(targets);
            }
            */
            table[index] = targets;
        } 

        return table;
    }

    public static ulong[] CreateDiagonalAttackTable(int square, ulong[] blockerConfigurations)
    {
        ulong magic = BishopMagics[square];
        int shift = BishopShifts[square];

        int numBits = 64 - shift;
        int tableSize = 1 << numBits;

        ulong[] table = new ulong[tableSize];

        foreach (ulong blockerBitboard in blockerConfigurations)
        {
            ulong index = (blockerBitboard * magic) >> shift;
            ulong targets = MoveGenData.ComputeLegalDiagonalMovesBitboard(square, blockerBitboard);
            table[index] = targets;
        }

        return table;
    }

    public static ulong GetBishopTargets(int bishopSquare, ulong allPieces)
    {
        ulong blockerBitboard = CreateBlockerBitboard(allPieces ^ (1ul << bishopSquare), GetSimplifiedDiagonalMovementMask(bishopSquare));
        return DiagonalMovesAttackTable[bishopSquare][GetDiagonalAttackTableKey(bishopSquare, blockerBitboard)];
    }

    public static ulong GetRookTargets(int rookSquare, ulong allPieces)
    {
        ulong blockerBitboard = CreateBlockerBitboard(allPieces ^ (1ul << rookSquare), GetSimplifiedOrthogonalMovementMask(rookSquare));
        return OrthogonalMovesAttackTable[rookSquare][GetOrthogonalAttackTableKey(rookSquare, blockerBitboard)];
    }

    public static ulong CreateBlockerBitboard(ulong blockers, ulong movementMask)
    {
        return movementMask & blockers;
    }

    public static ulong GetSimplifiedDiagonalMovementMask(int square)
    {
        return diagonalMovementMasksForBlockers[square];
    }

    public static ulong GetSimplifiedOrthogonalMovementMask(int square)
    {
        return orthogonalMovementMasksForBlockers[square];
    }

}