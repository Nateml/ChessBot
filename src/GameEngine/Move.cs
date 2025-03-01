namespace ChessBot;

public class Move
{
    /// <summary>
    /// A bunch of flags for moves (do not exceed 4 bits).
    /// NOTE: All captures have the 3rd bit set, and all promotions have the 4th bit set.
    /// </summary>
    public const int QuietMoveFlag = 0;
    public const int DoublePawnPushFlag = 0b1;
    public const int KingCastleFlag = 0b10;
    public const int QueenCastleFlag = 0b11;
    public const int CaptureFlag = 0b100;
    public const int EpCaptureFlag = 0b101;
    public const int KnightPromotionFlag = 0b1000;
    public const int BishopPromotionFlag = 0b1001;
    public const int RookPromotionFlag = 0b1010;
    public const int QueenPromotionFlag = 0b1011;
    public const int KnightPromoCaptureFlag = 0b1100;
    public const int BishopPromoCaptureFlag = 0b1101;
    public const int RookPromoCaptureFlag = 0b1110;
    public const int QueenPromoCaptureFlag = 0b1111;

    private readonly int encodedMove = 0;

    private int score = 0;
    private bool hasCachedScore = false;

    public Move(int from, int to, PieceType movingPiece, PieceType capturePiece, int flag)
    {
        encodedMove |= ((flag & 0xf) << 20) | (((int)capturePiece & 0xf) << 16) | (((int)movingPiece & 0xf) << 12) | ((from & 0x3f) << 6) | (to & 0x3f);
    }

    /// <summary>
    /// Returns the piece being captured, if any.
    /// NOTE: If no piece is being captured, this will return PieceType.PAWN, so make sure there is a capture by calling the isCapture() method.
    /// </summary>
    public PieceType CapturedPiece 
    {
        get 
        {
            return (PieceType) ((encodedMove >> 16) & 0xf);
        }
    }

    /// <summary>
    /// Returns the piece being moved.
    /// </summary>
    public PieceType MovingPiece 
    {
        get 
        {
            return (PieceType) ((encodedMove >> 12) & 0xf);
        }
    }

    /// <summary>
    /// Returns the starting square of the move as an integer correponding the the index of the square in the bitboard representation of the board.
    /// </summary>
    public int From 
    {
        get 
        {
            return (encodedMove >> 6) & 0x3f;
        }
    }

    /// <summary>
    /// Returns the target square of the move as an integer correponding the the index of the square in the bitboard representation of the board.
    /// </summary>
    public int To
    {
        get
        {
            return encodedMove & 0x3f;
        }
    }

    public bool IsNullMove
    {
        get
        {
            return From == 0 && To == 0;
        }
    }

    /// <summary>
    /// Returns true if this move captures a piece.
    /// </summary>
    public bool IsCapture()
    {
        return (Flag & CaptureFlag) == CaptureFlag;
    }

    /// <summary>
    /// Returns true if this move is a promoting move.
    /// </summary>
    public bool IsPromotion()
    {
        return (Flag & 0b1000) == 0b1000;
    }

    /// <summary>
    /// Returns true if this move is a queen promotion move.
    /// </summary>
    public bool IsQueenPromotion()
    {
        return Flag == QueenPromotionFlag | Flag == QueenPromoCaptureFlag;
    }

    /// <summary>
    /// Returns true if this move is a knight promotion move.
    /// </summary>
    public bool IsKnightPromotion()
    {
        return Flag == KnightPromotionFlag | Flag == KnightPromoCaptureFlag;
    }

    /// <summary>
    /// Returns true if this move is a bishop promotion move.
    /// </summary>
    public bool IsBishopPromotion()
    {
        return Flag == BishopPromotionFlag | Flag == BishopPromoCaptureFlag;
    }

    /// <summary>
    /// Returns true if this move is a rook promotion move.
    /// </summary>
    public bool IsRookPromotion()
    {
        return Flag == RookPromotionFlag | Flag == RookPromoCaptureFlag;
    }

    /// <summary>
    /// Returns true if this move is an en passant move.
    /// </summary>
    public bool IsEnPassant()
    {
        return Flag == EpCaptureFlag;
    }

    /// <summary>
    /// Returns true if this move is a kingside castling move.
    /// </summary>
    public bool IsKingsideCastle()
    {
        return Flag == KingCastleFlag;
    }

    /// <summary>
    /// Returns true if this move is a queenside castling move.
    /// </summary>
    public bool IsQueensideCastle()
    {
        return Flag == QueenCastleFlag;
    }

    /// <summary>
    /// Returns true if this move is a double pawn push move.
    /// </summary>
    public bool IsDoublePawnPush()
    {
        return Flag == DoublePawnPushFlag;
    }

    /// <summary>
    /// Returns true if this move is a quiet move.
    /// </summary>
    public bool IsQuietMove()
    {
        return Flag == QuietMoveFlag;
    }

    /// <summary>
    /// Returns the flag of the move.
    /// </summary>
    public int Flag
    {
        get
        {
            return (encodedMove >> 20) & 0xf;
        }
    }

    public int EncodedMove => encodedMove;

    /// <summary>
    /// Returns true if the given move has the same To and From values as this move.
    /// </summary>
    public bool Equals(Move? move)
    {
        return move != null && move.To == To && move.From == From;
    }

    /// <summary>
    /// Returns the type of piece this move is promoting to.
    /// If this move is not a promoting move, returns null.
    /// NOTE: Always returns a white piece type for simplicity. 
    /// </summary>
    /// <returns></returns>
    public PieceType? GetPromotionType()
    {
        return Flag switch
        {
            KnightPromotionFlag or KnightPromoCaptureFlag => (PieceType?)PieceType.WN,
            BishopPromotionFlag or BishopPromoCaptureFlag => (PieceType?)PieceType.WB,
            RookPromotionFlag or RookPromoCaptureFlag => (PieceType?)PieceType.WR,
            QueenPromotionFlag or QueenPromoCaptureFlag => (PieceType?)PieceType.WQ,
            _ => null,
        };
    }

    public override string ToString()
    {
        string output = ((Square)From).ToString().ToLower() + ((Square)To).ToString().ToLower();

        PieceType? promotedPiece = GetPromotionType();
        if (promotedPiece != null)
        {
            switch (promotedPiece)
            {
                case PieceType.WN:
                    output += "n";
                    break;
                case PieceType.WB:
                    output += "b";
                    break;
                case PieceType.WR:
                    output += "r";
                    break;
                case PieceType.WQ:
                    output += "q";
                    break;
            }
        }

        return output;
    }

    public int GetScore(Board board, KillerMoves killerMoves, int distanceFromRoot, int gamePhase)
    {
        if (hasCachedScore) return score;

        score = Evaluation.EvaluateMove(this, board, killerMoves, distanceFromRoot, gamePhase);
        hasCachedScore = true;

        return score;
    }

    public static Move MakeNullMove()
    {
        return new Move(0, 0, PieceType.EMPTY, PieceType.EMPTY, 0);
    }
}