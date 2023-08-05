namespace ChessBot;

using static PieceType;

public static class MoveUtility
{

    /// <summary>
    /// Returns the colour of a piece.
    /// 0 for white, 1 for black
    /// </summary>
    public static int GetPieceColour(PieceType piece)
    {
        return piece switch
        {
            WP or WN or WB or WR or WQ or WK => 0,
            BP or BN or BB or BR or BQ or BK => 1,
            _ => -1,
        };
    }

    public static Move ConvertFromAlgebraic(string algebraicMove, Board board)
    {
        algebraicMove = algebraicMove.Trim();

        // from:
        char fileFrom = algebraicMove[0];
        char rankFrom = algebraicMove[1];

        int from = PositionConverter(fileFrom, rankFrom);

        // to:
        char fileTo = algebraicMove[2];
        char rankTo = algebraicMove[3];

        int to = PositionConverter(fileTo, rankTo);

        PieceType movingPiece = board.GetPieceType(from);
        PieceType capturedPiece = board.GetPieceType(to);

        int flag = 0;

        if (capturedPiece != EMPTY)
        {
            flag = Move.CaptureFlag;
        }

        if (movingPiece == WP || movingPiece == BP)
        {
            if (((1ul << from) & MoveGenData.RankMasks[1]) != 0 || ((1ul << from) & MoveGenData.RankMasks[6]) != 0)
            {
                if (((1ul << to) & MoveGenData.RankMasks[3]) != 0 || ((1ul << to) & MoveGenData.RankMasks[4]) != 0)
                {
                    flag = Move.DoublePawnPushFlag;
                }
            }
        }

        if (board.GetPieceType(from) == WK)
        {
            if (from == 60 && to == 62)
            {
                flag = Move.KingCastleFlag;
            }
            else if (from == 60 && to == 58)
            {
                flag = Move.QueenCastleFlag;
            }
        }
        else if (board.GetPieceType(from) == BK)
        {
            if (from == 4 && to == 6)
            {
                flag = Move.KingCastleFlag;
            }
            else if (from == 4 && to == 2)
            {
                flag = Move.QueenCastleFlag;
            }
        }

        if (algebraicMove.Length == 5)
        {
            switch (char.ToLower(algebraicMove[4]))
            {
                case 'q':
                    flag |= Move.QueenPromotionFlag;
                    break;
                case 'r':
                    flag |= Move.RookPromotionFlag;
                    break;
                case 'b':
                    flag |= Move.BishopPromotionFlag;
                    break;
                case 'n':
                    flag |= Move.KnightPromotionFlag;
                    break;
            }
        }

        return new Move(from, to, movingPiece, capturedPiece, flag);
    }

    public static int PositionConverter(char file, char rank)
    {
        int pos = (8-(int)char.GetNumericValue(rank))*8 + ((int)file - 97);
        return pos;
    }
}