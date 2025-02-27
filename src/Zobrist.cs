namespace ChessBot;

using System.Net;
using System.Security.Cryptography;
using ChessBot;

public static class Zobrist
{
    public static readonly ulong[][] zArray = new ulong[12][];
    public static readonly ulong[] zEnPassant = new ulong[8];
    public static readonly ulong[] zCastle = new ulong[4];
    public static readonly ulong zBlackMove;

    private static RandomNumberGenerator rNumGenerator = RandomNumberGenerator.Create();

    static Zobrist()
    {
        for (int pieceType = 0; pieceType < 12; pieceType++)
        {
            zArray[pieceType] = new ulong[64];
            for (int i = 0; i < 64; i++)
            {
                zArray[pieceType][i] = GetRandom64();
            }
        }

        for (int file = 0; file < 8; file++)
        {
            zEnPassant[file] = GetRandom64();
        }

        for (int i = 0; i < 4; i++)
        {
           zCastle[i] = GetRandom64(); 
        }

        zBlackMove = GetRandom64();
    }

    public static ulong GetZobristHash(Board board)
    {
        ulong zKey = 0ul;
        for (int square = 0; square < 64; square++)
        {
            PieceType piece = board.GetPieceType(square);
            switch (piece)
            {
                case PieceType.WP:
                    zKey ^= zArray[(int)PieceType.WP][square];
                    break;
                case PieceType.WN:
                    zKey ^= zArray[(int)PieceType.WN][square];
                    break;
                case PieceType.WB:
                    zKey ^= zArray[(int)PieceType.WB][square];
                    break;
                case PieceType.WR:
                    zKey ^= zArray[(int)PieceType.WR][square];
                    break;
                case PieceType.WQ:
                    zKey ^= zArray[(int)PieceType.WQ][square];
                    break;
                case PieceType.WK:
                    zKey ^= zArray[(int)PieceType.WK][square];
                    break;
                case PieceType.BP:
                    zKey ^= zArray[(int)PieceType.BP][square];
                    break;
                case PieceType.BN:
                    zKey ^= zArray[(int)PieceType.BN][square];
                    break;
                case PieceType.BB:
                    zKey ^= zArray[(int)PieceType.BB][square];
                    break;
                case PieceType.BR:
                    zKey ^= zArray[(int)PieceType.BR][square];
                    break;
                case PieceType.BQ:
                    zKey ^= zArray[(int)PieceType.BQ][square];
                    break;
                case PieceType.BK:
                    zKey ^= zArray[(int)PieceType.BK][square];
                    break;
            }
        }

        if (board.EpFile < 8)
        {
            zKey ^= zEnPassant[board.EpFile];
        }

        if (board.CanWhiteCastleKingside()) zKey ^= zCastle[0];
        if (board.CanWhiteCastleQueenside()) zKey ^= zCastle[1];
        if (board.CanBlackCastleKingside()) zKey ^= zCastle[2];
        if (board.CanBlackCastleQueenside()) zKey ^= zCastle[3];

        if (!board.IsWhiteToMove) zKey ^= zBlackMove;

        return zKey;
    }

    private static ulong GetRandom64()
    {
        var bytes = new byte[sizeof(ulong)];
        rNumGenerator.GetBytes(bytes);
        return BitConverter.ToUInt64(bytes, 0);
    }
}