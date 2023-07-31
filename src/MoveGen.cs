namespace ChessBot;
public class MoveGen : IBoardListener
{
    // Maintain an instance of Board for appropriate move generation
    Board board;

    private List<Move> legalMoves;
    private bool hasCachedLegalMoves;

    private ulong pinnedPieces;
    private bool hasCachedPinnedPieces;
    
    public MoveGen(Board board)
    {
        this.board = board;

        legalMoves = new();
        hasCachedLegalMoves = false;

        pinnedPieces = new();
        hasCachedPinnedPieces = false; 
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
            legalMoves = GenerateWhiteLegalMoves();
        }
        else
        {
            legalMoves = GenerateBlackLegalMoves();
        }

        hasCachedLegalMoves = true;
        return legalMoves;
    }

    private List<Move> GenerateWhiteLegalMoves()
    {
        ulong friendlyPieces = board.WhitePiecesBitboard;
        ulong enemyPieces = board.BlackPiecesBitboard;

        List<Move> moves = new();

        ulong enemyKing = board.GetBitboardByPieceType(PieceType.BK);

        // Generate rook moves
        GenerateRookMoves(moves, board.GetBitboardByPieceType(PieceType.WR), friendlyPieces | enemyKing, enemyPieces ^ enemyKing, true);
        GenerateBishopMoves(moves, board.GetBitboardByPieceType(PieceType.WB), friendlyPieces | enemyKing, enemyPieces ^ enemyKing, true);
        GenerateQueenMoves(moves, board.GetBitboardByPieceType(PieceType.WQ), friendlyPieces | enemyKing, enemyPieces ^ enemyKing, true);
        GenerateKnightMoves(moves, board.GetBitboardByPieceType(PieceType.WN), friendlyPieces | enemyKing, enemyPieces ^ enemyKing, true);
        GenerateWhitePawnMoves(moves, board.GetBitboardByPieceType(PieceType.WP), friendlyPieces | enemyKing, enemyPieces ^ enemyKing, board.EpFile);
        GenerateKingMoves(moves, board.GetBitboardByPieceType(PieceType.WK), friendlyPieces | enemyKing, enemyPieces ^ enemyKing, true);
        GenerateCastlingMoves(moves, enemyPieces | friendlyPieces, board.CanWhiteCastleKingside(), board.CanWhiteCastleQueenside(), board.CanBlackCastleKingside(), board.CanBlackCastleQueenside(), true);

        return moves;
    }


    private List<Move> GenerateBlackLegalMoves()
    {

        ulong friendlyPieces = board.BlackPiecesBitboard;
        ulong enemyPieces = board.WhitePiecesBitboard;

        List<Move> moves = new();

        ulong enemyKing = board.GetBitboardByPieceType(PieceType.WK);

        // Generate moves for each piece type here
        GenerateRookMoves(moves, board.GetBitboardByPieceType(PieceType.BR),friendlyPieces | enemyKing, enemyPieces ^ enemyKing, false);
        GenerateBishopMoves(moves, board.GetBitboardByPieceType(PieceType.BB), friendlyPieces | enemyKing, enemyPieces ^ enemyKing, false);
        GenerateKnightMoves(moves, board.GetBitboardByPieceType(PieceType.BN), friendlyPieces | enemyKing, enemyPieces ^ enemyKing, false);
        GenerateQueenMoves(moves, board.GetBitboardByPieceType(PieceType.BQ), friendlyPieces | enemyKing, enemyPieces ^ enemyKing, false);
        GenerateBlackPawnMoves(moves, board.GetBitboardByPieceType(PieceType.BP), friendlyPieces | enemyKing, enemyPieces ^ enemyKing, board.EpFile);
        GenerateKingMoves(moves, board.GetBitboardByPieceType(PieceType.BK), friendlyPieces | enemyKing, enemyPieces ^ enemyKing, false);
        GenerateCastlingMoves(moves, enemyPieces | friendlyPieces, board.CanWhiteCastleKingside(), board.CanWhiteCastleQueenside(), board.CanBlackCastleKingside(), board.CanBlackCastleQueenside(), false);
        
        return moves;
    }

    private List<Move> GenerateKingMoves(List<Move> moves, ulong kingBitboard, ulong friendlyPieces, ulong enemyPieces, bool white)
    {
        ulong kingLSB = BitboardUtility.IsolateLSB(kingBitboard);
        int kingPosition = BitboardUtility.IndexOfLSB(kingLSB);

        ulong targets = MoveGenData.kingTargets[kingPosition];
        BitboardUtility.ForEachBitscanForward(targets, (targetSquare) =>
        {
            ulong targetBB = 1ul << targetSquare;
            if ((targetBB & friendlyPieces)==0)
            {
                //if (kingPosition == 51 & white) BitboardUtility.PrintBitboard(board.BlackAttackBitboard);
                if ((targetBB & (white ? board.WhiteUnsafeKingSquares : board.BlackUnsafeKingSquares)) == 0)
                {
                    int flag = Move.QuietMoveFlag;
                    if ((targetBB & enemyPieces) != 0) flag = Move.CaptureFlag;
                    Move move = new(kingPosition, targetSquare, white ? PieceType.WK : PieceType.BK, board.GetPieceType(targetSquare), flag);
                    moves.Add(move);
                }
            }
        });

        return moves;
    }

    private List<Move> GenerateCastlingMoves(List<Move> moves, ulong allPieces, bool CWK, bool CWQ, bool CBK, bool CBQ, bool white)
    {
        if (board.IsKingInCheck(white)) return moves;

        // Does white have kingside castling rights and are there no obstructing pieces
        if (white)
        {
            if (CWK && ((0b11ul << 61) & allPieces) == 0 && ((0b1ul << 61) & board.BlackAttackBitboard) == 0)
            {
                Move move = new(60, 62, PieceType.WK, PieceType.EMPTY, Move.KingCastleFlag);
                if (board.IsMoveLegal(move)) moves.Add(move);
            }

            if (CWQ && ((0b111ul << 57) & allPieces) == 0 && ((0b11ul << 58) & board.BlackAttackBitboard) == 0)
            {
                Move move = new(60, 58, PieceType.WK, PieceType.EMPTY, Move.QueenCastleFlag);
                if (board.IsMoveLegal(move)) moves.Add(move);
            }
        }
        else
        {
            if (CBK && ((0b1100000ul & allPieces) == 0) && ((0b100000ul & board.WhiteAttackBitboard) == 0))
            {
                Move move = new(4, 6, PieceType.BK, PieceType.EMPTY, Move.KingCastleFlag);
                if (board.IsMoveLegal(move)) moves.Add(move);
            }

            if (CBQ && ((0b1110ul & allPieces) == 0) && ((0b1100) & board.WhiteAttackBitboard) == 0)
            {
                Move move = new(4, 2, PieceType.BK, PieceType.EMPTY, Move.QueenCastleFlag);
                if (board.IsMoveLegal(move)) moves.Add(move);
            }
        }

        return moves;
    }

    public List<Move>  GenerateBlackPawnMoves(List<Move> moves, ulong pawnBitboard, ulong friendlyPieces, ulong enemyPieces, ulong epFile, bool capturesOnly = false)
    {
        bool unsafeMove;

        ulong kingAttackers = board.GetKingAttackers(board.IsWhiteToMove);

        int numAttackers = BitboardUtility.CountSetBits(kingAttackers);
        if (numAttackers > 1) return moves; // only king moves are allowed in double check

        ulong captureMask = ulong.MaxValue; // if the king is in check, this is limited to just the attacking piece(s) bitboard
        ulong pushMask = ulong.MaxValue; // if the king is in check, this is limited to the squares between the king and the attacking piece(s)

        if (numAttackers == 1)
        {
            captureMask = kingAttackers;
            pushMask = MoveGenData.inBetweenLookupTable[board.BlackKingSquare][BitboardUtility.IndexOfLSB(kingAttackers)];
        }

        BitboardUtility.ForEachBitscanForward(pawnBitboard, (startingIndex) =>
        {
            ulong pawnLSB = 1ul << startingIndex;

            if ((pawnLSB & GetPinnedPiecesBitboard()) != 0) unsafeMove = true;
            else unsafeMove = false;

            // Generate attacks
            ulong attackTargets = MoveGenData.blackPawnAttacks[startingIndex] & enemyPieces & captureMask; 
            BitboardUtility.ForEachBitscanForward(attackTargets, (targetIndex) => 
            {
                PieceType capturedPiece = board.GetPieceType(targetIndex);

                // Check if this is a promoting move
                if ((targetIndex / 8) == 7)
                {
                    if (unsafeMove)
                    {
                        Move legalityTest = new(startingIndex, targetIndex, PieceType.BP, capturedPiece, Move.KnightPromoCaptureFlag);
                        if (board.IsMoveLegal(legalityTest))
                        {
                            moves.Add(new Move(startingIndex, targetIndex, PieceType.BP, capturedPiece, Move.KnightPromoCaptureFlag));
                            moves.Add(new Move(startingIndex, targetIndex, PieceType.BP, capturedPiece, Move.BishopPromoCaptureFlag));
                            moves.Add(new Move(startingIndex, targetIndex, PieceType.BP, capturedPiece, Move.RookPromoCaptureFlag));
                            moves.Add(new Move(startingIndex, targetIndex, PieceType.BP, capturedPiece, Move.QueenPromoCaptureFlag));
                        }
                    }
                    else
                    {
                        moves.Add(new Move(startingIndex, targetIndex, PieceType.BP, capturedPiece, Move.KnightPromoCaptureFlag));
                        moves.Add(new Move(startingIndex, targetIndex, PieceType.BP, capturedPiece, Move.BishopPromoCaptureFlag));
                        moves.Add(new Move(startingIndex, targetIndex, PieceType.BP, capturedPiece, Move.RookPromoCaptureFlag));
                        moves.Add(new Move(startingIndex, targetIndex, PieceType.BP, capturedPiece, Move.QueenPromoCaptureFlag));
                    }

                }
                else
                {
                    Move move = new(startingIndex, targetIndex, PieceType.BP, board.GetPieceType(targetIndex), Move.CaptureFlag);
                    if (!unsafeMove) moves.Add(move);
                    else
                    {
                        if (board.IsMoveLegal(move))
                        {
                            moves.Add(move);
                        }
                    }
                }

            });

            // en passant
            if (epFile != 0)
            {
                ulong thisPawnBitboard = 1ul << startingIndex;

                // EP to the right
                // Is this pawn to the left of the en passant file? Is this pawn on rank 4?
                if (((thisPawnBitboard << 1) & epFile) != 0 && (thisPawnBitboard & MoveGenData.RankMasks[3]) != 0 && (thisPawnBitboard & MoveGenData.FileMasks[7]) == 0)
                {
                    Move move = new(startingIndex, startingIndex+9, PieceType.BP, PieceType.WP, Move.EpCaptureFlag);
                    if (board.IsMoveLegal(move)) moves.Add(move);
                }

                if (((thisPawnBitboard >> 1) & epFile) != 0 && (thisPawnBitboard & MoveGenData.RankMasks[3]) != 0 & (thisPawnBitboard & MoveGenData.FileMasks[0]) == 0)
                {
                    Move move = new(startingIndex, startingIndex+7, PieceType.BP, PieceType.WP, Move.EpCaptureFlag);
                    if (board.IsMoveLegal(move)) moves.Add(move);
                }
            }

            // Stop here if we only want to generate captures
            if (capturesOnly) return;
            
            ulong target; // declaring this variable here because if I don't then some weird error happens (idk why)

            // Down pawn push
            if ((startingIndex / 8) == 1)
            {
                target = 1ul << (startingIndex + 16) & ~(friendlyPieces|enemyPieces);
                target &= pushMask;
                if (target != 0 && ((target>>8)&(friendlyPieces|enemyPieces))==0) // check for obstruction
                {
                    int targetIndex = BitboardUtility.IndexOfLSB(target);
                    Move move = new(startingIndex, targetIndex, PieceType.BP, PieceType.EMPTY, Move.DoublePawnPushFlag);
                    if (!unsafeMove)
                    {
                        moves.Add(move);
                    }
                    else
                    {
                        if (board.IsMoveLegal(move))
                        {
                            moves.Add(move);
                        }
                    }
                }
            }

            // Single pawn push
            target = 1ul << (startingIndex + 8) & ~(friendlyPieces|enemyPieces);
            target &= pushMask;
            if (target != 0) // check for obstruction
            {
                int targetIndex = BitboardUtility.IndexOfLSB(target);

                // Check if this is a promoting move
                if ((targetIndex / 8) == 7)
                {
                    if (unsafeMove)
                    {
                        Move legalityTest = new(startingIndex, targetIndex, PieceType.BP, PieceType.EMPTY, Move.KnightPromotionFlag);
                        if (board.IsMoveLegal(legalityTest))
                        {
                            moves.Add(new Move(startingIndex, targetIndex, PieceType.BP, PieceType.EMPTY, Move.KnightPromotionFlag));
                            moves.Add(new Move(startingIndex, targetIndex, PieceType.BP, PieceType.EMPTY, Move.BishopPromotionFlag));
                            moves.Add(new Move(startingIndex, targetIndex, PieceType.BP, PieceType.EMPTY, Move.RookPromotionFlag));
                            moves.Add(new Move(startingIndex, targetIndex, PieceType.BP, PieceType.EMPTY, Move.QueenPromotionFlag));
                        }
                    }
                    else
                    {
                        moves.Add(new Move(startingIndex, targetIndex, PieceType.BP, PieceType.EMPTY, Move.KnightPromotionFlag));
                        moves.Add(new Move(startingIndex, targetIndex, PieceType.BP, PieceType.EMPTY, Move.BishopPromotionFlag));
                        moves.Add(new Move(startingIndex, targetIndex, PieceType.BP, PieceType.EMPTY, Move.RookPromotionFlag));
                        moves.Add(new Move(startingIndex, targetIndex, PieceType.BP, PieceType.EMPTY, Move.QueenPromotionFlag));
                    }
                }
                else
                {
                    Move move = new(startingIndex, targetIndex, PieceType.BP, PieceType.EMPTY, Move.QuietMoveFlag);
                    if (!unsafeMove) moves.Add(move);
                    else
                    {
                        if (board.IsMoveLegal(move)) moves.Add(move);
                    }
                }
            }

        });

        return moves;
    }

    public List<Move> GenerateWhitePawnMoves(List<Move> moves, ulong pawnBitboard, ulong friendlyPieces, ulong enemyPieces, ulong epFile, bool capturesOnly = false)
    {
        bool unsafeMove;

        ulong kingAttackers = board.GetKingAttackers(board.IsWhiteToMove);

        int numAttackers = BitboardUtility.CountSetBits(kingAttackers);
        if (numAttackers > 1) return moves; // only king moves are allowed in double check

        ulong captureMask = ulong.MaxValue; // if the king is in check, this is limited to just the attacking piece(s) bitboard
        ulong pushMask = ulong.MaxValue; // if the king is in check, this is limited to the squares between the king and the attacking piece(s)

        if (numAttackers == 1)
        {
            captureMask = kingAttackers;
            pushMask = MoveGenData.inBetweenLookupTable[board.WhiteKingSquare][BitboardUtility.IndexOfLSB(kingAttackers)];
        }

        BitboardUtility.ForEachBitscanForward(pawnBitboard, (startingIndex) => 
        {
            ulong pawnLSB = 1ul << startingIndex;

            if ((pawnLSB & GetPinnedPiecesBitboard()) != 0) unsafeMove = true;
            else unsafeMove = false;

            // Generate attacks
            ulong attackTargets = MoveGenData.whitePawnAttacks[startingIndex] & enemyPieces & captureMask; 
            BitboardUtility.ForEachBitscanForward(attackTargets, (targetIndex) => 
            {
                PieceType capturedPiece = board.GetPieceType(targetIndex);

                // Check if this is a promoting move
                if ((targetIndex / 8) == 0)
                {
                    if (unsafeMove)
                    {
                        Move legalityTest = new(startingIndex, targetIndex, PieceType.WP, capturedPiece, Move.KnightPromoCaptureFlag);
                        if (board.IsMoveLegal(legalityTest))
                        {
                            moves.Add(new Move(startingIndex, targetIndex, PieceType.WP, capturedPiece, Move.KnightPromoCaptureFlag));
                            moves.Add(new Move(startingIndex, targetIndex, PieceType.WP, capturedPiece, Move.BishopPromoCaptureFlag));
                            moves.Add(new Move(startingIndex, targetIndex, PieceType.WP, capturedPiece, Move.RookPromoCaptureFlag));
                            moves.Add(new Move(startingIndex, targetIndex, PieceType.WP, capturedPiece, Move.QueenPromoCaptureFlag));
                        }
                    }
                    else
                    {
                        moves.Add(new Move(startingIndex, targetIndex, PieceType.WP, capturedPiece, Move.KnightPromoCaptureFlag));
                        moves.Add(new Move(startingIndex, targetIndex, PieceType.WP, capturedPiece, Move.BishopPromoCaptureFlag));
                        moves.Add(new Move(startingIndex, targetIndex, PieceType.WP, capturedPiece, Move.RookPromoCaptureFlag));
                        moves.Add(new Move(startingIndex, targetIndex, PieceType.WP, capturedPiece, Move.QueenPromoCaptureFlag));
                    }
                }
                else
                {
                    Move move = new(startingIndex, targetIndex, PieceType.WP, board.GetPieceType(targetIndex), Move.CaptureFlag);
                    if (!unsafeMove) moves.Add(move);
                    else
                    {
                        if (board.IsMoveLegal(move)) moves.Add(move);
                    }
                }
            });

            // en passant
            if (epFile != 0)
            {
                ulong thisPawnBitboard = 1ul << startingIndex;

                // EP to the right
                // Is this pawn to the left of the en passant file? Is this pawn on rank 5? Is this pawn not on file H?
                if (((thisPawnBitboard << 1) & epFile) != 0 && (thisPawnBitboard & MoveGenData.RankMasks[4]) != 0 && (thisPawnBitboard & MoveGenData.FileMasks[7]) == 0)
                {
                    Move move = new(startingIndex, startingIndex-7, PieceType.WP, PieceType.BP, Move.EpCaptureFlag);
                    if (board.IsMoveLegal(move)) moves.Add(move);
                }

                if (((thisPawnBitboard >> 1) & epFile) != 0 && (thisPawnBitboard & MoveGenData.RankMasks[4]) != 0 && (thisPawnBitboard & MoveGenData.FileMasks[0]) == 0)
                {
                    Move move = new(startingIndex, startingIndex-9, PieceType.WP, PieceType.BP, Move.EpCaptureFlag);
                    if (board.IsMoveLegal(move)) moves.Add(move);
                }
            }

            // Stop here if we only want to generate captures
            if (capturesOnly) return;
            
            ulong target; // declaring this variable here because if I don't then some weird error happens (idk why)

            // Down pawn push
            if ((startingIndex / 8) == 6)
            {
                target = 1ul << (startingIndex - 16);
                target &= ~(friendlyPieces|enemyPieces);
                target &= pushMask;
                if (target != 0 && ((target<<8)&(friendlyPieces|enemyPieces))==0) // check for obstruction
                {
                    int targetIndex = BitboardUtility.IndexOfLSB(target);
                    Move move = new(startingIndex, targetIndex, PieceType.WP, PieceType.EMPTY, Move.DoublePawnPushFlag);
                    if (!unsafeMove) moves.Add(move);
                    else
                    {
                        if (board.IsMoveLegal(move)) moves.Add(move);
                    }
                }
            }

            // Single pawn push
            target = 1ul << (startingIndex - 8) & ~(friendlyPieces|enemyPieces);
            target &= pushMask;
            if (target != 0) // check for obstruction
            {
                int targetIndex = BitboardUtility.IndexOfLSB(target);

                // Check if this is a promoting move
                if ((targetIndex / 8) == 0)
                {
                    if (unsafeMove)
                    {
                        Move legalityTest = new(startingIndex, targetIndex, PieceType.WP, PieceType.EMPTY, Move.KnightPromotionFlag);
                        if (board.IsMoveLegal(legalityTest)) 
                        {
                            moves.Add(new Move(startingIndex, targetIndex, PieceType.WP, PieceType.EMPTY, Move.KnightPromotionFlag));
                            moves.Add(new Move(startingIndex, targetIndex, PieceType.WP, PieceType.EMPTY, Move.BishopPromotionFlag));
                            moves.Add(new Move(startingIndex, targetIndex, PieceType.WP, PieceType.EMPTY, Move.RookPromotionFlag));
                            moves.Add(new Move(startingIndex, targetIndex, PieceType.WP, PieceType.EMPTY, Move.QueenPromotionFlag));
                        }
                    }
                    else
                    {
                        moves.Add(new Move(startingIndex, targetIndex, PieceType.WP, PieceType.EMPTY, Move.KnightPromotionFlag));
                        moves.Add(new Move(startingIndex, targetIndex, PieceType.WP, PieceType.EMPTY, Move.BishopPromotionFlag));
                        moves.Add(new Move(startingIndex, targetIndex, PieceType.WP, PieceType.EMPTY, Move.RookPromotionFlag));
                        moves.Add(new Move(startingIndex, targetIndex, PieceType.WP, PieceType.EMPTY, Move.QueenPromotionFlag));
                    }
                }
                else
                {
                    Move move = new(startingIndex, targetIndex, PieceType.WP, PieceType.EMPTY, Move.QuietMoveFlag);
                    if (!unsafeMove) moves.Add(move);
                    else
                    {
                        if (board.IsMoveLegal(move)) moves.Add(move);
                    }
                }
            }
        });

        return moves;
    }

    public List<Move> GenerateQueenMoves(List<Move> moves, ulong queenBitboard, ulong friendlyPieces, ulong enemyPieces, bool white)
    {
        bool unsafeMove;

        ulong kingAttackers = board.GetKingAttackers(board.IsWhiteToMove);

        int numAttackers = BitboardUtility.CountSetBits(kingAttackers);
        if (numAttackers > 1) return moves; // only king moves are allowed in double check

        ulong captureMask = ulong.MaxValue; // if the king is in check, this is limited to just the attacking piece(s) bitboard
        ulong pushMask = ulong.MaxValue; // if the king is in check, this is limited to the squares between the king and the attacking piece(s)

        if (numAttackers == 1)
        {
            captureMask = kingAttackers;
            pushMask = MoveGenData.inBetweenLookupTable[white ? board.WhiteKingSquare : board.BlackKingSquare][BitboardUtility.IndexOfLSB(kingAttackers)];
        }

        ulong queenLSB = BitboardUtility.IsolateLSB(queenBitboard);
        while (queenLSB != 0)
        {
            if ((queenLSB & GetPinnedPiecesBitboard()) != 0) unsafeMove = true;
            else unsafeMove = false;

            int startingIndex = BitboardUtility.IndexOfLSB(queenLSB);

            ulong diagonalBlockerBitboard = Magic.CreateBlockerBitboard((friendlyPieces | enemyPieces) ^ queenLSB, Magic.GetSimplifiedDiagonalMovementMask(startingIndex));
            ulong orthogonalBlockerBitboard = Magic.CreateBlockerBitboard((friendlyPieces | enemyPieces) ^ queenLSB, Magic.GetSimplifiedOrthogonalMovementMask(startingIndex));

            ulong targets = MoveGenData.DiagonalMovesAttackTable[startingIndex][Magic.GetDiagonalAttackTableKey(startingIndex, diagonalBlockerBitboard)] | MoveGenData.OrthogonalMovesAttackTable[startingIndex][Magic.GetOrthogonalAttackTableKey(startingIndex, orthogonalBlockerBitboard)];
            targets &= captureMask|pushMask;

            ulong targetsLSB = BitboardUtility.IsolateLSB(targets);
            while (targetsLSB != 0)
            {
                // Do not add the move if it lands on a friendly piece
                if ((targetsLSB & friendlyPieces) == 0)
                {
                    int targetIndex = BitboardUtility.IndexOfLSB(targetsLSB);
                    int flag = Move.QuietMoveFlag;

                    if ((targetsLSB & enemyPieces) != 0) flag = Move.CaptureFlag;

                    Move move = new(startingIndex, targetIndex, white ? PieceType.WQ : PieceType.BQ, board.GetPieceType(targetIndex), flag);
                    if (!unsafeMove) moves.Add(move);
                    else
                    {
                        if (board.IsMoveLegal(move)) moves.Add(move);
                    }
                }

                targets &= ~targetsLSB;
                targetsLSB = BitboardUtility.IsolateLSB(targets);
            }

            queenBitboard &= ~queenLSB;
            queenLSB = BitboardUtility.IsolateLSB(queenBitboard);
        }

        return moves;
    }

    public List<Move> GenerateKnightMoves(List<Move> moves, ulong knightBitboard, ulong friendlyPieces, ulong enemyPieces, bool white)
    {
        bool unsafeMove;

        ulong kingAttackers = board.GetKingAttackers(board.IsWhiteToMove);

        int numAttackers = BitboardUtility.CountSetBits(kingAttackers);
        if (numAttackers > 1) return moves; // only king moves are allowed in double check

        ulong captureMask = ulong.MaxValue; // if the king is in check, this is limited to just the attacking piece(s) bitboard
        ulong pushMask = ulong.MaxValue; // if the king is in check, this is limited to the squares between the king and the attacking piece(s)

        if (numAttackers == 1)
        {
            captureMask = kingAttackers;
            pushMask = MoveGenData.inBetweenLookupTable[white ? board.WhiteKingSquare : board.BlackKingSquare][BitboardUtility.IndexOfLSB(kingAttackers)];
        }

        ulong knightLSB = BitboardUtility.IsolateLSB(knightBitboard);
        while (knightLSB != 0)
        {
            if ((knightLSB & GetPinnedPiecesBitboard()) != 0) unsafeMove = true;
            else unsafeMove = false;

            int startingIndex = BitboardUtility.IndexOfLSB(knightLSB);

            ulong targets = MoveGenData.GetKnightTargetsBitboard(startingIndex) & (captureMask|pushMask);

            ulong targetsLSB = BitboardUtility.IsolateLSB(targets);
            while (targetsLSB != 0)
            {
                // Do not add moves which land on friendly pieces
                if ((targetsLSB & friendlyPieces) == 0)
                {
                    int targetIndex = BitboardUtility.IndexOfLSB(targetsLSB);
                    int flag = Move.QuietMoveFlag;

                    // Check if this is a capturing move
                    if ((targetsLSB & enemyPieces) != 0) flag = Move.CaptureFlag;

                    Move move = new(startingIndex, targetIndex, white ? PieceType.WN : PieceType.BN, board.GetPieceType(targetIndex), flag);
                    if (!unsafeMove) moves.Add(move);
                    else
                    {
                        if (board.IsMoveLegal(move)) moves.Add(move);
                    }
                }

                targets &= ~targetsLSB;
                targetsLSB = BitboardUtility.IsolateLSB(targets);
            }

            knightBitboard &= ~knightLSB;
            knightLSB = BitboardUtility.IsolateLSB(knightBitboard);
        }

        return moves;
    }

    /// <summary>
    /// Generates a list of legal bishop moves for the given coloured player.
    /// </summary>
    public List<Move> GenerateBishopMoves(List<Move> moves, ulong bishopBitboard, ulong friendlyPieces, ulong enemyPieces, bool white) 
    {
        bool unsafeMove;
        ulong kingAttackers = board.GetKingAttackers(board.IsWhiteToMove);

        int numAttackers = BitboardUtility.CountSetBits(kingAttackers);
        if (numAttackers > 1) return moves; // only king moves are allowed in double check

        ulong captureMask = ulong.MaxValue; // if the king is in check, this is limited to just the attacking piece(s) bitboard
        ulong pushMask = ulong.MaxValue; // if the king is in check, this is limited to the squares between the king and the attacking piece(s)

        if (numAttackers == 1)
        {
            captureMask = kingAttackers;
            pushMask = MoveGenData.inBetweenLookupTable[white ? board.WhiteKingSquare : board.BlackKingSquare][BitboardUtility.IndexOfLSB(kingAttackers)];
        }

        ulong bishopLSB = BitboardUtility.IsolateLSB(bishopBitboard);
        while (bishopLSB != 0)
        {
            if ((bishopLSB & GetPinnedPiecesBitboard()) != 0) unsafeMove = true;
            else unsafeMove = false;

            int startingIndex = BitboardUtility.IndexOfLSB(bishopLSB);

            // Get the target bitboard
            ulong targets = Magic.GetBishopTargets(startingIndex, friendlyPieces|enemyPieces) & (captureMask|pushMask);

            ulong targetsLSB = BitboardUtility.IsolateLSB(targets);
            while (targetsLSB != 0)
            {
                // Do not add the move if the move lands on a friendly piece
                if ((targetsLSB & friendlyPieces) == 0)
                {
                    int targetIndex = BitboardUtility.IndexOfLSB(targetsLSB);
                    int flag = Move.QuietMoveFlag;

                    // Check if this is a capturing move
                    if ((targetsLSB & enemyPieces) != 0) flag = Move.CaptureFlag;

                    Move move = new(startingIndex, targetIndex, white ? PieceType.WB : PieceType.BB, board.GetPieceType(targetIndex), flag);
                    if (!unsafeMove) moves.Add(move);
                    else 
                    {
                        if (board.IsMoveLegal(move)) moves.Add(move);
                    }
                }

                targets &= ~targetsLSB;
                targetsLSB = BitboardUtility.IsolateLSB(targets);
            }

            bishopBitboard &= ~bishopLSB;
            bishopLSB = BitboardUtility.IsolateLSB(bishopBitboard);
        }

        return moves;
    }

    /// <summary>
    /// Generates a list of legal rook moves for the given coloured played.
    /// </summary>
    public List<Move> GenerateRookMoves(List<Move> moves, ulong rookBitboard, ulong friendlyPieces, ulong enemyPieces, bool white)
    {
        bool unsafeMove;

        ulong kingAttackers = board.GetKingAttackers(board.IsWhiteToMove);

        int numAttackers = BitboardUtility.CountSetBits(kingAttackers);
        if (numAttackers > 1) return moves; // only king moves are allowed in double check

        ulong captureMask = ulong.MaxValue; // if the king is in check, this is limited to just the attacking piece(s) bitboard
        ulong pushMask = ulong.MaxValue; // if the king is in check, this is limited to the squares between the king and the attacking piece(s)

        if (numAttackers == 1)
        {
            captureMask = kingAttackers;
            pushMask = MoveGenData.inBetweenLookupTable[white ? board.WhiteKingSquare : board.BlackKingSquare][BitboardUtility.IndexOfLSB(kingAttackers)];
        }

        ulong rookLSB = BitboardUtility.IsolateLSB(rookBitboard);
        while (rookLSB != 0)
        {
            if ((rookLSB & GetPinnedPiecesBitboard()) != 0) unsafeMove = true;
            else unsafeMove = false;

            int startingIndex = BitboardUtility.IndexOfLSB(rookLSB);

            // Get the rook targets, AND it with the (push OR capture) masks
            ulong targets = Magic.GetRookTargets(startingIndex, friendlyPieces|enemyPieces) & (captureMask|pushMask);

            ulong targetsLSB = BitboardUtility.IsolateLSB(targets);
            while (targetsLSB != 0)
            {
                // Do not add the move if the move lands on a friendly piece
                if ((targetsLSB & friendlyPieces) == 0)
                {
                    int targetIndex = BitboardUtility.IndexOfLSB(targetsLSB);
                    int flag = Move.QuietMoveFlag;

                    // Check if this is a capturing move
                    if ((targetsLSB & enemyPieces) != 0) flag = Move.CaptureFlag;

                    Move move = new(startingIndex, targetIndex, white ? PieceType.WR : PieceType.BR, board.GetPieceType(targetIndex), flag);
                    if (!unsafeMove) moves.Add(move);
                    else
                    {
                        if (board.IsMoveLegal(move)) moves.Add(move);
                    }
                }

                targets &= ~targetsLSB;
                targetsLSB = BitboardUtility.IsolateLSB(targets);
            }

            rookBitboard &= ~rookLSB;
            rookLSB = BitboardUtility.IsolateLSB(rookBitboard);
        }

        return moves;
    }

    public ulong GetPinnedPiecesBitboard()
    {
        if (hasCachedPinnedPieces) return pinnedPieces;
        if (board.IsWhiteToMove)
        {
            pinnedPieces = MoveGenHelper.GetPinnedPiecesBitboard(board.AllPiecesBitboard, board.WhitePiecesBitboard, board.GetBitboardByPieceType(PieceType.BB), board.GetBitboardByPieceType(PieceType.BR), board.GetBitboardByPieceType(PieceType.BQ), board.WhiteKingSquare);
        }
        else
        {
            pinnedPieces = MoveGenHelper.GetPinnedPiecesBitboard(board.AllPiecesBitboard, board.BlackPiecesBitboard, board.GetBitboardByPieceType(PieceType.WB), board.GetBitboardByPieceType(PieceType.WR), board.GetBitboardByPieceType(PieceType.WQ), board.BlackKingSquare);
        }

        hasCachedPinnedPieces = true;

        return pinnedPieces;
    }

    public void OnBoardStateChange()
    {
        legalMoves = new();
        hasCachedLegalMoves = false;
        hasCachedPinnedPieces = new();
        hasCachedPinnedPieces = false;
    }

}