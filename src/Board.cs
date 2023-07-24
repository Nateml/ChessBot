sealed class Board
{

    private ulong[] bitboards = new ulong[13];

    private long epFile;
    private bool CWK, CWQ, CBQ, CBK;

    MoveGen moveGen;

    public readonly string FenStartingPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    /// <summary>
    /// Creates a board instance, using the specified fen string as the starting position.
    /// </summary>
    /// <param name="fen">the fen string specifying the position of the board</param>
    public Board(String fen)
    {
        bitboards[12] = ulong.MaxValue; // sentinal value
        moveGen = new MoveGen(this);
        LoadPositionFromFen(fen);
    }

    /// <summary>
    /// Sets up the board according to the given fen string.
    /// </summary>
    public void LoadPositionFromFen(String fen)
    {

    }

    /// <summary>
    /// Returns all the piece bitboards as an array, indexed by their PieceType enum.
    /// </summary>
    public ulong[] Bitboards { get; }

    /// <summary>
    /// Returns the file mask of the current file where an en-passant is possible, or zero if there is no legal en-passant.
    /// </summary>
    public long EpFile { get; }

    /// <summary>
    /// Returns true if white has kingside castling permission, false otherwise.
    /// </summary>
    public bool CanWhiteCastleKingside() { return CWK;}

    /// <summary>
    /// Returns true if white has queenside castling permission, false otherwise.
    /// </summary>
    public bool CanWhiteCastleQueenside() { return CWQ; }

    /// <summary>
    /// Returns true if black has kingside castling permission, false otherwise.
    /// </summary>
    public bool CanBlackCastleKingside() { return CBK; }

    /// <summary>
    /// Returns true if black has queenside castling permission, false otherwise.
    /// </summary>
    public bool CanBlackCastleQueenside() { return CBQ; }

    public Move[] GetLegalMoves()
    {
        return moveGen.GenerateLegalMoves();
    }

}