using System.Linq.Expressions;
using System.Numerics;

static class BitboardUtility
{
    public readonly static ulong[] FileMasks = {
        0x101010101010101L, 0x202020202020202L, 0x404040404040404L, 0x808080808080808L,
        0x1010101010101010L, 0x2020202020202020L, 0x4040404040404040L, 0x8080808080808080L
    };

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

}
