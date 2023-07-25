
using System.Net;

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
    private static ulong[][] orthogonalMasks = new ulong[64][];

    private static Dictionary<(int, ulong), ulong> orthogonalMovesLookupTable = new();

    private static Dictionary<(int, ulong), ulong> diagonalMovesLookupTable = new();

    private static readonly ulong[] FileMasks = 
    {
        0x101010101010101L, 0x202020202020202L, 0x404040404040404L, 0x808080808080808L,
        0x1010101010101010L, 0x2020202020202020L, 0x4040404040404040L, 0x8080808080808080L
    };

    private static readonly ulong[] RankMasks = 
    {
        0xFFL, 0xFF00L, 0xFF0000L, 0xFF000000L, 
        0xFF00000000L, 0xFF0000000000L, 0xFF000000000000L, 0xFF00000000000000L
    };

    /// <summary>
    /// Computes move generation data, such as paths for each piece type.
    /// This should be called once before any other methods in this class are used.
    /// </summary>
    public static void Init()
    {
        // Compute the movement masks
        ComputeDiagonalPaths();
        ComputeOrthogonalPaths();

        // Compute the legal move lookup tables
        orthogonalMovesLookupTable = ComputeOrthogonalMovesLookupTable();
        diagonalMovesLookupTable = ComputeDiagonalMovesLookupTable();
    }

    private static Dictionary<(int, ulong), ulong> ComputeOrthogonalMovesLookupTable()
    {
        Dictionary<(int, ulong), ulong> lookupTable = new();

        // For every square on the board...
        for (int i = 0; i < 64; i++)
        {
            ulong movementMask = ComputeOrthogonalMovementMask(i); // Get the orthogonal movement mask
            ulong[] blockerConfigurations = ComputeBlockerBitboards(movementMask); // Generate all blocker bitboard configurations given the current movement mask

            // Calculate the legal orthogonal moves for each blocker bitboard
            foreach (ulong blockerBitboard in blockerConfigurations)
            {
                ulong legalMoveBitboard = ComputeLegalOrthogonalMovesBitboard(i, blockerBitboard);
                lookupTable.Add((i, blockerBitboard), legalMoveBitboard);
            }
        }

        return lookupTable;
    }

    private static Dictionary<(int, ulong), ulong> ComputeDiagonalMovesLookupTable()
    {
        Dictionary<(int, ulong), ulong> lookupTable = new();

        // For every square on the board...
        for (int i = 0; i < 64; i++)
        {
            ulong movementMask = ComputeDiagonalMovementMask(i);
            ulong[] blockerConfigurations = ComputeBlockerBitboards(movementMask);

            // Calculate the legal diagonal moves for each blocker bitboard
            foreach (ulong blockerBitboard in blockerConfigurations)
            {
                ulong legalMoveBitboard = ComputeLegalDiagonalMovesBitboard(i, blockerBitboard);
                lookupTable.Add((i, blockerBitboard), legalMoveBitboard);
            }
        }

        return lookupTable;
    }

    /// <summary>
    /// Generates a bitboard containing set bits in the position of each legal target square for an orthogonal move starting from the square in the given position.
    /// Treats blockers as enemies, so the returned bitboard will have set bits in positions of the set bits in the blocker bitboard.
    /// </summary>
    private static ulong ComputeLegalOrthogonalMovesBitboard(int startSquare, ulong blockerBitboard)
    {
        ulong targets = 0;

        ulong LSB = BitboardUtility.IsolateLSB(blockerBitboard);
        while (LSB != 0)
        {
            int indexLSB = BitboardUtility.IndexOfLSB(LSB);

            // Compute the paths by XORing the blocker paths with the movement masks
            targets |= OrthogPathUp(startSquare) ^ (OrthogPathUp(startSquare) & OrthogPathUp(indexLSB));
            targets |= OrthogPathRight(startSquare) ^ (OrthogPathRight(startSquare) & OrthogPathRight(indexLSB));
            targets |= OrthogPathDown(startSquare) ^ (OrthogPathDown(startSquare) & OrthogPathDown(indexLSB));
            targets |= OrthogPathLeft(startSquare) ^ (OrthogPathLeft(startSquare) & OrthogPathLeft(indexLSB));

            blockerBitboard &= ~LSB;
            LSB = BitboardUtility.IsolateLSB(blockerBitboard);
        }

        return targets;
    }

    /// <summary>
    /// Generates a bitboard containing set bits in the position of each legal target square for a diagonal move starting from the square in the given position.
    /// Treats blockers as enemies, so the returned bitboard will have set bits in positions of the set bits in the blocker bitboard.
    /// </summary>
    private static ulong ComputeLegalDiagonalMovesBitboard(int startSquare, ulong blockerBitboard)
    {
        ulong targets = 0;

        ulong LSB = BitboardUtility.IsolateLSB(blockerBitboard);
        while (LSB != 0)
        {
            int indexLSB = BitboardUtility.IndexOfLSB(LSB);

            targets |= DiagPathToTopRight(startSquare) ^ (DiagPathToTopRight(startSquare) & DiagPathToTopRight(indexLSB));
            targets |= DiagPathToTopLeft(startSquare) ^ (DiagPathToTopLeft(startSquare) & DiagPathToTopLeft(indexLSB));
            targets |= DiagPathToBottomRight(startSquare) ^ (DiagPathToBottomRight(startSquare) & DiagPathToBottomRight(indexLSB));
            targets |= DiagPathToBottomLeft(startSquare) ^ (DiagPathToBottomLeft(startSquare) & DiagPathToBottomLeft(indexLSB));

            blockerBitboard &= ~LSB;
            LSB = BitboardUtility.IsolateLSB(blockerBitboard);
        }

        return targets;
    }

    /// <summary>
    /// Creates an array of all possible configurations of blocker pieces in the way of the given movement mask.
    /// </summary>
    private static ulong[] ComputeBlockerBitboards(ulong movementMask)
    {
        // Create a list of the indices of the set bits in the movement mask
        List<int> movementMaskIndices = new();
        for (int i = 0; i < 64; i++)
        {
            // Loop through each bit and check if the bit is set
            if (((movementMask >> i) & 1)==1)
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
    private static void ComputeDiagonalPaths()
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
    private static ulong DiagPathToTopRight(int i)
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
    private static ulong DiagPathToBottomRight(int i)
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
    private static ulong DiagPathToBottomLeft(int i)
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
    private static ulong DiagPathToTopLeft(int i)
    {
        ulong bitboard = 0;
        ulong possibility = 1ul << i;

        // stop when the possibility bit reaches file A or rank 1
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
    private static ulong ComputeOrthogonalMovementMask(int i)
    {
        return OrthogPathRight(i) | OrthogPathDown(i) | OrthogPathLeft(i) | OrthogPathUp(i);
    }

    /// <summary>
    /// Returns the diagonal movement mask (bishop movement) as a ulong from the specified position in the board.
    /// </summary>
    private static ulong ComputeDiagonalMovementMask(int i)
    {
        return DiagPathToTopRight(i) | DiagPathToBottomRight(i) | DiagPathToBottomLeft(i) | DiagPathToTopLeft(i);
    }
    /// <summary>
    /// Creates and returns a bitboard representing the target squares for the up orthogonal sliding path starting at the square with index i.
    /// </summary>
    private static ulong OrthogPathUp(int i)
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
    private static ulong OrthogPathRight(int i)
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
    private static ulong OrthogPathDown(int i)
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
    private static ulong OrthogPathLeft(int i)
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
    /// </summary>
    public static ulong OrthogonalPaths { get; }

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
    public static ulong DiagonalPaths { get; }

    public static Dictionary<(int, ulong), ulong>? OrthogonalMovesLookupTable
    {
        get;
        private set;
    }

    public static Dictionary<(int, ulong), ulong>? DiagonalMovesLookupTable
    {
        get;
        private set;
    }

}