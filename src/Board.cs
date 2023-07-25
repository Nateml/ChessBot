using System.Net.WebSockets;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics.X86;
using System.Xml.XPath;

sealed class Board
{

    private ulong[] bitboards = new ulong[13];

    private long epFile;
    private bool CWK, CWQ, CBQ, CBK;

    private bool isWhiteToMove;

    MoveGen moveGen;

    List<IBoardListener> listeners;

    public readonly string FenStartingPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    /// <summary>
    /// Creates a board instance, using the specified fen string as the starting position.
    /// </summary>
    /// <param name="fen">the fen string specifying the position of the board</param>
    public Board(String fen)
    {
        bitboards[12] = ulong.MaxValue; // sentinal value
        moveGen = new MoveGen(this);
        listeners = new();
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

    public bool IsWhiteToMove { get; }

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

    /// <summary>
    /// The bitboard containing the locations of all white pieces.
    /// </summary>
    public ulong WhitePiecesBitboard
    {
        get
        {
            return bitboards[(int)PieceType.WP] | bitboards[(int)PieceType.WN] | bitboards[(int)PieceType.WB] | bitboards[(int)PieceType.WR] | bitboards[(int)PieceType.WQ] | bitboards[(int)PieceType.WK];
        }
    }

    /// <summary>
    /// The bitboard containing the locations of all black pieces.
    /// </summary>
    public ulong BlackPiecesBitboard
    {
        get
        {
            return bitboards[(int)PieceType.BP] | bitboards[(int)PieceType.BN] | bitboards[(int)PieceType.BB] | bitboards[(int)PieceType.BR] | bitboards[(int)PieceType.BQ] | bitboards[(int)PieceType.BK];
        }
    }

    /// <summary>
    /// The bitboard containing the locations of all pieces on the board.
    /// </summary>
    public ulong AllPiecesBitboard
    {
        get
        {
            return BitboardUtility.BitwiseOverArray(bitboards, bitboards.Length-1, (b1, b2) => b1 | b2);
        }
    }

    public ulong GetBitboardByPieceType(PieceType pieceType)
    {
        return bitboards[(int)pieceType];
    }

    public Move[] GetLegalMoves()
    {
        return moveGen.GenerateLegalMoves().ToArray();
    }

    /// <summary>
    /// Makes a given move, updates the appropriate bitboards and other state information.
    /// Notifies listeners of a state change.
    /// </summary>
    public void MakeMove(Move move)
    {
        // TODO: Implement Method

        // Notify listeners of the state change
        listeners.ForEach(listener => listener.OnBoardStateChange());
    }

    /// <summary>
    /// Returns the type of piece on the tile at the given index.
    /// </summary>
    public PieceType GetPieceType(int index)
    {
        ulong positionBitboard = 1ul << index;
        for (int i = 0;; i++)
        {
            if ((bitboards[i] & positionBitboard) != 0) return (PieceType)i;
        }
    }

    public void AttachListener(IBoardListener listener)
    {
        listeners.Add(listener);
    }

}