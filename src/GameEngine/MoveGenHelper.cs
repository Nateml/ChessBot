namespace ChessBot;

using static MoveGenData;

static class MoveGenHelper
{

    public static ulong GetPinnedPiecesBitboard(ulong occupied, ulong friendlyPieces, ulong enemyBishop, ulong enemyRook, ulong enemyQueen, int kingSquare)
    {
        ulong pinned = 0;
        ulong pinner = XRayRookAttacks(occupied, friendlyPieces, kingSquare) & (enemyRook | enemyQueen);
        while (pinner != 0)
        {
            int square = BitboardUtility.IndexOfLSB(pinner);
            pinned |= inBetweenLookupTable[square][kingSquare] & friendlyPieces;
            pinner &= pinner - 1; // LSB reset
        }

        pinner = XRayBishopAttacks(occupied, friendlyPieces, kingSquare) & (enemyBishop | enemyQueen);
        while (pinner != 0)
        {
            int square = BitboardUtility.IndexOfLSB(pinner);
            pinned |= inBetweenLookupTable[square][kingSquare] & friendlyPieces;
            pinner &= pinner - 1;
        }

        return pinned;
    }

    public static ulong XRayRookAttacks(ulong occupied, ulong blockers, int rookSquare)
    {
        ulong attacks = RookAttacks(occupied, rookSquare);
        blockers &= attacks; // Intersect the blockers with the orthogonal attacks
        return attacks ^ RookAttacks(occupied ^ blockers, rookSquare); // Remove the closest blockers from the occupied bitboard to reveal the second closest blockers
    }

    public static ulong XRayBishopAttacks(ulong occupied, ulong blockers, int bishopSquare)
    {
        ulong attacks = BishopAttacks(occupied, bishopSquare);
        blockers &= attacks; // Intersect the blockers with the diagonal attacks
        return attacks ^ BishopAttacks(occupied ^ blockers, bishopSquare); // Remove the closest blockers from the occupied bitboard to reveal the second closest blockers
    }


    /// <summary>
    /// Returns a rook attack bitboard given the bitboard of all the pieces on the board (including the rook in question) and the square of the rook.
    /// </summary>
    public static ulong RookAttacks(ulong allPieces, int rookSquare)
    {
        allPieces ^= 1ul << rookSquare; // excude the rook from the blockers
        ulong movementMask = Magic.GetSimplifiedOrthogonalMovementMask(rookSquare);
        ulong blockers = Magic.CreateBlockerBitboard(allPieces, movementMask);
        return orthogonalMovesAttackTable[rookSquare][Magic.GetOrthogonalAttackTableKey(rookSquare, blockers)];
    }

    /// <summary>
    /// Returns a bishop attack bitboard given the bitboard of all the pieces on the board (including the bishop in question) and the square of the bishop.
    /// </summary>
    public static ulong BishopAttacks(ulong allPieces, int bishopSquare)
    {
        allPieces ^= 1ul << bishopSquare; // excude the rook from the blockers
        ulong movementMask = Magic.GetSimplifiedDiagonalMovementMask(bishopSquare);
        ulong blockers = Magic.CreateBlockerBitboard(allPieces, movementMask);
        return DiagonalMovesAttackTable[bishopSquare][Magic.GetDiagonalAttackTableKey(bishopSquare, blockers)];
    }

    /// <summary>
    /// Returns a bishop attack bitboard given the bitboard of all the pieces on the board (including the bishop in question) and the square of the bishop.
    /// </summary>
    public static ulong QueenAttacks(ulong allPieces, int queenSquare)
    {
        allPieces ^= 1ul << queenSquare; // excude the queen from the blockers
        ulong diagMovementMask = Magic.GetSimplifiedDiagonalMovementMask(queenSquare);
        ulong orthogMovementMask = Magic.GetSimplifiedOrthogonalMovementMask(queenSquare);
        ulong diagBlockers = Magic.CreateBlockerBitboard(allPieces, diagMovementMask);
        ulong orthogBlockers = Magic.CreateBlockerBitboard(allPieces, orthogMovementMask);
        return DiagonalMovesAttackTable[queenSquare][Magic.GetDiagonalAttackTableKey(queenSquare, diagBlockers)] | OrthogonalMovesAttackTable[queenSquare][Magic.GetOrthogonalAttackTableKey(queenSquare, orthogBlockers)];
    }
}