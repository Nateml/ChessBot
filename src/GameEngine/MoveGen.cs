namespace ChessBot;
public class MoveGen : IBoardListener
{
    // Maintain an instance of Board for appropriate move generation
    Board board;

    private List<Move> legalMoves;
    private bool hasCachedLegalMoves;

    private ulong pinnedPieces;
    private bool hasCachedPinnedPieces;

    private ulong discoveredChecks;
    private bool hasCachedDiscoveredChecks;

    private ulong whitePawnAttackBitboard;
    private bool hasCachedWhitePawnAttackBitboard;

    private ulong blackPawnAttackBitboard;
    private bool hasCachedBlackPawnAttackBitboard;
    
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
    public List<Move> GenerateLegalMoves(bool capturesOnly = false)
    {
        // Return the cache if available
        if (hasCachedLegalMoves) return legalMoves;

        if (board.IsWhiteToMove)
        {
            legalMoves = GenerateWhiteLegalMoves(capturesOnly);
        }
        else
        {
            legalMoves = GenerateBlackLegalMoves(capturesOnly);
        }

        hasCachedLegalMoves = true;
        return legalMoves;
    }

    private List<Move> GenerateWhiteLegalMoves(bool capturesOnly = false)
    {
        ulong friendlyPieces = board.WhitePiecesBitboard;
        ulong enemyPieces = board.BlackPiecesBitboard;

        List<Move> moves = new();

        ulong enemyKing = board.GetBitboardByPieceType(PieceType.BK);

        // Generate rook moves
        GenerateRookMoves(moves, board.GetBitboardByPieceType(PieceType.WR), friendlyPieces | enemyKing, enemyPieces ^ enemyKing, true, capturesOnly);
        GenerateBishopMoves(moves, board.GetBitboardByPieceType(PieceType.WB), friendlyPieces | enemyKing, enemyPieces ^ enemyKing, true, capturesOnly);
        GenerateQueenMoves(moves, board.GetBitboardByPieceType(PieceType.WQ), friendlyPieces | enemyKing, enemyPieces ^ enemyKing, true, capturesOnly);
        GenerateKnightMoves(moves, board.GetBitboardByPieceType(PieceType.WN), friendlyPieces | enemyKing, enemyPieces ^ enemyKing, true, capturesOnly);
        GenerateWhitePawnMoves(moves, board.GetBitboardByPieceType(PieceType.WP), friendlyPieces | enemyKing, enemyPieces ^ enemyKing, board.EpFile, capturesOnly);
        GenerateKingMoves(moves, board.GetBitboardByPieceType(PieceType.WK), friendlyPieces | enemyKing, enemyPieces ^ enemyKing, true, capturesOnly);
        if (!capturesOnly) GenerateCastlingMoves(moves, enemyPieces | friendlyPieces, board.CanWhiteCastleKingside(), board.CanWhiteCastleQueenside(), board.CanBlackCastleKingside(), board.CanBlackCastleQueenside(), true);

        return moves;
    }


    private List<Move> GenerateBlackLegalMoves(bool capturesOnly = false)
    {

        ulong friendlyPieces = board.BlackPiecesBitboard;
        ulong enemyPieces = board.WhitePiecesBitboard;

        List<Move> moves = new();

        ulong enemyKing = board.GetBitboardByPieceType(PieceType.WK);

        // Generate moves for each piece type here
        GenerateRookMoves(moves, board.GetBitboardByPieceType(PieceType.BR),friendlyPieces | enemyKing, enemyPieces ^ enemyKing, false, capturesOnly);
        GenerateBishopMoves(moves, board.GetBitboardByPieceType(PieceType.BB), friendlyPieces | enemyKing, enemyPieces ^ enemyKing, false, capturesOnly);
        GenerateKnightMoves(moves, board.GetBitboardByPieceType(PieceType.BN), friendlyPieces | enemyKing, enemyPieces ^ enemyKing, false, capturesOnly);
        GenerateQueenMoves(moves, board.GetBitboardByPieceType(PieceType.BQ), friendlyPieces | enemyKing, enemyPieces ^ enemyKing, false, capturesOnly);
        GenerateBlackPawnMoves(moves, board.GetBitboardByPieceType(PieceType.BP), friendlyPieces | enemyKing, enemyPieces ^ enemyKing, board.EpFile, capturesOnly);
        GenerateKingMoves(moves, board.GetBitboardByPieceType(PieceType.BK), friendlyPieces | enemyKing, enemyPieces ^ enemyKing, false, capturesOnly);
        if (!capturesOnly) GenerateCastlingMoves(moves, enemyPieces | friendlyPieces, board.CanWhiteCastleKingside(), board.CanWhiteCastleQueenside(), board.CanBlackCastleKingside(), board.CanBlackCastleQueenside(), false);
        
        return moves;
    }

    private List<Move> GenerateKingMoves(List<Move> moves, ulong kingBitboard, ulong friendlyPieces, ulong enemyPieces, bool white, bool capturesOnly = false, bool checksOnly = false)
    {
        ulong kingLSB = BitboardUtility.IsolateLSB(kingBitboard);
        int kingPosition = BitboardUtility.IndexOfLSB(kingLSB);

        ulong targets = MoveGenData.kingTargets[kingPosition];

        // Must we only generate capturing moves
        if (capturesOnly)
        {
            targets &= enemyPieces;
        }

        // King cannot check the opponent king, besides for discoveries
        // TODO: Fix this, we should only be able to generate moves which land outside of the discovered check line,
        // as to reveal the check.
        //if (checksOnly && ((GetDiscoveredChecksBitboard() & kingLSB) == 0)) return moves;

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

    private List<Move> GenerateCastlingMoves(List<Move> moves, ulong allPieces, bool CWK, bool CWQ, bool CBK, bool CBQ, bool white, bool checksOnly = false)
    {
        if (board.IsKingInCheck(white)) return moves;

        // Does white have kingside castling rights and are there no obstructing pieces
        if (white)
        {
            if (CWK && ((0b11ul << 61) & allPieces) == 0 && ((0b1ul << 61) & board.BlackAttackBitboard) == 0)
            {
                /*
                if (!checksOnly | (checksOnly && (Magic.GetRookTargets(board.BlackKingSquare, allPieces) & (1ul<<61)) != 0))
                {
                    Move move = new(60, 62, PieceType.WK, PieceType.EMPTY, Move.KingCastleFlag);
                    if (board.DoesMoveNotPutOwnKingInCheck(move)) moves.Add(move);
                }
                */
                Move move = new(60, 62, PieceType.WK, PieceType.EMPTY, Move.KingCastleFlag);
                if (board.DoesMoveNotPutOwnKingInCheck(move)) moves.Add(move);
            }

            if (CWQ && ((0b111ul << 57) & allPieces) == 0 && ((0b11ul << 58) & board.BlackAttackBitboard) == 0)
            {
                /*
                if (!checksOnly | (checksOnly && (Magic.GetRookTargets(board.BlackKingSquare, allPieces) & (1ul<<59)) != 0))
                {
                    Move move = new(60, 58, PieceType.WK, PieceType.EMPTY, Move.QueenCastleFlag);
                    if (board.DoesMoveNotPutOwnKingInCheck(move)) moves.Add(move);
                }
                */
                Move move = new(60, 58, PieceType.WK, PieceType.EMPTY, Move.QueenCastleFlag);
                if (board.DoesMoveNotPutOwnKingInCheck(move)) moves.Add(move);
            }
        }
        else
        {
            if (CBK && ((0b1100000ul & allPieces) == 0) && ((0b100000ul & board.WhiteAttackBitboard) == 0))
            {
                /*
                if (!checksOnly | (checksOnly && (Magic.GetRookTargets(board.WhiteKingSquare, allPieces) & (1ul<<5)) != 0))
                {
                    Move move = new(4, 6, PieceType.BK, PieceType.EMPTY, Move.KingCastleFlag);
                    if (board.DoesMoveNotPutOwnKingInCheck(move)) moves.Add(move);
                }
                */
                Move move = new(4, 6, PieceType.BK, PieceType.EMPTY, Move.KingCastleFlag);
                if (board.DoesMoveNotPutOwnKingInCheck(move)) moves.Add(move);
            }

            if (CBQ && ((0b1110ul & allPieces) == 0) && ((0b1100) & board.WhiteAttackBitboard) == 0)
            {
                /*
                if (!checksOnly | (checksOnly && (Magic.GetRookTargets(board.WhiteKingSquare, allPieces) & (1ul<<3)) != 0))
                {
                    Move move = new(4, 2, PieceType.BK, PieceType.EMPTY, Move.QueenCastleFlag);
                    if (board.DoesMoveNotPutOwnKingInCheck(move)) moves.Add(move);
                }
                */
                Move move = new(4, 2, PieceType.BK, PieceType.EMPTY, Move.QueenCastleFlag);
                if (board.DoesMoveNotPutOwnKingInCheck(move)) moves.Add(move);
            }
        }

        return moves;
    }

    public List<Move>  GenerateBlackPawnMoves(List<Move> moves, ulong pawnBitboard, ulong friendlyPieces, ulong enemyPieces, ulong epFile, bool capturesOnly = false, bool checksOnly = false)
    {
        bool unsafeMove;

        ulong kingAttackers = board.GetKingAttackers(board.IsWhiteToMove);

        int numAttackers = BitboardUtility.CountSetBits(kingAttackers);
        if (numAttackers > 1) return moves; // only king moves are allowed in double check

        ulong captureMask = ulong.MaxValue; // if the king is in check, this is limited to just the attacking piece(s) bitboard
        ulong pushMask = ulong.MaxValue; // if the king is in check, this is limited to the squares between the king and the attacking piece(s)
        //ulong checkMask = ulong.MaxValue;

        if (numAttackers == 1)
        {
            captureMask = kingAttackers;
            pushMask = MoveGenData.inBetweenLookupTable[board.BlackKingSquare][BitboardUtility.IndexOfLSB(kingAttackers)];
        }

        /*
        if (checksOnly)
        {
            checkMask = MoveGenData.whitePawnAttacks[board.WhiteKingSquare];
        }
        */

        BitboardUtility.ForEachBitscanForward(pawnBitboard, (startingIndex) =>
        {
            ulong pawnLSB = 1ul << startingIndex;

            if ((pawnLSB & GetPinnedPiecesBitboard()) != 0) unsafeMove = true;
            else unsafeMove = false;

            //if (checksOnly && (pawnLSB & GetDiscoveredChecksBitboard()) != 0) checkMask = ulong.MaxValue;

            // Generate attacks
            ulong attackTargets = MoveGenData.blackPawnAttacks[startingIndex];

            // Cache the attack bitboard
            blackPawnAttackBitboard = attackTargets;
            hasCachedBlackPawnAttackBitboard = true;

            // Can only attack enemy pieces which are on the capture mask
            attackTargets &= enemyPieces & captureMask;
            //attackTargets &= enemyPieces & captureMask & checkMask; 

            BitboardUtility.ForEachBitscanForward(attackTargets, (targetIndex) => 
            {
                PieceType capturedPiece = board.GetPieceType(targetIndex);

                // Check if this is a promoting move
                if ((targetIndex / 8) == 7)
                {
                    if (unsafeMove)
                    {
                        Move legalityTest = new(startingIndex, targetIndex, PieceType.BP, capturedPiece, Move.KnightPromoCaptureFlag);
                        if (board.DoesMoveNotPutOwnKingInCheck(legalityTest))
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
                        if (board.DoesMoveNotPutOwnKingInCheck(move))
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
                    if (board.DoesMoveNotPutOwnKingInCheck(move)) moves.Add(move);
                }

                if (((thisPawnBitboard >> 1) & epFile) != 0 && (thisPawnBitboard & MoveGenData.RankMasks[3]) != 0 & (thisPawnBitboard & MoveGenData.FileMasks[0]) == 0)
                {
                    Move move = new(startingIndex, startingIndex+7, PieceType.BP, PieceType.WP, Move.EpCaptureFlag);
                    if (board.DoesMoveNotPutOwnKingInCheck(move)) moves.Add(move);
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
                //target &= pushMask & checkMask;
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
                        if (board.DoesMoveNotPutOwnKingInCheck(move))
                        {
                            moves.Add(move);
                        }
                    }
                }
            }

            // Single pawn push
            target = 1ul << (startingIndex + 8) & ~(friendlyPieces|enemyPieces);
            target &= pushMask;
            //target &= pushMask & checkMask;
            if (target != 0) // check for obstruction
            {
                int targetIndex = BitboardUtility.IndexOfLSB(target);

                // Check if this is a promoting move
                if ((targetIndex / 8) == 7)
                {
                    if (unsafeMove)
                    {
                        Move legalityTest = new(startingIndex, targetIndex, PieceType.BP, PieceType.EMPTY, Move.KnightPromotionFlag);
                        if (board.DoesMoveNotPutOwnKingInCheck(legalityTest))
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
                        if (board.DoesMoveNotPutOwnKingInCheck(move)) moves.Add(move);
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
            ulong attackTargets = MoveGenData.whitePawnAttacks[startingIndex];

            // Cache the attack bitboard
            whitePawnAttackBitboard = attackTargets;
            hasCachedWhitePawnAttackBitboard = true;

            // Can only capture enemy pieces that are on the capture mask
            attackTargets &= enemyPieces & captureMask; 

            BitboardUtility.ForEachBitscanForward(attackTargets, (targetIndex) => 
            {
                PieceType capturedPiece = board.GetPieceType(targetIndex);

                // Check if this is a promoting move
                if ((targetIndex / 8) == 0)
                {
                    if (unsafeMove)
                    {
                        Move legalityTest = new(startingIndex, targetIndex, PieceType.WP, capturedPiece, Move.KnightPromoCaptureFlag);
                        if (board.DoesMoveNotPutOwnKingInCheck(legalityTest))
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
                        if (board.DoesMoveNotPutOwnKingInCheck(move)) moves.Add(move);
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
                    if (board.DoesMoveNotPutOwnKingInCheck(move)) moves.Add(move);
                }

                if (((thisPawnBitboard >> 1) & epFile) != 0 && (thisPawnBitboard & MoveGenData.RankMasks[4]) != 0 && (thisPawnBitboard & MoveGenData.FileMasks[0]) == 0)
                {
                    Move move = new(startingIndex, startingIndex-9, PieceType.WP, PieceType.BP, Move.EpCaptureFlag);
                    if (board.DoesMoveNotPutOwnKingInCheck(move)) moves.Add(move);
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
                        if (board.DoesMoveNotPutOwnKingInCheck(move)) moves.Add(move);
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
                        if (board.DoesMoveNotPutOwnKingInCheck(legalityTest)) 
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
                        if (board.DoesMoveNotPutOwnKingInCheck(move)) moves.Add(move);
                    }
                }
            }
        });

        return moves;
    }

    public List<Move> GenerateQueenMoves(List<Move> moves, ulong queenBitboard, ulong friendlyPieces, ulong enemyPieces, bool white, bool captureOnly = false, bool checksOnly = false)
    {
        bool unsafeMove;

        ulong kingAttackers = board.GetKingAttackers(board.IsWhiteToMove);

        int numAttackers = BitboardUtility.CountSetBits(kingAttackers);
        if (numAttackers > 1) return moves; // only king moves are allowed in double check

        ulong captureMask = ulong.MaxValue; // if the king is in check, this is limited to just the attacking piece(s) bitboard
        ulong pushMask = ulong.MaxValue; // if the king is in check, this is limited to the squares between the king and the attacking piece(s)
        //ulong checkMask = ulong.MaxValue;

        if (numAttackers == 1)
        {
            captureMask = kingAttackers;
            pushMask = MoveGenData.inBetweenLookupTable[white ? board.WhiteKingSquare : board.BlackKingSquare][BitboardUtility.IndexOfLSB(kingAttackers)];
        }

        // Must we only generate captures?
        if (captureOnly)
        {
            captureMask &= enemyPieces;
            pushMask &= enemyPieces;
        }

        /*
        if (checksOnly)
        {
            checkMask = Magic.GetBishopTargets(white ? board.BlackKingSquare : board.WhiteKingSquare, friendlyPieces|enemyPieces) | Magic.GetRookTargets(white ? board.BlackKingSquare : board.WhiteKingSquare, friendlyPieces|enemyPieces);
        }
        */

        ulong queenLSB = BitboardUtility.IsolateLSB(queenBitboard);
        while (queenLSB != 0)
        {
            if ((queenLSB & GetPinnedPiecesBitboard()) != 0) unsafeMove = true;
            else unsafeMove = false;

            //if (checksOnly && (queenLSB & GetDiscoveredChecksBitboard()) != 0) checkMask = ulong.MaxValue;

            int startingIndex = BitboardUtility.IndexOfLSB(queenLSB);

            ulong diagonalBlockerBitboard = Magic.CreateBlockerBitboard((friendlyPieces | enemyPieces) ^ queenLSB, Magic.GetSimplifiedDiagonalMovementMask(startingIndex));
            ulong orthogonalBlockerBitboard = Magic.CreateBlockerBitboard((friendlyPieces | enemyPieces) ^ queenLSB, Magic.GetSimplifiedOrthogonalMovementMask(startingIndex));

            ulong targets = MoveGenData.DiagonalMovesAttackTable[startingIndex][Magic.GetDiagonalAttackTableKey(startingIndex, diagonalBlockerBitboard)] | MoveGenData.OrthogonalMovesAttackTable[startingIndex][Magic.GetOrthogonalAttackTableKey(startingIndex, orthogonalBlockerBitboard)];
            targets &= captureMask|pushMask;
            //targets &= (captureMask|pushMask) & checkMask;

            ulong targetsLSB = BitboardUtility.IsolateLSB(targets);
            while (targetsLSB != 0)
            {
                // Do not add the move if it lands on a friendly piece
                if ((targetsLSB & friendlyPieces) == 0)
                {
                    int targetIndex = BitboardUtility.IndexOfLSB(targetsLSB);
                    int flag = Move.QuietMoveFlag;

                    if ((targetsLSB & enemyPieces) != 0) flag = Move.CaptureFlag;

                    Move move = new(startingIndex, targetIndex, white ? PieceType.WQ : PieceType.BQ, board.GetPieceType(targetsLSB), flag);
                    if (!unsafeMove) moves.Add(move);
                    else
                    {
                        if (board.DoesMoveNotPutOwnKingInCheck(move)) moves.Add(move);
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

    public List<Move> GenerateKnightMoves(List<Move> moves, ulong knightBitboard, ulong friendlyPieces, ulong enemyPieces, bool white, bool capturesOnly = false, bool checksOnly = false)
    {
        bool unsafeMove;

        ulong kingAttackers = board.GetKingAttackers(board.IsWhiteToMove);

        int numAttackers = BitboardUtility.CountSetBits(kingAttackers);
        if (numAttackers > 1) return moves; // only king moves are allowed in double check

        ulong captureMask = ulong.MaxValue; // if the king is in check, this is limited to just the attacking piece(s) bitboard
        ulong pushMask = ulong.MaxValue; // if the king is in check, this is limited to the squares between the king and the attacking piece(s)
        //ulong checkMask = ulong.MaxValue;

        if (numAttackers == 1)
        {
            captureMask = kingAttackers;
            pushMask = MoveGenData.inBetweenLookupTable[white ? board.WhiteKingSquare : board.BlackKingSquare][BitboardUtility.IndexOfLSB(kingAttackers)];
        }

        // Must we only generate captures?
        if (capturesOnly)
        {
            captureMask &= enemyPieces;
            pushMask &= enemyPieces;
        }

        /*
        if (checksOnly)
        {
            checkMask = MoveGenData.knightTargets[white ? board.BlackKingSquare : board.WhiteKingSquare];
        }
        */

        ulong knightLSB = BitboardUtility.IsolateLSB(knightBitboard);
        while (knightLSB != 0)
        {
            if ((knightLSB & GetPinnedPiecesBitboard()) != 0) unsafeMove = true;
            else unsafeMove = false;

            //if (checksOnly && (knightLSB & GetDiscoveredChecksBitboard()) != 0) checkMask = ulong.MaxValue;

            int startingIndex = BitboardUtility.IndexOfLSB(knightLSB);

            ulong targets = MoveGenData.GetKnightTargetsBitboard(startingIndex) & (captureMask|pushMask);//&checkMask;

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
                        if (board.DoesMoveNotPutOwnKingInCheck(move)) moves.Add(move);
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
    public List<Move> GenerateBishopMoves(List<Move> moves, ulong bishopBitboard, ulong friendlyPieces, ulong enemyPieces, bool white, bool capturesOnly = false, bool checksOnly = false) 
    {
        bool unsafeMove;
        ulong kingAttackers = board.GetKingAttackers(board.IsWhiteToMove);

        int numAttackers = BitboardUtility.CountSetBits(kingAttackers);
        if (numAttackers > 1) return moves; // only king moves are allowed in double check

        ulong captureMask = ulong.MaxValue; // if the king is in check, this is limited to just the attacking piece(s) bitboard
        ulong pushMask = ulong.MaxValue; // if the king is in check, this is limited to the squares between the king and the attacking piece(s)
        //ulong checkMask = ulong.MaxValue;

        if (numAttackers == 1)
        {
            captureMask = kingAttackers;
            pushMask = MoveGenData.inBetweenLookupTable[white ? board.WhiteKingSquare : board.BlackKingSquare][BitboardUtility.IndexOfLSB(kingAttackers)];
        }

        // Must we only generate captures?
        if (capturesOnly)
        {
            captureMask &= enemyPieces;
            pushMask &= enemyPieces;
        }


        // Must we only generate checks?
        /*
        if (checksOnly)
        {
            checkMask = Magic.GetBishopTargets(white ? board.BlackKingSquare : board.WhiteKingSquare, friendlyPieces|enemyPieces);
        }
        */

        ulong bishopLSB = BitboardUtility.IsolateLSB(bishopBitboard);
        while (bishopLSB != 0)
        {
            if ((bishopLSB & GetPinnedPiecesBitboard()) != 0) unsafeMove = true;
            else unsafeMove = false;

            // We can move this bishop anywhere if it leads to a discovered check on the enemy king,
            // in the case that we only want to search for checks.
            //if (checksOnly && (bishopLSB & GetDiscoveredChecksBitboard()) != 0) checkMask = ulong.MaxValue;

            int startingIndex = BitboardUtility.IndexOfLSB(bishopLSB);

            // Get the target bitboard
            ulong targets = Magic.GetBishopTargets(startingIndex, friendlyPieces|enemyPieces) & (captureMask|pushMask);//&checkMask;

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
                        if (board.DoesMoveNotPutOwnKingInCheck(move)) moves.Add(move);
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
    public List<Move> GenerateRookMoves(List<Move> moves, ulong rookBitboard, ulong friendlyPieces, ulong enemyPieces, bool white, bool capturesOnly = false, bool checksOnly = false)
    {
        bool unsafeMove;

        ulong kingAttackers = board.GetKingAttackers(board.IsWhiteToMove);

        int numAttackers = BitboardUtility.CountSetBits(kingAttackers);
        if (numAttackers > 1) return moves; // only king moves are allowed in double check

        ulong captureMask = ulong.MaxValue; // if the king is in check, this is limited to just the attacking piece(s) bitboard
        ulong pushMask = ulong.MaxValue; // if the king is in check, this is limited to the squares between the king and the attacking piece(s)
        //ulong checkMask = ulong.MaxValue;

        if (numAttackers == 1)
        {
            captureMask = kingAttackers;
            pushMask = MoveGenData.inBetweenLookupTable[white ? board.WhiteKingSquare : board.BlackKingSquare][BitboardUtility.IndexOfLSB(kingAttackers)];
        }

        if (capturesOnly)
        {
            captureMask &= enemyPieces;
            pushMask &= enemyPieces;
        }

        /*
        if (checksOnly)
        {
            checkMask = Magic.GetRookTargets(white ? board.BlackKingSquare : board.WhiteKingSquare, friendlyPieces|enemyPieces);
        }
        */

        ulong rookLSB = BitboardUtility.IsolateLSB(rookBitboard);
        while (rookLSB != 0)
        {
            if ((rookLSB & GetPinnedPiecesBitboard()) != 0) unsafeMove = true;
            else unsafeMove = false;

            // Even if we only want to generate checks, we need to check if moving this piece anywhere will lead to a discovered attack on the king
            //if ((rookLSB & GetDiscoveredChecksBitboard()) != 0) checkMask = ulong.MaxValue;

            int startingIndex = BitboardUtility.IndexOfLSB(rookLSB);

            // Get the rook targets, AND it with the (push OR capture) masks
            ulong targets = Magic.GetRookTargets(startingIndex, friendlyPieces|enemyPieces) & (captureMask|pushMask);//&checkMask;

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
                        if (board.DoesMoveNotPutOwnKingInCheck(move)) moves.Add(move);
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

    public ulong GetDiscoveredChecksBitboard()
    {
        if (hasCachedDiscoveredChecks) return discoveredChecks;

        if (board.IsWhiteToMove)
        {
            discoveredChecks = MoveGenHelper.GetPinnedPiecesBitboard(board.AllPiecesBitboard, board.WhitePiecesBitboard, board.GetBitboardByPieceType(PieceType.WB), board.GetBitboardByPieceType(PieceType.WR), board.GetBitboardByPieceType(PieceType.WQ), board.BlackKingSquare);
        }
        else
        {
            discoveredChecks = MoveGenHelper.GetPinnedPiecesBitboard(board.AllPiecesBitboard, board.BlackPiecesBitboard, board.GetBitboardByPieceType(PieceType.BB), board.GetBitboardByPieceType(PieceType.BR), board.GetBitboardByPieceType(PieceType.BQ), board.WhiteKingSquare);
        }

        hasCachedDiscoveredChecks = true;

        return discoveredChecks;

    }

    public ulong BlackPawnAttackBitboard(ulong blackPawnBitboard)
    {
        if (hasCachedBlackPawnAttackBitboard) return blackPawnBitboard;

        BitboardUtility.ForEachBitscanForward(blackPawnBitboard, pawnIndex =>
        {
            blackPawnAttackBitboard |= MoveGenData.blackPawnAttacks[pawnIndex];
        });

        hasCachedBlackPawnAttackBitboard = true;
        return blackPawnAttackBitboard;
    }

    public ulong WhitePawnAttackBitboard(ulong whitePawnBitboard)
    {
        if (hasCachedWhitePawnAttackBitboard) return whitePawnAttackBitboard;

        BitboardUtility.ForEachBitscanForward(whitePawnBitboard, pawnIndex => {
            whitePawnAttackBitboard |= MoveGenData.whitePawnAttacks[pawnIndex];
        });

        hasCachedWhitePawnAttackBitboard = true;
        return whitePawnAttackBitboard;
    }

    public void OnBoardStateChange()
    {
        legalMoves.Clear();
        hasCachedLegalMoves = false;
        pinnedPieces = 0ul;
        hasCachedPinnedPieces = false;
        discoveredChecks = 0ul;
        hasCachedDiscoveredChecks = false;
        whitePawnAttackBitboard = 0ul;
        hasCachedWhitePawnAttackBitboard = false;
        blackPawnAttackBitboard = 0ul;
        hasCachedBlackPawnAttackBitboard = false;
    }

}