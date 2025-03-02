using System.ComponentModel;

namespace ChessBot;
class FenBuilder
{
    public static string BuildFen(Board board)
    {
        string fen = "";
        
        int empties = 0;
        for (int i = 0; i < 64; i++)
        {
            PieceType piece = board.GetPieceType(i);
            switch (piece)
            {
                case PieceType.WP:
                    if (empties != 0)
                    {
                        fen += empties;
                        empties = 0;
                    }
                    fen += "P";
                    break;
                case PieceType.WN:
                    if (empties != 0)
                    {
                        fen += empties;
                        empties = 0;
                    }
                    fen += "N";
                    break;
                case PieceType.WB:
                    if (empties != 0)
                    {
                        fen += empties;
                        empties = 0;
                    }
                    fen += "B";
                    break;
                case PieceType.WR:
                    if (empties != 0)
                    {
                        fen += empties;
                        empties = 0;
                    }
                    fen += "R";
                    break;
                case PieceType.WQ:
                    if (empties != 0)
                    {
                        fen += empties;
                        empties = 0;
                    }
                    fen += "Q";
                    break;
                case PieceType.WK:
                    if (empties != 0)
                    {
                        fen += empties;
                        empties = 0;
                    }
                    fen += "K";
                    break;
                case PieceType.BP:
                    if (empties != 0)
                    {
                        fen += empties;
                        empties = 0;
                    }
                    fen += "p";
                    break;
                case PieceType.BN:
                    if (empties != 0)
                    {
                        fen += empties;
                        empties = 0;
                    }
                    fen += "n";
                    break;
                case PieceType.BB:
                    if (empties != 0)
                    {
                        fen += empties;
                        empties = 0;
                    }
                    fen += "b";
                    break;
                case PieceType.BR:
                    if (empties != 0)
                    {
                        fen += empties;
                        empties = 0;
                    }
                    fen += "r";
                    break;
                case PieceType.BQ:
                    if (empties != 0)
                    {
                        fen += empties;
                        empties = 0;
                    }
                    fen += "q";
                    break;
                case PieceType.BK:
                    if (empties != 0)
                    {
                        fen += empties;
                        empties = 0;
                    }
                    fen += "k";
                    break;
                case PieceType.EMPTY:
                    empties++;
                    break;

            }

            if (i % 8 == 7) 
            {
                if (empties != 0) 
                {
                    fen += empties;
                    empties = 0;
                }
                if (i != 63)
                {
                    fen += '/';
                }
            }

        }

        fen += board.IsWhiteToMove ? " w " : " b ";

        if (board.CanWhiteCastleKingside())
        {
            fen += "K";
        }
        if (board.CanWhiteCastleQueenside())
        {
            fen += "Q";
        }
        if (board.CanBlackCastleKingside())
        {
            fen += "k";
        }
        if (board.CanBlackCastleQueenside())
        {
            fen += "q";
        }
        if (!(board.CanBlackCastleQueenside() || board.CanBlackCastleKingside() || board.CanWhiteCastleKingside() | board.CanWhiteCastleQueenside()))
        {
            fen += "-";
        }

        fen += " ";

        if (MoveGenData.FileMasks[board.EpFile] != 0)
        {
            if (board.IsWhiteToMove)
            {
                fen += ((Square) BitboardUtility.IndexOfLSB(MoveGenData.RankMasks[4] & MoveGenData.FileMasks[board.EpFile])).ToString().ToLower();
            }
            else
            {
                fen += ((Square) BitboardUtility.IndexOfLSB(MoveGenData.RankMasks[3] & MoveGenData.FileMasks[board.EpFile])).ToString().ToLower();
            }
        }
        else
        {
            fen += "-";
        }

        fen += " ";

        fen += board.NumPlySincePawnMoveOrCapture + " ";

        fen += board.FullMoveCount;

        return fen;
    }
}