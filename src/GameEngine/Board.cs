namespace ChessBot;

using System.ComponentModel;
using System.Net.WebSockets;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics.X86;
using System.Xml.XPath;

public sealed class Board
{

    private ulong[] bitboards = new ulong[13];

    private ulong allPiecesBitboard = 0ul;

    private HashSet<ulong> repetitionHistory = new();

    private int numPlySincePawnMoveOrCapture = 0;

    private byte epFile; 
    private bool CWK, CWQ, CBQ, CBK;

    private bool isWhiteToMove;

    private ulong zobristHash;

    // CACHE:
    private ulong whiteAttackBitboard;
    private bool hasCachedWhiteAttackBitboard;
    private ulong blackAttackBitboard;
    private bool hasCachedBlackAttackBitboard;
    private ulong whiteUnsafeKingSquares;
    private bool hasCachedWhiteUnsafeKingSquares;
    private ulong blackUnsafeKingSquares;
    private bool hasCachedBlackUnsafeKingSquares;
    private bool isKingInCheck;
    private bool hasCachedIsKingInCheck;
    private ulong kingAttackers;
    private bool hasCachedKingAttackers;

    private Stack<StateData> stateHistory = new();

    public int fullMoveCount;
    public int halfMoveCount;

    public MoveGen moveGen;

    List<IBoardListener> listeners = new();

    public const string FenStartingPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    //public const string FenStartingPosition = "rnbqkbnr/ppp1pppp/8/3p4/2P5/8/PP1PPPPP/RNBQKBNR w KQkq d5 1 2";


    /// <summary>
    /// Creates a board instance, using the specified fen string as the starting position.
    /// </summary>
    /// <param name="fen">the fen string specifying the position of the board</param>
    public Board(string fen = FenStartingPosition)
    {
        bitboards[12] = ulong.MaxValue; // sentinal value
        listeners = new();
        halfMoveCount = 0;
        fullMoveCount = 0;
        LoadPositionFromFen(fen);
        moveGen = new MoveGen(this);
    }

    /// <summary>
    /// Sets up the board according to the given fen string.
    /// </summary>
    public void LoadPositionFromFen(string fen)
    {
        FenParser fenParser = new(fen);
        bitboards[(int)PieceType.WP] = fenParser.WP;
        bitboards[(int)PieceType.WN] = fenParser.WN;
        bitboards[(int)PieceType.WB] = fenParser.WB;
        bitboards[(int)PieceType.WR] = fenParser.WR;
        bitboards[(int)PieceType.WQ] = fenParser.WQ;
        bitboards[(int)PieceType.WK] = fenParser.WK;
        bitboards[(int)PieceType.BP] = fenParser.BP;
        bitboards[(int)PieceType.BN] = fenParser.BN;
        bitboards[(int)PieceType.BB] = fenParser.BB;
        bitboards[(int)PieceType.BR] = fenParser.BR;
        bitboards[(int)PieceType.BQ] = fenParser.BQ;
        bitboards[(int)PieceType.BK] = fenParser.BK;

        allPiecesBitboard = BitboardUtility.BitwiseOverArray(bitboards, bitboards.Length-1, (b1, b2) => b1 | b2);

        epFile = fenParser.epFile;

        CWK = fenParser.cwk;
        CWQ = fenParser.cwq;
        CBK = fenParser.cbk;
        CBQ = fenParser.cbq;

        halfMoveCount = fenParser.halfMoveCount;
        fullMoveCount = fenParser.fullMoveCount;

        isWhiteToMove = fenParser.isWhiteToMove;

        zobristHash = Zobrist.GetZobristHash(this);
    }

    /// <summary>
    /// Makes a given move, updates the appropriate bitboards and other state information.
    /// Notifies listeners of a state change.
    /// </summary>
    public void MakeMove(Move move)
    {

        // Because I am getting some bugs, I am going to do a quick check if there is a piece on the from square
        if ((bitboards[(int)move.MovingPiece] & (1ul << move.From)) == 0)
        {
            // Print out some debug information
            Console.WriteLine("From square: " + move.From);
            Console.WriteLine("To square: " + move.To);
            Console.WriteLine("Moving piece: " + move.MovingPiece);
            Console.WriteLine("Captured piece: " + move.CapturedPiece);
            Console.WriteLine("Flag: " + move.Flag);
            Console.WriteLine("Is capture: " + move.IsCapture());
            Console.WriteLine("Is en passant: " + move.IsEnPassant());
            Console.WriteLine("Is promotion: " + move.IsPromotion());
            Console.WriteLine("Is kingside castle: " + move.IsKingsideCastle());
            Console.WriteLine("Is queenside castle: " + move.IsQueensideCastle());
            Console.WriteLine("Is double pawn push: " + move.IsDoublePawnPush());
            Console.WriteLine("Is quiet move: " + move.IsQuietMove());
            Console.WriteLine("Encoded move: " + move.EncodedMove);
            Console.WriteLine("Move: " + move);
            BitboardUtility.PrintBitboard(bitboards[(int)move.MovingPiece]);
            throw new Exception("No piece on the from square");
        }

        //repetitionHistory.Add(zobristHash);

        // Push state data to stack before making the move
        stateHistory.Push(new StateData(move, CWK, CWQ, CBK, CBQ, epFile, fullMoveCount, halfMoveCount, zobristHash, numPlySincePawnMoveOrCapture));

        numPlySincePawnMoveOrCapture++;

        int to = move.To;
        int from = move.From;

        ulong fromBitboard = 1ul << from;
        ulong toBitboard = 1ul << to;

        PieceType movingPiece = move.MovingPiece;
        PieceType capturedPiece = move.CapturedPiece;

        bool white = MoveUtility.GetPieceColour(movingPiece) == 0;

        ulong newZobristHash = zobristHash;
        bool prevCWK = CWK;
        bool prevCWQ = CWQ;
        bool prevCBK = CBK;
        bool prevCBQ = CBQ;
        byte prevEnPasantFile = 8;
        prevEnPasantFile = epFile;
        // Clear en passant file
        epFile = 8;

        // Update the moving piece's bitboard

        bitboards[(int)movingPiece] ^= fromBitboard;
        newZobristHash ^= Zobrist.zArray[(int)movingPiece][from];

        allPiecesBitboard ^= fromBitboard;
        allPiecesBitboard |= toBitboard;

        if (!move.IsPromotion())
        {
            bitboards[(int)movingPiece] ^= toBitboard;
            newZobristHash ^= Zobrist.zArray[(int)movingPiece][to];
        }
        else
        {
            switch (move.Flag) {
                case Move.KnightPromoCaptureFlag:
                case Move.KnightPromotionFlag:
                    bitboards[isWhiteToMove ? (int)PieceType.WN : (int)PieceType.BN] ^= toBitboard;
                    newZobristHash ^= Zobrist.zArray[isWhiteToMove ? (int)PieceType.WN : (int)PieceType.BN][to];
                    break;
                case Move.BishopPromoCaptureFlag:
                case Move.BishopPromotionFlag:
                    bitboards[isWhiteToMove ? (int)PieceType.WB : (int)PieceType.BB] ^= toBitboard;
                    newZobristHash ^= Zobrist.zArray[isWhiteToMove ? (int)PieceType.WB : (int)PieceType.BB][to];
                    break;
                case Move.RookPromoCaptureFlag:
                case Move.RookPromotionFlag:
                    bitboards[isWhiteToMove ? (int)PieceType.WR : (int)PieceType.BR] ^= toBitboard;
                    newZobristHash ^= Zobrist.zArray[isWhiteToMove ? (int)PieceType.WR : (int)PieceType.BR][to];
                    break;
                case Move.QueenPromoCaptureFlag:
                case Move.QueenPromotionFlag:
                    bitboards[isWhiteToMove ? (int)PieceType.WQ : (int)PieceType.BQ] ^= toBitboard;
                    newZobristHash ^= Zobrist.zArray[isWhiteToMove ? (int)PieceType.WQ : (int)PieceType.BQ][to];
                    break;
            }
        }

        // Handle castling rights
        if (movingPiece == PieceType.WK)
        {
            CWK = false;
            CWQ = false;
            if (prevCWK)
            {
                newZobristHash ^= Zobrist.zCastle[0];
            }
            if (prevCWQ)
            {
                newZobristHash ^= Zobrist.zCastle[1];
            }
        }
        else if (movingPiece == PieceType.BK)
        {
            CBK = false;
            CBQ = false;
            if (prevCBK != CBK)
            {
                newZobristHash ^= Zobrist.zCastle[2];
            }
            if (prevCBQ != CBQ)
            {
                newZobristHash ^= Zobrist.zCastle[3];
            }
        }
        else
        {
            switch (from)
            {
                case 63:
                    CWK = false;
                    if (prevCWK) newZobristHash ^= Zobrist.zCastle[0];
                    break;
                case 56:
                    CWQ = false;
                    if (prevCWQ) newZobristHash ^= Zobrist.zCastle[1];
                    break;
                case 7:
                    CBK = false;
                    if (prevCBK) newZobristHash ^= Zobrist.zCastle[2];
                    break;
                case 0:
                    CBQ = false;
                    if (prevCBQ) newZobristHash ^= Zobrist.zCastle[3];
                    break;
            }
        } 

        if (move.IsCapture())
        {
            numPlySincePawnMoveOrCapture = 0;
            if (move.IsEnPassant())
            {
                bitboards[(int)capturedPiece] ^= isWhiteToMove ? toBitboard << 8 : toBitboard >> 8;
                allPiecesBitboard ^= isWhiteToMove ? toBitboard << 8 : toBitboard >> 8;
                newZobristHash ^= Zobrist.zArray[(int)capturedPiece][isWhiteToMove ? to + 8 : to - 8];
            }
            else
            {
                bitboards[(int)capturedPiece] ^= toBitboard;
                newZobristHash ^= Zobrist.zArray[(int)capturedPiece][to];
            }

            switch (to)
            {
                case 63:
                    CWK = false;
                    if (prevCWK != CWK) newZobristHash ^= Zobrist.zCastle[0];
                    break;
                case 56:
                    CWQ = false;
                    if (prevCWQ != CWQ) newZobristHash ^= Zobrist.zCastle[1];
                    break;
                case 7:
                    CBK = false;
                    if (prevCBK != CBK) newZobristHash ^= Zobrist.zCastle[2];
                    break;
                case 0:
                    CBQ = false;
                    if (prevCBQ != CBQ) newZobristHash ^= Zobrist.zCastle[3];
                    break;
            }
        }
        else if (movingPiece == PieceType.WP || movingPiece == PieceType.BP)
        {
            numPlySincePawnMoveOrCapture = 0;
            if (move.IsDoublePawnPush()) epFile = (byte) (to % 8); // Record the EP file
        }
        else if (move.IsKingsideCastle())
        {
            // Update the rook bitboard
            if (white)
            {
                bitboards[(int)PieceType.WR] ^= 0b101ul << 61;
                allPiecesBitboard ^= 0b101ul << 61;
                CWK = false;
                // Update Zobrist Hash
                // newZobristHash ^= Zobrist.zCastle[0]; // I would have already done this when checking if a piece moved from square 63
                newZobristHash ^= Zobrist.zArray[(int)PieceType.WR][63]; // Remove old rook position
                newZobristHash ^= Zobrist.zArray[(int)PieceType.WR][61]; // Add new rook position
            }
            else
            {
                bitboards[(int)PieceType.BR] ^= 0b10100000ul;
                allPiecesBitboard ^= 0b10100000ul;
                CBK = false;
                // Update Zobrist Hash
                // newZobristHash ^= Zobrist.zCastle[2];
                newZobristHash ^= Zobrist.zArray[(int)PieceType.BR][7]; // Remove old rook position
                newZobristHash ^= Zobrist.zArray[(int)PieceType.BR][5]; // Add new rook position
            }
        }
        else if (move.IsQueensideCastle())
        {
            // Update the rook bitboard
            if (white)
            {
                bitboards[(int)PieceType.WR] ^= 0b1001ul << 56;
                allPiecesBitboard ^= 0b1001ul << 56;
                CWQ = false;
                // newZobristHash ^= Zobrist.zCastle[1];
                newZobristHash ^= Zobrist.zArray[(int)PieceType.WR][56]; // Remove old rook position
                newZobristHash ^= Zobrist.zArray[(int)PieceType.WR][59]; // Add new rook position
            }
            else
            {
                bitboards[(int)PieceType.BR] ^= 0b1001ul;
                allPiecesBitboard ^= 0b1001ul;
                CBQ = false;
                // newZobristHash ^= Zobrist.zCastle[3];
                newZobristHash ^= Zobrist.zArray[(int)PieceType.BR][0]; // Remove old rook position
                newZobristHash ^= Zobrist.zArray[(int)PieceType.BR][3]; // Add new rook position
            }
        }

        // XOR in the new en-passant file
        if (epFile != 8) newZobristHash ^= Zobrist.zEnPassant[epFile];


        isWhiteToMove = !isWhiteToMove; // Toggle who's turn it is
        newZobristHash ^= Zobrist.zBlackMove;

        // Update state information
        if (halfMoveCount != 1 || ((halfMoveCount == 1) && IsWhiteToMove))
        {
            halfMoveCount++;
        }
        if (halfMoveCount % 2 == 0) fullMoveCount++;

        if (prevEnPasantFile != 8) {
            newZobristHash ^= Zobrist.zEnPassant[prevEnPasantFile];
        }

        zobristHash = newZobristHash;

        // Clear cached data
        ClearCaches();

        // Notify listeners of the state change
        listeners.ForEach(listener => listener.OnBoardStateChange());
    }

    public void UnmakeMove()
    {
        //repetitionHistory.Remove(zobristHash);

        StateData previousState = stateHistory.Pop();
        Move move = previousState.lastMove;

        ulong fromBitboard = 1ul << move.From;
        ulong toBitboard = 1ul << move.To;

        allPiecesBitboard ^= fromBitboard | toBitboard;

        // update the moving pieces bitboard
        bitboards[(int)move.MovingPiece]^= (toBitboard & bitboards[(int)move.MovingPiece]) | fromBitboard;

        switch(move.Flag)
        {
            case Move.KnightPromoCaptureFlag:
            case Move.KnightPromotionFlag:
                bitboards[isWhiteToMove ? (int)PieceType.BN : (int)PieceType.WN] ^= toBitboard;
                break;
            case Move.BishopPromoCaptureFlag:
            case Move.BishopPromotionFlag:
                bitboards[isWhiteToMove ? (int)PieceType.BB : (int)PieceType.WB] ^= toBitboard;
                break;
            case Move.RookPromoCaptureFlag:
            case Move.RookPromotionFlag:
                bitboards[isWhiteToMove ? (int)PieceType.BR : (int)PieceType.WR] ^= toBitboard;
                break;
            case Move.QueenPromoCaptureFlag:
            case Move.QueenPromotionFlag:
                bitboards[isWhiteToMove ? (int)PieceType.BQ : (int)PieceType.WQ] ^= toBitboard;
                break;
        }

        if (move.IsCapture())
        {
            if (move.IsEnPassant())
            {
                bitboards[(int)move.CapturedPiece] ^= isWhiteToMove ? toBitboard >> 8 : toBitboard << 8;
                allPiecesBitboard ^= isWhiteToMove ? toBitboard >> 8 : toBitboard << 8;
            }
            else
            {
                bitboards[(int)move.CapturedPiece] ^= toBitboard;
                allPiecesBitboard ^= toBitboard;
            }
        }
        else if (move.IsKingsideCastle())
        {
            // Update the rook bitboard
            if (isWhiteToMove)
            {
                bitboards[(int)PieceType.BR] ^= 0b10100000ul;
                allPiecesBitboard ^= 0b10100000ul;
            }
            else
            {
                bitboards[(int)PieceType.WR] ^= 0b101ul << 61;
                allPiecesBitboard ^= 0b101ul << 61;
            }
        }
        else if (move.IsQueensideCastle())
        {
            // Update the rook bitboard
            if (isWhiteToMove)
            {
                bitboards[(int)PieceType.BR] ^= 0b1001ul;
                allPiecesBitboard ^= 0b1001ul;
            }
            else
            {
                bitboards[(int)PieceType.WR] ^= 0b1001ul << 56;
                allPiecesBitboard ^= 0b1001ul << 56;
            }
        }

        isWhiteToMove = !isWhiteToMove;

        // Clear cached data
        ClearCaches();

        CWK = previousState.CWK;
        CWQ = previousState.CWQ;
        CBK = previousState.CBK;
        CBQ = previousState.CBQ;
        epFile = previousState.epFile;
        halfMoveCount = previousState.halfMoveCount;
        fullMoveCount = previousState.fullMoveCount;
        zobristHash = previousState.zobristHash;
        numPlySincePawnMoveOrCapture = previousState.numPlySincePawnMoveOrCapture;

        listeners.ForEach(listener => listener.OnBoardStateChange());
    }

    /// <summary>
    /// Returns all the piece bitboards as an array, indexed by their PieceType enum.
    /// </summary>
    public ulong[] Bitboards { get { return bitboards; } }

    public bool IsWhiteToMove { get { return isWhiteToMove; } }

    /// <summary>
    /// Returns the file number (starting from zero) of the file where an en-passant is possible, or 8 if there is no legal en-passant.
    /// </summary>
    public byte EpFile { get { return epFile; } }

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

    public ulong ZobristHash {
        get { return zobristHash; }
    }

    /// <summary>
    /// A HashSet of all the positions, represented by zobrist keys, which have appeared in the board's history.
    /// </summary>
    public HashSet<ulong> RepetitionHistory => repetitionHistory;

    public int WhiteKingSquare
    {
        get
        {
            return BitboardUtility.IndexOfLSB(GetBitboardByPieceType(PieceType.WK));
        }
    }

    public int BlackKingSquare
    {
        get
        {
            return BitboardUtility.IndexOfLSB(GetBitboardByPieceType(PieceType.BK));
        }
    }

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
            //return BitboardUtility.BitwiseOverArray(bitboards, bitboards.Length-1, (b1, b2) => b1 | b2);
            return allPiecesBitboard;
        }
    }

    public ulong WhiteAttackBitboard
    {
        get
        {
            if (hasCachedWhiteAttackBitboard) return whiteAttackBitboard;

            whiteAttackBitboard = 0ul;
            blackUnsafeKingSquares = 0ul;
            
            ulong wpOcc = GetBitboardByPieceType(PieceType.WP);
            ulong wnOcc = GetBitboardByPieceType(PieceType.WN);
            ulong wbOcc = GetBitboardByPieceType(PieceType.WB);
            ulong wrOcc = GetBitboardByPieceType(PieceType.WR);
            ulong wqOcc = GetBitboardByPieceType(PieceType.WQ);
            ulong wkOcc = GetBitboardByPieceType(PieceType.WK);

            BitboardUtility.ForEachBitscanForward(wpOcc, (square) => {
                whiteAttackBitboard |= MoveGenData.whitePawnAttacks[square];
            });

            BitboardUtility.ForEachBitscanForward(wnOcc, (square) => {
                whiteAttackBitboard |= MoveGenData.knightTargets[square];
            });

            BitboardUtility.ForEachBitscanForward(wbOcc, (square) => {
                whiteAttackBitboard |= MoveGenHelper.BishopAttacks(AllPiecesBitboard, square);
                blackUnsafeKingSquares |= MoveGenHelper.BishopAttacks(AllPiecesBitboard^GetBitboardByPieceType(PieceType.BK), square);
            });

            BitboardUtility.ForEachBitscanForward(wrOcc, (square) => {
                whiteAttackBitboard |= MoveGenHelper.RookAttacks(AllPiecesBitboard, square);
                blackUnsafeKingSquares |= MoveGenHelper.RookAttacks(AllPiecesBitboard^GetBitboardByPieceType(PieceType.BK), square);
            });

            BitboardUtility.ForEachBitscanForward(wqOcc, (square) => {
                whiteAttackBitboard |= MoveGenHelper.QueenAttacks(AllPiecesBitboard, square);
                blackUnsafeKingSquares |= MoveGenHelper.QueenAttacks(AllPiecesBitboard^GetBitboardByPieceType(PieceType.BK), square);
            });

            BitboardUtility.ForEachBitscanForward(wkOcc, (square) => {
                whiteAttackBitboard |= MoveGenData.kingTargets[square];
            });


            blackUnsafeKingSquares |= whiteAttackBitboard;

            hasCachedWhiteAttackBitboard = true;
            hasCachedBlackUnsafeKingSquares = true;
            return whiteAttackBitboard;
        }
    }

    public ulong WhiteUnsafeKingSquares
    {
        get
        {
            if (hasCachedWhiteUnsafeKingSquares) return whiteUnsafeKingSquares;

            whiteUnsafeKingSquares = 0ul;

            ulong bpOcc = GetBitboardByPieceType(PieceType.BP);
            ulong bnOcc = GetBitboardByPieceType(PieceType.BN);
            ulong bbOcc = GetBitboardByPieceType(PieceType.BB);
            ulong brOcc = GetBitboardByPieceType(PieceType.BR);
            ulong bqOcc = GetBitboardByPieceType(PieceType.BQ);
            ulong bkOcc = GetBitboardByPieceType(PieceType.BK);

            BitboardUtility.ForEachBitscanForward(bpOcc, (square) => {
                whiteUnsafeKingSquares |= MoveGenData.blackPawnAttacks[square];
            });

            BitboardUtility.ForEachBitscanForward(bnOcc, (square) => {
                whiteUnsafeKingSquares |= MoveGenData.knightTargets[square];
            });

            BitboardUtility.ForEachBitscanForward(bbOcc, (square) => {
                whiteUnsafeKingSquares |= MoveGenHelper.BishopAttacks(AllPiecesBitboard^GetBitboardByPieceType(PieceType.WK), square);
            });

            BitboardUtility.ForEachBitscanForward(brOcc, (square) => {
                whiteUnsafeKingSquares |= MoveGenHelper.RookAttacks(AllPiecesBitboard^GetBitboardByPieceType(PieceType.WK), square);
            });

            BitboardUtility.ForEachBitscanForward(bqOcc, (square) => {
                whiteUnsafeKingSquares |= MoveGenHelper.QueenAttacks(AllPiecesBitboard^GetBitboardByPieceType(PieceType.WK), square);
            });

            BitboardUtility.ForEachBitscanForward(bkOcc, (square) => {
                whiteUnsafeKingSquares |= MoveGenData.kingTargets[square];
            });

            hasCachedWhiteUnsafeKingSquares = true;
            return whiteUnsafeKingSquares;
        }
    }

    public ulong BlackAttackBitboard
    {
        get
        {
            if (hasCachedBlackAttackBitboard) return blackAttackBitboard;

            blackAttackBitboard = 0ul;
            whiteUnsafeKingSquares = 0ul;

            ulong bpOcc = GetBitboardByPieceType(PieceType.BP);
            ulong bnOcc = GetBitboardByPieceType(PieceType.BN);
            ulong bbOcc = GetBitboardByPieceType(PieceType.BB);
            ulong brOcc = GetBitboardByPieceType(PieceType.BR);
            ulong bqOcc = GetBitboardByPieceType(PieceType.BQ);
            ulong bkOcc = GetBitboardByPieceType(PieceType.BK);

            BitboardUtility.ForEachBitscanForward(bpOcc, (square) => {
                blackAttackBitboard |= MoveGenData.blackPawnAttacks[square];
            });

            BitboardUtility.ForEachBitscanForward(bnOcc, (square) => {
                blackAttackBitboard |= MoveGenData.knightTargets[square];
            });

            BitboardUtility.ForEachBitscanForward(bbOcc, (square) => {
                blackAttackBitboard |= MoveGenHelper.BishopAttacks(AllPiecesBitboard, square);
                whiteUnsafeKingSquares |= MoveGenHelper.BishopAttacks(AllPiecesBitboard^GetBitboardByPieceType(PieceType.WK), square);
            });

            BitboardUtility.ForEachBitscanForward(brOcc, (square) => {
                blackAttackBitboard |= MoveGenHelper.RookAttacks(AllPiecesBitboard, square);
                whiteUnsafeKingSquares |= MoveGenHelper.RookAttacks(AllPiecesBitboard^GetBitboardByPieceType(PieceType.WK), square);
            });

            BitboardUtility.ForEachBitscanForward(bqOcc, (square) => {
                blackAttackBitboard |= MoveGenHelper.QueenAttacks(AllPiecesBitboard, square);
                whiteUnsafeKingSquares |= MoveGenHelper.QueenAttacks(AllPiecesBitboard^GetBitboardByPieceType(PieceType.WK), square);
            });

            BitboardUtility.ForEachBitscanForward(bkOcc, (square) => {
                blackAttackBitboard |= MoveGenData.kingTargets[square];
            });

            whiteUnsafeKingSquares |= blackAttackBitboard;

            hasCachedWhiteUnsafeKingSquares = true;
            hasCachedBlackAttackBitboard = true;
            return blackAttackBitboard;
        }
    }

    public ulong BlackUnsafeKingSquares
    {
        get
        {
            if (hasCachedBlackUnsafeKingSquares) return blackUnsafeKingSquares;

            blackUnsafeKingSquares = 0ul;
            
            ulong wpOcc = GetBitboardByPieceType(PieceType.WP);
            ulong wnOcc = GetBitboardByPieceType(PieceType.WN);
            ulong wbOcc = GetBitboardByPieceType(PieceType.WB);
            ulong wrOcc = GetBitboardByPieceType(PieceType.WR);
            ulong wqOcc = GetBitboardByPieceType(PieceType.WQ);
            ulong wkOcc = GetBitboardByPieceType(PieceType.WK);

            BitboardUtility.ForEachBitscanForward(wpOcc, (square) => {
                blackUnsafeKingSquares |= MoveGenData.whitePawnAttacks[square];
            });

            BitboardUtility.ForEachBitscanForward(wnOcc, (square) => {
                blackUnsafeKingSquares |= MoveGenData.knightTargets[square];
            });

            BitboardUtility.ForEachBitscanForward(wbOcc, (square) => {
                blackUnsafeKingSquares |= MoveGenHelper.BishopAttacks(AllPiecesBitboard^GetBitboardByPieceType(PieceType.BK), square);
            });

            BitboardUtility.ForEachBitscanForward(wrOcc, (square) => {
                blackUnsafeKingSquares |= MoveGenHelper.RookAttacks(AllPiecesBitboard^GetBitboardByPieceType(PieceType.BK), square);
            });

            BitboardUtility.ForEachBitscanForward(wqOcc, (square) => {
                blackUnsafeKingSquares |= MoveGenHelper.QueenAttacks(AllPiecesBitboard^GetBitboardByPieceType(PieceType.BK), square);
            });

            BitboardUtility.ForEachBitscanForward(wkOcc, (square) => {
                blackUnsafeKingSquares |= MoveGenData.kingTargets[square];
            });

            hasCachedBlackUnsafeKingSquares = true;
            return blackUnsafeKingSquares;
        }
    }

    public ulong GetBitboardByPieceType(PieceType pieceType)
    {
        return bitboards[(int)pieceType];
    }

    private void ClearCaches()
    {
        hasCachedWhiteAttackBitboard = false;
        hasCachedBlackAttackBitboard = false;

        hasCachedBlackUnsafeKingSquares = false;
        hasCachedWhiteUnsafeKingSquares = false;

        hasCachedIsKingInCheck = false;

        hasCachedKingAttackers = false;
    }

    public Move[] GetLegalMoves(bool capturesOnly = false)
    {
        return moveGen.GenerateLegalMoves(capturesOnly).ToArray();
    }

    public bool DoesMoveNotPutOwnKingInCheck(Move move)
    {
        MakeMove(move);
        bool inCheck = IsKingInCheck(!isWhiteToMove);
        UnmakeMove();

        return !inCheck;
    }

    public bool IsMoveLegal(Move move)
    {
        // Check that we have a piece on that square
        if (((1ul << move.From) & (IsWhiteToMove ? WhitePiecesBitboard : BlackPiecesBitboard)) == 0) return false;

        // Check that there isn't a friendly piece on that square
        if (((1ul << move.To) & (IsWhiteToMove ? WhitePiecesBitboard : BlackPiecesBitboard)) != 0) return false;

        if (!DoesMoveNotPutOwnKingInCheck(move)) return false;

        return true;
    }

    public bool IsKingInCheck(bool white)
    {
        if (hasCachedIsKingInCheck) return isKingInCheck;

        int kingSquare = white ? BitboardUtility.IndexOfLSB(bitboards[(int)PieceType.WK]) : BitboardUtility.IndexOfLSB(bitboards[(int)PieceType.BK]);
        isKingInCheck = BitboardUtility.IsBitSet(white ? BlackAttackBitboard : WhiteAttackBitboard, kingSquare);

        hasCachedIsKingInCheck = true;
        return isKingInCheck;
    }

    public ulong GetKingAttackers(bool white)
    {
        if (hasCachedKingAttackers) return kingAttackers;

        int kingSquare = white ? BitboardUtility.IndexOfLSB(bitboards[(int)PieceType.WK]) : BitboardUtility.IndexOfLSB(bitboards[(int)PieceType.BK]);
        kingAttackers = 0ul;

        ulong pawnAttackers = white ? MoveGenData.whitePawnAttacks[kingSquare] & bitboards[(int)PieceType.BP] : MoveGenData.blackPawnAttacks[kingSquare] & bitboards[(int)PieceType.WP];
        ulong knightAttackers = MoveGenData.knightTargets[kingSquare] & bitboards[white ? (int)PieceType.BN : (int)PieceType.WN];
        ulong bishopQueenAttackers = Magic.GetBishopTargets(kingSquare, AllPiecesBitboard) & (white ? bitboards[(int)PieceType.BB] | bitboards[(int)PieceType.BQ] : bitboards[(int)PieceType.WB] | bitboards[(int)PieceType.WQ]);
        ulong rookQueenAttackers = Magic.GetRookTargets(kingSquare, AllPiecesBitboard) & (white ? bitboards[(int)PieceType.BR] | bitboards[(int)PieceType.BQ] : bitboards[(int)PieceType.WR] | bitboards[(int)PieceType.WQ]);

        kingAttackers |= pawnAttackers | knightAttackers | bishopQueenAttackers | rookQueenAttackers;

        isKingInCheck = kingAttackers != 0;
        hasCachedIsKingInCheck = true;

        hasCachedKingAttackers = true;
        return kingAttackers;
    }

    public PieceType GetPieceType(ulong positionBitboard)
    {
        if ((bitboards[0] & positionBitboard) != 0) return PieceType.WP;
        else if ((bitboards[6] & positionBitboard) != 0) return PieceType.BP;
        else if ((bitboards[1] & positionBitboard) != 0) return PieceType.WN;
        else if ((bitboards[2] & positionBitboard) != 0) return PieceType.WB;
        else if ((bitboards[3] & positionBitboard) != 0) return PieceType.WR;
        else if ((bitboards[7] & positionBitboard) != 0) return PieceType.BN;
        else if ((bitboards[8] & positionBitboard) != 0) return PieceType.BB;
        else if ((bitboards[9] & positionBitboard) != 0) return PieceType.BR;
        else if ((bitboards[4] & positionBitboard) != 0) return PieceType.WQ;
        else if ((bitboards[10] & positionBitboard) != 0) return PieceType.BQ;
        else if ((bitboards[5] & positionBitboard) != 0) return PieceType.WK;
        else if ((bitboards[11] & positionBitboard) != 0) return PieceType.BK;
        else return PieceType.EMPTY;
    }

    /// <summary>
    /// Returns the type of piece on the tile at the given index.
    /// </summary>
    public PieceType GetPieceType(int index)
    {
        ulong positionBitboard = 1ul << index;
        return GetPieceType(positionBitboard);

        /*
        for (int i = 0; ; i++)
        {
            try
            {
                if ((bitboards[i] & positionBitboard) != 0) return (PieceType)i;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("Index: " + i);
                for (int j = 0; j < bitboards.Length; j++)
                {
                    Console.WriteLine("Bitboard for " + (PieceType)j);
                    BitboardUtility.PrintBitboard(bitboards[j]);
                }
                Environment.Exit(0);
            }
        }
        */
    }

    public bool SquareIsUnderAttackByEnemyPawn(int square)
    {
        return BitboardUtility.IsBitSet(isWhiteToMove ? moveGen.BlackPawnAttackBitboard(bitboards[(int)PieceType.BP]) : moveGen.WhitePawnAttackBitboard(bitboards[(int)PieceType.WP]), square);
    }

    public bool SquareIsUnderEnemyAttack(int square)
    {
        return BitboardUtility.IsBitSet(isWhiteToMove ? BlackAttackBitboard : WhiteAttackBitboard, square);
    }

    public bool SquareIsDefended(int square)
    {
        return BitboardUtility.IsBitSet(isWhiteToMove ? WhiteAttackBitboard : BlackAttackBitboard, square);
    }

    public ulong BlackPawnAttackBitboard => moveGen.BlackPawnAttackBitboard(bitboards[(int)PieceType.BP]);

    public ulong WhitePawnAttackBitboard => moveGen.WhitePawnAttackBitboard(bitboards[(int)PieceType.WP]);

    /// <summary>
    /// How many half moves have there been since a pawn move or capture?
    /// Used to detect draws due to the 50-move rule.
    /// </summary>
    public int NumPlySincePawnMoveOrCapture => numPlySincePawnMoveOrCapture;

    public void AttachListener(IBoardListener listener)
    {
        listeners.Add(listener);
    }

    public int NumPlyPlayed => stateHistory.Count;

}