namespace ChessBot;

using System.Linq.Expressions;
using System.Net;
using System.Security;

static class MoveGenData
{
    /**
     * Diagonal movement mask bitboard
     * Outer array indexed by position on board (0 <= i < 64)
     * Inner array indexed by diagonal direction:
     *      0 : to top right 
     *      1 : to bottom left
     *      2 : to bottom right
     *      3 : to top left
     *      (direction descriptions are based on if you were looking at the piece on an actual chessboard, with rank 1 at the bottom and file A on the left)
     * 
     * The bitboard should not include a 1 in the position of the index of the outer array, as to allow for moves which capture the piece on that index
     */
    private static ulong[][] diagonalMasks = new ulong[64][];

    /**
     * Orthogonal movement mask bitboard
     * Outer array index by position on board (0 <= i < 64)
     * Inner array indexed by sliding direction:
     *      0 : to the right
     *      1 : downwards
     *      2 : to the left
     *      3 : upwards
     *      (direction descriptions are based on if you were looking at the piece on an actual chessboard, with rank 1 at the bottom and file A on the left)
     * 
     * The bitboard should not include a 1 in the position of the index of the outer array, as to allow for moves which capture the piece on that index
     */
    private static readonly ulong[][] orthogonalMasks = new ulong[64][];

    public static readonly ulong[][] orthogonalMovesAttackTable;

    public static readonly ulong[][] diagonalMovesAttackTable;

    public static readonly ulong[] knightTargets = new ulong[64];

    private static readonly ulong knightSpan = 0xA1100110AL; // bitboard for target squares for a knight on bit 1 << 18

    public static readonly ulong[] whitePawnAttacks = new ulong[64];
    public static readonly ulong[] blackPawnAttacks = new ulong[64];

    public static readonly ulong[] kingTargets = new ulong[64];

    /// <summary>
    /// Ordered from file A to file H
    /// </summary>
    public static readonly ulong[] FileMasks = 
    {
        0x101010101010101L, 0x202020202020202L, 0x404040404040404L, 0x808080808080808L,
        0x1010101010101010L, 0x2020202020202020L, 0x4040404040404040L, 0x8080808080808080L
    };

    /// <summary>
    /// Ordered from rank 1 to rank 8
    /// </summary>
    public static readonly ulong[] RankMasks = 
    {
        0xFF00000000000000ul, 0xFF000000000000ul, 0xFF0000000000ul, 0xFF00000000ul,
        0xFF000000ul, 0xFF0000ul, 0xFF00ul, 0xFFul
    };

    public static readonly ulong[][] inBetweenLookupTable = new ulong[64][];

    /// <summary>
    /// Computes move generation data, such as paths for each piece type.
    /// </summary>
    static MoveGenData()
    {
        // Compute the movement masks
        ComputeDiagonalPaths();
        ComputeOrthogonalPaths();

        // Compute the magic movement masks
        Magic.CreateMovementMasks();

        // Compute the pseudolegal sliding move lookup tables
        orthogonalMovesAttackTable = ComputeOrthogonalMovesLookupTable();
        diagonalMovesAttackTable = ComputeDiagonalMovesLookupTable();

        knightTargets = ComputeKnightTargets();

        whitePawnAttacks = ComputeWhitePawnAttacks();
        blackPawnAttacks = ComputeBlackPawnAttacks();

        kingTargets = ComputeKingTargets();

        // Populate the in between lookup table
        inBetweenLookupTable = ComputeInBetweenLookupTable();

    }



    private static ulong[][] ComputeInBetweenLookupTable()
    {

        ulong[][] lookupTable = new ulong[64][];

        for (int startSquare = 0; startSquare < 64; startSquare++)
        {
            lookupTable[startSquare] = new ulong[64];

            for (int endSquare = 0; endSquare < 64; endSquare++)
            {
                ulong inBetween = 0;

                if (BitboardUtility.IsOnOrthogonalLine(startSquare, endSquare))
                {
                    inBetween |= ComputeLegalOrthogonalMovesBitboard(startSquare, 1ul << endSquare) & ComputeLegalOrthogonalMovesBitboard(endSquare, 1ul << startSquare);
                }
                else if (BitboardUtility.IsOnDiagonalLine(startSquare, endSquare))
                {
                    inBetween |= ComputeLegalDiagonalMovesBitboard(startSquare, 1ul << endSquare) & ComputeLegalDiagonalMovesBitboard(endSquare, 1ul << startSquare);
                }

                lookupTable[startSquare][endSquare] = inBetween;
            }    
        }

        return lookupTable;
    }

    private static ulong[] ComputeKingTargets()
    {
        ulong[] targets = new ulong[64];

        for (int i = 0; i < 64; i++)
        {
            ulong target = 0;

            // ORTHOGONAL MOVES:

            if ((i % 8) != 7) // can only move right if king is not on file H
            {
                target |= 1ul << (i+1);
            }

            if ((i % 8) != 0) // can only move left if king is not on file A
            {
                target |= 1ul << (i-1);
            }

            if ((i / 8) != 0) // can only move up if king is not on rank 8
            {
                target |= 1ul << (i-8);
            }

            if ((i / 8) != 7)
            {
                target |= 1ul << (i+8);
            }

            // top right
            if ((i % 8) != 7 && (i / 8) != 0)
            {
                target |= 1ul << (i-7);
            }

            // bottom right
            if ((i % 8) != 7 && (i / 8) != 7)
            {
                target |= 1ul << (i+9);
            }

            // bottom left
            if ((i % 8) != 0 && (i / 8) != 7)
            {
                target |= 1ul << (i+7);
            }

            // top left
            if ((i % 8) != 0 && (i / 8) != 0)
            {
                target |= 1ul << (i-9);
            }

            targets[i] = target;
        }

        return targets;
    }

    private static ulong[] ComputeWhitePawnAttacks()
    {
        ulong[] targets = new ulong[64];

        // For every square...
        for (int i = 8; i < 64; i++) // no pawn attacks on the 8th rank
        {
            ulong target = 0;

            if ((i % 8) != 7) // pawn can only move right if it is not on file H
            {
                target |= 1ul << (i-7);
            }

            if ((i % 8) != 0) // pawn can only move left if it is not on file A
            {
                target |= 1ul << (i-9);
            }

            targets[i] = target;
        }

        return targets;
    }

    private static ulong[] ComputeBlackPawnAttacks()
    {
        ulong[] targets = new ulong[64];

        // For every square...
        for (int i = 0; i < 56; i++) // no pawn attacks on the first rank
        {
            ulong target = 0;

            if ((i % 8) != 7) // pawn can only move right if it is not on file H
            {
                target |= 1ul << (i+9);
            }

            if ((i % 8) != 0) // pawn can only move left if it is not on file A
            {
                target |= 1ul << (i+7);
            }

            targets[i] = target;
        }

        return targets;
    }

    /// <summary>
    /// Generates knight target bitboards for each starting square on the board.
    /// </summary>
    private static ulong[] ComputeKnightTargets()
    {
        ulong[] targets = new ulong[64];;

        // For each square...
        for (int i = 0; i < 64; i++)
        {
            ulong target;

            if (i > 18)
            {
                target = knightSpan << (i-18);
            }
            else
            {
                target = knightSpan >> (18-i);
            }

            // Prevent span from wrapping around the edges of the board
            if (i % 8 < 4)
            {
                target &= ~(FileMasks[6] | FileMasks[7]);
            }
            else
            {
                target &= ~(FileMasks[0] | FileMasks[1]);
            }
            
            targets[i] = target;
        }

        return targets;
    }

    private static ulong[][] ComputeOrthogonalMovesLookupTable()
    {
        ulong[][] attackTable = new ulong[64][];

        // For every square on the board...
        for (int i = 0; i < 64; i++)
        {
            attackTable[i] = Magic.CreateOrthogonalAttackTable(i, ComputeBlockerBitboards(Magic.GetSimplifiedOrthogonalMovementMask(i)));
        }

        return attackTable;
    }

    private static ulong[][] ComputeDiagonalMovesLookupTable()
    {
        ulong[][] attackTable = new ulong[64][];

        // For every square on the board...
        for (int i = 0; i < 64; i++)
        {
            attackTable[i] = Magic.CreateDiagonalAttackTable(i, ComputeBlockerBitboards(Magic.GetSimplifiedDiagonalMovementMask(i)));
        }

        return attackTable;
    }

    /// <summary>
    /// Generates a bitboard containing set bits in the position of each legal target square for a diagonal move starting from the square in the given position.
    /// Treats blockers as enemies, so the returned bitboard will have set bits in positions of the set bits in the blocker bitboard.
    /// </summary>
    public static ulong ComputeLegalDiagonalMovesBitboard(int startSquare, ulong blockerBitboard)
    {
        ulong rayTopRight = DiagPathToTopRight(startSquare);
        ulong rayBottomRight = DiagPathToBottomRight(startSquare);
        ulong rayBottomLeft = DiagPathToBottomLeft(startSquare);
        ulong rayTopLeft = DiagPathToTopLeft(startSquare);

        BitboardUtility.ForEachBitscanForward(blockerBitboard, (indexLSB) => {
            ulong LSB = 1ul << indexLSB;
            if ((LSB & rayTopRight) != 0)
            {
                rayTopRight ^= rayTopRight & DiagPathToTopRight(indexLSB);
            }
            else if ((LSB & rayTopLeft) != 0)
            {
                rayTopLeft ^= rayTopLeft & DiagPathToTopLeft(indexLSB);
            }
            else if ((LSB & rayBottomRight) != 0)
            {
                rayBottomRight ^= rayBottomRight & DiagPathToBottomRight(indexLSB);
            }
            else if ((LSB & rayBottomLeft) != 0)
            {
                rayBottomLeft ^= rayBottomLeft & DiagPathToBottomLeft(indexLSB);
            }
        });

        return rayTopRight | rayBottomRight | rayBottomLeft | rayTopLeft;
    }

    /// <summary>
    /// Generates a bitboard containing set bits in the position of each legal target square for an orthogonal move starting from the square in the given position.
    /// Treats blockers as enemies, so the returned bitboard will have set bits in positions of the set bits in the blocker bitboard.
    /// </summary>
    public static ulong ComputeLegalOrthogonalMovesBitboard(int startSquare, ulong blockerBitboard)
    {

        ulong rayUp = OrthogPathUp(startSquare);
        ulong rayRight = OrthogPathRight(startSquare);
        ulong rayDown = OrthogPathDown(startSquare);
        ulong rayLeft = OrthogPathLeft(startSquare);

        ulong LSB = BitboardUtility.IsolateLSB(blockerBitboard);
        while (LSB != 0)
        {
            int indexLSB = BitboardUtility.IndexOfLSB(LSB);

            // Compute the paths by XORing the blocker paths with the movement masks

            if ((LSB & rayUp) != 0)
            {
                rayUp ^= rayUp & OrthogPathUp(indexLSB);
            }
            else if ((LSB & rayRight) != 0)
            {
                rayRight ^= rayRight & OrthogPathRight(indexLSB);
            }
            else if ((LSB & rayDown) != 0)
            {
                rayDown ^= rayDown & OrthogPathDown(indexLSB);
            }
            else if ((LSB & rayLeft) != 0)
            {
                rayLeft ^= rayLeft & OrthogPathLeft(indexLSB);
            }

            blockerBitboard &= ~LSB;
            LSB = BitboardUtility.IsolateLSB(blockerBitboard);
        }

        return rayUp | rayRight | rayDown | rayLeft;
    }



    /// <summary>
    /// Creates an array of all possible configurations of blocker pieces in the way of the given movement mask.
    /// NOTE: Configurations do not include blockers on the edges of the board, since these do not affect the eventual target bitboard.
    /// </summary>
    private static ulong[] ComputeBlockerBitboards(ulong movementMask)
    {
        // Create a list of the indices of the set bits in the movement mask
        List<int> movementMaskIndices = new();

        for (int i = 0; i < 64; i++)
        {
            // Loop through each bit and check if the bit is set
            if (BitboardUtility.IsBitSet(movementMask, i))
            {
                movementMaskIndices.Add(i);
            }
        }

        // Calculate the total number of blocker configurations
        int numConfigurations = 1 << movementMaskIndices.Count; // 2^n
        ulong[] blockerBitboards = new ulong[numConfigurations];
        
        // Create the blocker bitboards
        for (int blockerConfiguration = 0; blockerConfiguration < numConfigurations; blockerConfiguration++)
        {
            // Loop through every blocker configuration
            // Each configuration can be represented by a single number 

            for (int bitIndex = 0; bitIndex < movementMaskIndices.Count; bitIndex++)
            {
                // Shift the blocker configuration bits into the movement mask positions
                int bit = (blockerConfiguration >> bitIndex) & 1;
                blockerBitboards[blockerConfiguration] |= (ulong)bit << movementMaskIndices[bitIndex];
            }

        }

        return blockerBitboards;
    }

    /// <summary>
    /// Fills the diagonalPaths array with the appropriate path data.
    /// </summary>
    public static void ComputeDiagonalPaths()
    {
        // loop through every tile
        for (int i = 0; i < 64; i++)
        {
            diagonalMasks[i] = new ulong[4];

            diagonalMasks[i][0] = DiagPathToTopRight(i);
            diagonalMasks[i][1] = DiagPathToBottomRight(i);
            diagonalMasks[i][2] = DiagPathToBottomLeft(i);
            diagonalMasks[i][3] = DiagPathToTopLeft(i);
        }
    }

    /// <summary>
    /// Creates and returns a bitboard representing the target squares for the top right diagonal path starting at the square with index i.
    /// </summary>
    public static ulong DiagPathToTopRight(int i)
    {
        ulong bitboard = 0;
        ulong possibility = 1ul << i;

        // stop when the possibility bit reaches file H or rank 8
        while ((possibility & FileMasks[7]) == 0 && ((possibility & RankMasks[7]) == 0))
        {
            possibility >>= 7; // move possibility bit diagonally 1 tile to the top right
            bitboard |= possibility;
        }

        return bitboard;
    }

    /// <summary>
    /// Creates and returns a bitboard representing the target squares for the bottom right diagonal path starting at the square with index i.
    /// </summary>
    public static ulong DiagPathToBottomRight(int i)
    {
        ulong bitboard = 0;
        ulong possibility = 1ul << i;

        // stop when the possibility bit reaches file H or rank 1
        while ((possibility & FileMasks[7]) == 0 && ((possibility & RankMasks[0]) == 0))
        {
            possibility <<= 9; // move possibility bit diagonally 1 tile to the bottom right
            bitboard |= possibility;
        }

        return bitboard;
    }

    /// <summary>
    /// Creates and returns a bitboard representing the target squares for the bottom left diagonal path starting at the square with index i.
    /// </summary>
    public static ulong DiagPathToBottomLeft(int i)
    {
        ulong bitboard = 0;
        ulong possibility = 1ul << i;

        // stop when the possibility bit reaches file A or rank 1
        while ((possibility & FileMasks[0]) == 0 && ((possibility & RankMasks[0]) == 0))
        {
            possibility <<= 7; // move possibility bit diagonally 1 tile to the bottom left
            bitboard |= possibility;
        }

        return bitboard;
    }

    /// <summary>
    /// Creates and returns a bitboard representing the target squares for the top left diagonal path starting at the square with index i.
    /// </summary>
    public static ulong DiagPathToTopLeft(int i)
    {
        ulong bitboard = 0;
        ulong possibility = 1ul << i;

        // stop when the possibility bit reaches file A or rank 8
        while ((possibility & FileMasks[0]) == 0 && ((possibility & RankMasks[7]) == 0))
        {
            possibility >>= 9; // move possibility bit diagonally 1 tile to the top left
            bitboard |= possibility;
        }

        return bitboard;
    }

    /// <summary>
    /// Fills the orthogonalPaths array with the appropriate path data.
    /// </summary>
    private static void ComputeOrthogonalPaths()
    {
        // loop through every tile
        for (int i = 0; i < 64; i++)
        {
            orthogonalMasks[i] = new ulong[4];

            orthogonalMasks[i][0] = OrthogPathRight(i);
            orthogonalMasks[i][1] = OrthogPathDown(i);
            orthogonalMasks[i][2] = OrthogPathLeft(i);
            orthogonalMasks[i][3] = OrthogPathUp(i);
        }
    }

    /// <summary>
    /// Returns the orthogonal movement mask (rook movement) as a ulong from the specified position in the board.
    /// </summary>
    public static ulong ComputeOrthogonalMovementMask(int i)
    {
        return orthogonalMasks[i][0] | orthogonalMasks[i][1] | orthogonalMasks[i][2] | orthogonalMasks[i][3];
    }

    /// <summary>
    /// Returns the diagonal movement mask (bishop movement) as a ulong from the specified position in the board.
    /// </summary>
    public static ulong ComputeDiagonalMovementMask(int i)
    {
        return diagonalMasks[i][0] | diagonalMasks[i][1] | diagonalMasks[i][2] | diagonalMasks[i][3];
    }

    /// <summary>
    /// Creates and returns a bitboard representing the target squares for the up orthogonal sliding path starting at the square with index i.
    /// </summary>
    public static ulong OrthogPathUp(int i)
    {
        ulong bitboard = 0L;
        ulong possibility = 1ul << i;

        // stop when the possibility bit reaches rank 8
        while ((possibility & RankMasks[7]) == 0)
        {
            possibility >>= 8; // move possibility bit one tile up
            bitboard |= possibility;
        }

        return bitboard;
    }

    /// <summary>
    /// Creates and returns a bitboard representing the target squares for the right orthogonal sliding path starting at the square with index i.
    /// </summary>
    public static ulong OrthogPathRight(int i)
    {
        ulong bitboard = 0L;
        ulong possibility = 1ul << i;

        // stop when the possibility bit reaches file H
        while ((possibility & FileMasks[7]) == 0)
        {
            possibility <<= 1; // move possibility bit one tile to the right
            bitboard |= possibility;
        }

        return bitboard;
    }

    /// <summary>
    /// Creates and returns a bitboard representing the target squares for the downwards orthogonal sliding path starting at the square with index i.
    /// </summary>
    public static ulong OrthogPathDown(int i)
    {
        ulong bitboard = 0L;
        ulong possibility = 1ul << i;

        // stop when the possibility bit reaches rank 1
        while ((possibility & RankMasks[0]) == 0)
        {
            possibility <<= 8; // move possibility bit one tile down
            bitboard |= possibility;
        }

        return bitboard;
    }

    /// <summary>
    /// Creates and returns a bitboard representing the target squares for the left orthogonal sliding path starting at the square with index i.
    /// </summary>
    public static ulong OrthogPathLeft(int i)
    {
        ulong bitboard = 0L;
        ulong possibility = 1ul << i;

        // stop when the possibility bit reaches file A
        while ((possibility & FileMasks[0]) == 0)
        {
            possibility >>= 1; // move possibility bit one tile to the left
            bitboard |= possibility;
        }

        return bitboard;
    }

    /// <summary>
    /// Orthogonal target bitboard
    /// Outer array index by position on board (0 <= i < 64)
    /// Inner array indexed by sliding direction:
    ///     0 : to the right
    ///     1 : downwards 
    ///     2 : to the left
    ///     3 : upwards 
    /// (direction descriptions are based on if you were looking at the piece on an actual chessboard, with rank 1 at the bottom and file A on the left)
    /// The bitboard does not include a 1 in the position of the index of the outer array, as to allow for moves which capture the piece on that index
    /// NOTE: Does not include set bits in the edges of the ray directions.
    /// </summary>
    public static ulong[][] OrthogonalMovementMasks { get { return orthogonalMasks; } }

    /// <summary>
    /// Diagonal target bitboard
    /// Outer array index by position on board (0 <= i < 64)
    /// Inner array indexed by sliding direction:
    ///     0 : to the top right
    ///     1 : to the bottom right
    ///     2 : to the bottom left
    ///     3 : to the top left
    /// (direction descriptions are based on if you were looking at the piece on an actual chessboard, with rank 1 at the bottom and file A on the left)
    /// The bitboard does not include a 1 in the position of the index of the outer array, as to allow for moves which capture the piece on that index
    /// </summary>
    public static ulong[][] DiagonalMovementMasks { get { return diagonalMasks; } }

    public static ulong[][] OrthogonalMovesAttackTable { get { return orthogonalMovesAttackTable; } }

    public static ulong[][] DiagonalMovesAttackTable { get { return diagonalMovesAttackTable; } }

    /// <summary>
    /// Returns a bitboard with set bits in the possible knight target squares for a knight move starting at the given square.
    /// </summary>
    public static ulong GetKnightTargetsBitboard(int startingSquare)
    {
        return knightTargets[startingSquare];
    }

    public static ulong EdgeBitboard
    {
        get
        {
            return FileMasks[0] | FileMasks[7] | RankMasks[0] | RankMasks[7];
        }
    }

}