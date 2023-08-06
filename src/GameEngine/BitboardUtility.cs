namespace ChessBot;

using System.Linq.Expressions;
using System.Numerics;

static class BitboardUtility
{
    public readonly static ulong[] FileMasks = {
        0x101010101010101L, 0x202020202020202L, 0x404040404040404L, 0x808080808080808L,
        0x1010101010101010L, 0x2020202020202020L, 0x4040404040404040L, 0x8080808080808080L
    };

    public readonly static ulong[] SquareFileTable = new ulong[64];

    public static void PrintBitboard(ulong bitboard)
    {
        string[][] board = new string[8][];


        for (int i = 0; i < 64; i++) {
            if (board[i/8] is null)
            {
                board[i/8] = new string[8];
            }
            board[i/8][i%8] = "0 ";
        }

        for (int i = 0; i < 64; i++) {

            if (((bitboard >> i)  & 0b1) == 1)
            {
                board[i/8][i%8] = "1 ";
            }
        }

        for (int i = 0; i < 8; i++) {
            Console.WriteLine(string.Join("", board[i]));
        }
    }

    public static bool IsOnOrthogonalLine(int i, int j)
    {
        if ((i / 8) == (j / 8) | (i % 8) == (j % 8)) return true;
        return false;
    }

    public static bool IsOnDiagonalLine(int i, int j)
    {
        //if (Math.Abs((i / 8) - (j / 8)) == Math.Abs((i % 8) - (j % 8))) return true;
        if ((i / 8) - (i % 8) == (j / 8) - (j % 8) || (i / 8) + (i % 8) == (j / 8) + (j % 8)) return true;
        return false;
    }

    /// <summary>
    /// Forward bitscans through all the set bits in the given bitboard.
    /// </summary>
    public static void ForEachBitscanForward(ulong bitboard, Action<int> operation)
    {
        ulong bitboardLSB = IsolateLSB(bitboard);
        while(bitboardLSB != 0)
        {
            operation(IndexOfLSB(bitboardLSB));
            bitboard &= ~bitboardLSB;
            bitboardLSB = IsolateLSB(bitboard);
        }
    }

    /// <summary>
    /// Returns a bitboard with only the bit at the index of the least significant bit in the argument bitboard set.
    /// </summary>
   public static ulong IsolateLSB(ulong bitboard)
    {
        return bitboard & ~(bitboard-1);
    }

    /// <summary>
    /// Returns the index of the least significant bit in the given bitboard
    /// </summary>
    public static int IndexOfLSB(ulong bitboard)
    {
        return BitOperations.TrailingZeroCount(bitboard);
    }

    /// <summary>
    /// Checks if the bit at the given index is set (i.e. is 1).
    /// </summary>
    public static bool IsBitSet(ulong bitboard, int index)
    {
        if (((bitboard >> index) & 1)==1) return true;
        return false;
    }

    /// <summary>
    /// Returns a new bitboard which has no bits set in positions where there are set bits in the mask.
    /// </summary>
    public static ulong ExcludeBitsUsingMask(ulong bitboard, ulong mask)
    {
        return bitboard ^ (mask & bitboard);
    }

    /// <summary>
    /// Returns the result of performing a bitwise operation on an array of elements, starting at index 0 (inclusive) and ending at index k (exclusive).
    /// </summary>
    public static ulong BitwiseOverArray(ulong[] bitboards, int k, Func<ulong, ulong, ulong> operation)
    {
        ulong result = bitboards[0];
        for (int i = 0; i < k; i++)
        {
            result = operation(result, bitboards[i]);
        }

        return result;
    }

    /// <summary>
    /// Returns the result of performing a bitwise OR operation on an array of elements, starting at index 0 (inclusive) and ending at index k (exclusive).
    /// </summary>
    public static ulong OR(ulong[] bitboards, int k)
    {
        ulong result = bitboards[0];
        for (int i = 1; i < k; i++)
        {
            result &= bitboards[i];
        }
        return result;
    }

    /// <summary>
    /// Returns the result of performing a bitwise AND operation on an array of elements, starting at index 0 (inclusive) and ending at index k (exclusive).
    /// </summary>
    public static ulong AND(ulong[] bitboards, int k)
    {
        ulong result = bitboards[0];
        for (int i = 1; i < k; i++)
        {
            result &= bitboards[i];
        }
        return result;
    }

    /// <summary>
    /// Returns the number of set bits in the bitboard.
    /// i.e. the number of 1s
    /// </summary>
    public static int CountSetBits(ulong bitboard)
    {
        int count = 0;
        ForEachBitscanForward(bitboard, (lsb) => {
            count++;
        });
        return count;
    }
}
