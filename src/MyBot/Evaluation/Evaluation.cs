namespace ChessBot;
using System.ComponentModel;
using System.Reflection.Metadata;
using ChessBot;
using static PieceSquare;
using static MaterialEvaluation;

public static class Evaluation
{

    public const double PositionalWeight = 0.4;
    public const int KingInCheckPenalty = 60;
    public const int CastlingBonus = 5; // This probably needs to be very small because otherwise it would be better not to castle
    public const int BishopPairBonus = 40;
    public const int RookOnOpenFileBonus = 20;
    public const int RookOnSemiOpenFileBonus = 10;
    public const int KnightMobilityBonus = 5;
    public const int BishopMobilityBonus = 5;
    public const int RookMobilityBonus = 5;


    /// <summary>
    /// Returns a static evaluation of the board.
    /// </summary>
    public static int EvaluateBoard(Board board, EvaluationManager evalManager)
    {
        // We keep track of seperate opening/endgame score
        // which are then tapered according to the game phase, 
        // which is a continuous estimate, according to piece values (see gamePhaseInc),
        // of what game phase we are in.
        // These scores are calculated by the material weights of the player's pieces,
        // as well as a piece-squares table (multiplied by a positional weight)
        // to evaluation the position.

        int openingPositionalScore = 0;
        int endgamePositionalScore = 0;

        //int knightMobility = 0;

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.WN), (knightIndex) => {
            // Calculate knight mobility
            // knightMobility += BitboardUtility.CountSetBits(MoveGenData.knightTargets[knightIndex] & ~(board.WhitePiecesBitboard | board.BlackPawnAttackBitboard));

            // Penalty for having a minor piece on an undefended square
            if (!BitboardUtility.IsBitSet(board.WhiteAttackBitboard, knightIndex))
            {
                openingPositionalScore -= GetOpeningPieceValue(PieceType.WN);
                endgamePositionalScore -= GetEndgamePieceValue(PieceType.WN);
            }
        });

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.BN), (knightIndex) => {
            // Calculate knight mobility
            // knightMobility -= BitboardUtility.CountSetBits(MoveGenData.knightTargets[knightIndex] & ~(board.BlackPiecesBitboard | board.WhitePawnAttackBitboard));

            if (!BitboardUtility.IsBitSet(board.BlackAttackBitboard, knightIndex))
            {
                openingPositionalScore += GetOpeningPieceValue(PieceType.BN);
                endgamePositionalScore += GetEndgamePieceValue(PieceType.BN);
            }
        });

        int whiteBishopCount = 0;
        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.WB), (bishopIndex) => {
            whiteBishopCount++;

            if (!BitboardUtility.IsBitSet(board.WhiteAttackBitboard, bishopIndex))
            {
                openingPositionalScore -= GetOpeningPieceValue(PieceType.WB);
                endgamePositionalScore -= GetEndgamePieceValue(PieceType.WB);
            }
        });

        int blackBishopCount = 0;
        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.BB), (bishopIndex) => {
            blackBishopCount++;
            if (!BitboardUtility.IsBitSet(board.BlackAttackBitboard, bishopIndex))
            {
                openingPositionalScore += GetOpeningPieceValue(PieceType.BB);
                endgamePositionalScore += GetEndgamePieceValue(PieceType.BB);
            }
        });

        int rookOnOpenFileBonus = 0;

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.WR), (rookIndex) => {
            // Bonus for having a rook on an open file
            // An open file is a file with no pawns of either color
            // I can determine if the file is open by ANDing the file mask with the pawn bitboards
            // If the result is 0, then the file is open
            if ((MoveGenData.FileMasks[rookIndex % 8] & (board.GetBitboardByPieceType(PieceType.WP) | board.GetBitboardByPieceType(PieceType.BP))) == 0)
            {
                // Open file
                rookOnOpenFileBonus += RookOnOpenFileBonus;
            }
            else if ((MoveGenData.FileMasks[rookIndex % 8] & board.GetBitboardByPieceType(PieceType.BP)) == 0)
            {
                // Semi-open file
                rookOnOpenFileBonus += RookOnSemiOpenFileBonus;
            }
        });

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.BR), (rookIndex) => {
            if ((MoveGenData.FileMasks[rookIndex % 8] & (board.GetBitboardByPieceType(PieceType.WP) | board.GetBitboardByPieceType(PieceType.BP))) == 0)
            {
                // Open file
                rookOnOpenFileBonus -= RookOnOpenFileBonus;
            }
            else if ((MoveGenData.FileMasks[rookIndex % 8] & board.GetBitboardByPieceType(PieceType.WP)) == 0)
            {
                // Semi-open file
                rookOnOpenFileBonus -= RookOnSemiOpenFileBonus;
            }
        });

        int eval = (int)(evalManager.MaterialScore + PositionalWeight * evalManager.PieceSquareScore);

        eval += (int)(PositionalWeight * (openingPositionalScore * evalManager.GamePhase + endgamePositionalScore * (24-evalManager.GamePhase)) / 24.0);

        eval += rookOnOpenFileBonus;

        // eval += knightMobility * KnightMobilityBonus;

        // Penalty for being in check:
        if (board.IsKingInCheck(true))
        {
            eval -= KingInCheckPenalty;
        }
        else if (board.IsKingInCheck(false))
        {
            eval += KingInCheckPenalty;
        }

        // Bonus for having castling rights
        if (board.CanWhiteCastleKingside() || board.CanWhiteCastleQueenside())
        {
            eval += CastlingBonus;
        }
        if (board.CanBlackCastleKingside() || board.CanBlackCastleQueenside())
        {
            eval -= CastlingBonus;
        }

        // Bonus for having a pair of bishops
        if (whiteBishopCount >= 2)
        {
            eval += BishopPairBonus;
        }
        if (blackBishopCount >= 2)
        {
            eval -= BishopPairBonus;
        }

        // Bonus for having a rook on an open file


        return eval;
    }

    public static int EvaluateMove(Move move, Board board, KillerMoves killerMoves, int distanceFromRoot, int gamePhase)
    {
        int score = 0;

        const int KillerValue = 1000;

        if (move.IsCapture())
        {
            score = 100*GetOpeningPieceValue(move.CapturedPiece) - GetOpeningPieceValue(move.MovingPiece);
        }
        else if (killerMoves.Contains(move, distanceFromRoot))
        {
            score = KillerValue;
        }

        if (move.IsPromotion())
        {
            switch (move.Flag)
            {
                    case Move.KnightPromoCaptureFlag:
                    case Move.KnightPromotionFlag:
                        score += GetOpeningPieceValue(PieceType.WN);
                        break;
                    case Move.BishopPromoCaptureFlag:
                    case Move.BishopPromotionFlag:
                        score += GetOpeningPieceValue(PieceType.WB);
                        break;
                    case Move.RookPromoCaptureFlag:
                    case Move.RookPromotionFlag:
                        score += GetOpeningPieceValue(PieceType.WR);
                        break;
                    case Move.QueenPromoCaptureFlag:
                    case Move.QueenPromotionFlag:
                        score += GetOpeningPieceValue(PieceType.WQ);
                        break;
            }
        }

        if (gamePhase > 12)
        {
            score += GetOpeningPieceSquareScore(move.MovingPiece, move.To);
        }

        if (board.SquareIsUnderAttackByEnemyPawn(move.To))
        {
            score -= GetOpeningPieceValue(move.MovingPiece);
        }

        return score;
    }

    public static int GetOpeningPieceSquareScore(PieceType piece, int square)
    {
        return piece switch
        {
            PieceType.WP => pieceSquareTable[0][0][square],
            PieceType.BP => pieceSquareTable[0][0][mirror[square]],
            PieceType.WN => pieceSquareTable[0][1][square],
            PieceType.BN => pieceSquareTable[0][1][mirror[square]],
            PieceType.WB => pieceSquareTable[0][2][square],
            PieceType.BB => pieceSquareTable[0][2][mirror[square]],
            PieceType.WR => pieceSquareTable[0][3][square],
            PieceType.BR => pieceSquareTable[0][3][mirror[square]],
            PieceType.WQ => pieceSquareTable[0][4][square],
            PieceType.BQ => pieceSquareTable[0][4][mirror[square]],
            PieceType.WK => pieceSquareTable[0][5][square],
            PieceType.BK => pieceSquareTable[0][5][mirror[square]],
            _ => 0,
        };
    }

    public static int GetOpeningPieceValue(PieceType piece)
    {
        return piece switch
        {
            PieceType.WP or PieceType.BP => materialWeights[0][0],
            PieceType.WN or PieceType.BN => materialWeights[0][1],
            PieceType.WB or PieceType.BB => materialWeights[0][2],
            PieceType.WR or PieceType.BR => materialWeights[0][3],
            PieceType.WQ or PieceType.BQ => materialWeights[0][4],
            _ => -1,
        };
    }

    public static int GetEndgamePieceValue(PieceType piece)
    {
        return piece switch
        {
            PieceType.WP or PieceType.BP => materialWeights[1][0],
            PieceType.WN or PieceType.BN => materialWeights[1][1],
            PieceType.WB or PieceType.BB => materialWeights[1][2],
            PieceType.WR or PieceType.BR => materialWeights[1][3],
            PieceType.WQ or PieceType.BQ => materialWeights[1][4],
            _ => -1,
        };
    }
}