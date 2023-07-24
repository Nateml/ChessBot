class Move
{
    /// <summary>
    /// A bunch of flags for moves (do not exceed 4 bits).
    /// NOTE: All captures have the 3rd bit set, and all promotions have the 4th bit set.
    /// </summary>
    public static readonly int QuietMoveFlag = 0;
    public static readonly int DoublePawnPushFlag = 0b1;
    public static readonly int KingCastleFlag = 0b10;
    public static readonly int QueenCastleFlag = 0b11;
    public static readonly int CaptureFlag = 0b100;
    public static readonly int EpCaptureFlag = 0b101;
    public static readonly int KnightPromotionFlag = 0b1000;
    public static readonly int BishopPromotionFlag = 0b1001;
    public static readonly int RookPromotionFlag = 0b1010;
    public static readonly int QueenPromotionFlag = 0b1011;
    public static readonly int KnightPromoCaptureFlag = 0b1100;
    public static readonly int BishopPromoCaptureFlag = 0b1101;
    public static readonly int RookPromoCaptureFlag = 0b1110;
    public static readonly int QueenPromoCaptureFlag = 0b1111;

    private readonly int encodedMove = 0;

    public Move(int from, int to, PieceType movingPiece, PieceType capturePiece, int flag)
    {
        encodedMove |= ((flag & 0xf) << 18) | (((int)capturePiece & 0xf) << 14) | (((int)movingPiece & 0xf) << 10) | ((from & 0x3f) << 6) | (to & 0x3f);
    }

    /// <summary>
    /// Returns the piece being captured, if any.
    /// NOTE: If no piece is being captured, this will return PieceType.PAWN, so make sure there is a capture by calling the isCapture() method.
    /// </summary>
    public PieceType CapturedPiece 
    {
        get 
        {
            return (PieceType) ((encodedMove >> 14) & 0xf);
        }
    }

    /// <summary>
    /// Returns the piece being moved.
    /// </summary>
    public PieceType MovingPiece 
    {
        get 
        {
            return (PieceType) ((encodedMove >> 10) & 0xf);
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
    private int Flag
    {
        get
        {
            return (encodedMove >> 18) & 0xf;
        }
    }
}