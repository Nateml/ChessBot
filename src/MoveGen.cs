class MoveGen : IBoardListener
{
    // Maintain an instance of Board for appropriate move generation
    Board board;

    private List<Move> legalMoves;
    private bool hasCachedLegalMoves;
    
    public MoveGen(Board board)
    {
        this.board = board;

        legalMoves = new();
        hasCachedLegalMoves = false;

        board.AttachListener(this);

        MoveGenData.Init();
    }

    /// <summary>
    /// Generates all legal moves in the current board position.
    /// </summary>
    public List<Move> GenerateLegalMoves()
    {
        // Return the cache if available
        if (hasCachedLegalMoves) return legalMoves;

        if (board.IsWhiteToMove)
        {
            return GenerateWhiteLegalMoves();
        }
        else
        {
            return GenerateBlackLegalMoves();
        }
    }

    private List<Move> GenerateWhiteLegalMoves()
    {
        ulong blockerBitboard = board.AllPiecesBitboard;

        List<Move> moves = new();

        // Generate rook moves
        GenerateRookMoves(moves, blockerBitboard, true);

        return moves;
    }

    private List<Move> GenerateBlackLegalMoves()
    {
        ulong blockerBitboard = board.AllPiecesBitboard;

        List<Move> moves = new();

        // Generate moves for each piece type here
        
        return moves;
    }

    /// <summary>
    /// Generates a list of legal rook moves for the given coloured played.
    /// NOTE: the blocker bitboard that is passed as an argument naturally included the rooks that we are generating moves for,
    ///         so the LSB of the rook we are generating moves for must be XORed with the blocker bitboard before using the lookup table.
    ///         (a piece cannot be a blocker of itself)
    /// </summary>
    public List<Move> GenerateRookMoves(List<Move> moves, ulong blockerBitboard, bool white)
    {
        ulong rookBitboard = board.GetBitboardByPieceType(white ? PieceType.WR : PieceType.BR);

        ulong rookLSB = BitboardUtility.IsolateLSB(rookBitboard);

        while (rookLSB != 0)
        {
            int startingIndex = BitboardUtility.IndexOfLSB(rookLSB);
            ulong targets = MoveGenData.OrthogonalMovesLookupTable[(startingIndex, blockerBitboard^rookLSB)];

            ulong targetsLSB = BitboardUtility.IsolateLSB(targets);
            while (targetsLSB != 0)
            {
                // Do not add the move if the move lands on a friendly piece
                if ((targetsLSB & (white ? board.WhitePiecesBitboard : board.BlackPiecesBitboard)) == 0)
                {
                    int targetIndex = BitboardUtility.IndexOfLSB(targetsLSB);
                    int flag = Move.QuietMoveFlag;

                    // Check if this is a capturing move
                    if ((targetsLSB & (white ? board.BlackPiecesBitboard : board.WhitePiecesBitboard)) != 0) flag = Move.CaptureFlag;

                    moves.Add(new Move(startingIndex, targetIndex, white ? PieceType.WR : PieceType.BR, board.GetPieceType(targetIndex), flag));
                }
            }
        }

        return moves;
    }

    public void OnBoardStateChange()
    {
        legalMoves = new();
        hasCachedLegalMoves = false;
    }
}