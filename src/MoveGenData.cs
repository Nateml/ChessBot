
using System.Net;

static class MoveGenData
{
    /**
     * Diagonal Target bitboard
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
    private static ulong[][] diagonalPaths = new ulong[64][];

    /**
     * Orthogonal target bitboard
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
    private static ulong[][] orthogonalPaths = new ulong[64][];

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
        ComputeDiagonalPaths();
        ComputeOrthogonalPaths();
    }

    /// <summary>
    /// Fills the diagonalPaths array with the appropriate path data.
    /// </summary>
    private static void ComputeDiagonalPaths()
    {
        // loop through every tile
        for (int i = 0; i < 64; i++)
        {
            diagonalPaths[i] = new ulong[4];

            diagonalPaths[i][0] = DiagPathToTopRight(i);
            diagonalPaths[i][1] = DiagPathToBottomRight(i);
            diagonalPaths[i][2] = DiagPathToBottomLeft(i);
            diagonalPaths[i][3] = DiagPathToTopLeft(i);
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
            orthogonalPaths[i] = new ulong[4];

            orthogonalPaths[i][0] = OrthogPathRight(i);
            orthogonalPaths[i][1] = OrthogPathDown(i);
            orthogonalPaths[i][2] = OrthogPathLeft(i);
            orthogonalPaths[i][3] = OrthogPathUp(i);
        }
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
}