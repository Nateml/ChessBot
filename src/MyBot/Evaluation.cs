namespace ChessBot;
using System.ComponentModel;
using System.Reflection.Metadata;
using ChessBot;

public static class Evaluation
{

    /// <summary>
    /// Values for the different piece types.
    /// Indexed first by opening/endgame, and then by PieceType.
    /// </summary>
    public static readonly int[][] materialWeights = {
        // Opening material weights
        new int[] {89, 308, 319, 488, 888, 20001, -92, -307, -323, -492, -888, -20002},

        // Endgame material weights
        new int[] {96, 319, 331, 497, 853, 19998, -102, -318, -334, -501, -845, -20000}
    };

    /// <summary>
    /// Piece-square table.
    /// Index by opening/endgame, then piece, then square.
    /// Squares are index from white's viewpoint, so to get values for black use the mirror array.
    /// </summary>
    public static readonly int[][][] pieceSquareTable = {
        // Opening phase scores
        new int[][]
        {
            // Pawn
            new int[] 
            {
                0,   0,   0,   0,   0,   0,   0,   0,   
                -4,  68,  61,  47,  47,  49,  45,  -1,   
                6,  16,  25,  33,  24,  24,  14,  -6,
                0,  -1,   9,  28,  20,   8,  -1,  11,
                6,   4,   6,  14,  14,  -5,   6,  -6,   
                -1,  -8,  -4,   4,   2, -12,  -1,   5,   
                5,  16,  16, -14, -14,  13,  15,   8,   
                0,   0,   0,   0,   0,   0,   0,   0
            },
            // Knight
            new int[]
            {
                -55, -40, -30, -28, -26, -30, -40, -50,   
                -37, -15,   0,  -6,   4,   3, -17, -40,   
                -25,   5,  16,  12,  11,   6,   6, -29,   
                -24,   5,  21,  14,  18,   9,  11, -26,   
                -36,  -5,   9,  23,  24,  21,   2, -24,   
                -32,  -1,   4,  19,  20,   4,  11, -25,   
                -38, -22,   4,  -1,   8,  -5, -18, -34,   
                -50, -46, -32, -24, -36, -25, -34, -50,   
            },
            // Bishop
            new int[]
            {
                -16, -15, -12,  -5, -10, -12, -10, -20,   
                -13,   5,   6,   1,  -6,  -5,   3,  -6,   
                -16,   6,  -1,  16,   7,  -1,  -6,  -5,   
                -14,  -1,  11,  14,   4,  10,  11, -13,   
                -4,   5,  12,  16,   4,   6,   2, -16,   
                -15,   4,  14,   8,  16,   4,  16, -15,   
                -5,   6,   6,   6,   3,   6,   9,  -7,   
                -14,  -4, -15,  -4,  -9,  -4, -12, -14,   
            },
            // Rook
            new int[]
            {
                5,  -2,   6,   2,  -2,  -6,   4,  -2,   
                8,  13,  11,  15,  11,  15,  16,   4,   
                -6,   3,   3,   6,   1,  -2,   3,  -5,   
                -10,   5,  -4,  -4,  -1,  -6,   3,  -2,   
                -4,   3,   5,  -2,   4,   1,  -5,   1,   
                0,   1,   1,  -3,   5,   6,   1,  -9,   
                -10,  -1,  -4,   0,   5,  -6,  -6,  -9,   
                -1,  -2,  -6,   9,   9,   5,   4,  -5,   
            },
            // Queen
            new int[]
            {
                -25,  -9, -11,  -3,  -7, -13, -10, -17,   
                -4,  -6,   4,  -5,  -1,   6,   4,  -5,   
                -8,  -5,   2,   0,   7,   6,  -4,  -5,   
                0,  -4,   7,  -1,   7,  11,   0,   1,   
                -6,   4,   7,   1,  -1,   2,  -6,  -2,   
                -15,  11,  11,  11,   4,  11,   6, -15,   
                -5,  -6,   1,  -6,   3,  -3,   3, -10,   
                -15,  -4, -13,  -8,  -3, -16,  -8, -24,   
            },
            // King
            new int[]
            {
                -30, -40, -40, -50, -50, -40, -40, -30,   
                -30, -37, -43, -49, -50, -39, -40, -30,   
                -32, -41, -40, -46, -49, -40, -46, -30,   
                -32, -38, -39, -52, -54, -39, -39, -30,   
                -20, -33, -29, -42, -44, -29, -30, -19,   
                -10, -18, -17, -20, -22, -21, -20, -13,   
                14,  18,  -1,  -1,   4,  -1,  15,  14,   
                21,  35,  11,   6,   1,  14,  32,  22,    
            }
        },

        // Endgame phase scores
        new int[][]
        {
            // Pawn
            new int[]
            {
                0,   0,   0,   0,   0,   0,   0,   0,   
                -4, 174, 120,  94,  85,  98,  68,   4,   
                6,  48,  44,  45,  31,  38,  37,  -6,   
                -6,  -4,  -1,  -6,   2,  -1,  -2,  -2,   
                2,   2,   5,  -3,   0,  -5,   4,  -3,   
                -2,   0,   1,   5,   0,  -1,   0,   1,   
                -2,   5,   6,  -6,   0,   3,   4,  -4,   
                0,   0,   0,   0,   0,   0,   0,   0,   
            },
            // Knight
            new int[]
            {
                -50, -40, -30, -24, -24, -35, -40, -50,   
                -38, -17,   6,  -5,   5,  -4, -15, -40,   
                -24,   3,  15,   9,  15,  10,  -6, -26,   
                -29,   5,  21,  17,  18,   9,  10, -28,   
                -36,  -5,  18,  16,  14,  20,   5, -26,   
                -32,   7,   5,  20,  11,  15,   9, -27,   
                -43, -20,   5,  -1,   5,   1, -22, -40,   
                -50, -40, -32, -27, -30, -25, -35, -50,   
            },
            // Bishop
            new int[]
            {
                -14, -13,  -4,  -7, -14,  -9, -16, -20,   
                -11,   6,   3,  -6,   4,  -3,   5,  -4,   
                -11,  -3,   5,  15,   4,  -1,  -5, -10,   
                -7,  -1,  11,  16,   5,  11,   7, -13,   
                -4,   4,  10,  16,   6,  12,   4, -16,   
                -4,   4,  11,  12,  10,   7,   7, -12,   
                -11,   7,   6,   6,  -3,   2,   1,  -7,   
                -15,  -4, -11,  -4, -10, -10,  -6, -17,   
            },
            // Rook
            new int[]
            {
                5,  -6,   1,  -4,  -4,  -6,   6,  -3,   
                -6,   4,   2,   5,  -1,   3,   4, -15,   
                -15,   3,   3,   0,  -1,  -6,   5,  -9,   
                -16,   6,   0,  -6,  -3,  -3,  -4,  -4,   
                -15,   6,   2,  -6,   6,   0,  -6, -10,   
                -6,  -1,   3,  -2,   6,   5,   0, -15,   
                -8,  -4,   1,  -4,   3,  -5,  -6,  -5,   
                1,   0,  -2,   1,   1,   4,   2,   0,   
            },
            // Queen
            new int[]
            {
                -21,  -7,  -6,   1,  -8, -15, -10, -16,   
                -4,  -5,   3,  -4,   2,   6,   3, -10,   
                -13,  -2,   7,   2,   6,  10,  -4,  -6,   
                -1,  -4,   3,   1,   8,   8,  -2,  -2,   
                0,   6,   8,   1,  -1,   1,   0,  -3,   
                -11,  10,   6,   3,   7,   9,   4, -10,   
                -12,  -6,   5,   0,   0,  -5,   4, -10,   
                -20,  -6,  -7,  -7,  -4, -12,  -9, -20,    
            },
            // King
            new int[]
            {
                -50, -40, -30, -20, -20, -30, -40, -50,   
                -30, -18, -15,   6,   3,  -6, -24, -30,   
                -35, -16,  20,  32,  34,  14, -11, -30,   
                -34,  -5,  24,  35,  34,  35, -16, -35,   
                -36,  -7,  31,  34,  34,  34, -12, -31,   
                -30,  -7,  14,  33,  36,  16, -13, -33,   
                -36, -27,   5,   2,   5,  -1, -31, -33,   
                -48, -26, -26, -26, -28, -25, -30, -51,   
            }
        }
    };

    /// <summary>
    /// Stores mirrored squares.
    /// USAGE:
    /// To get the correct piece square table value for a black piece on square i, access mirror[i] to get the appropriate square index.
    /// </summary>
    public static readonly int[] mirror = {
        56, 57, 58, 59, 60, 61, 62, 63,
        48, 49, 50, 51, 52, 53, 54, 55,
        40, 41, 42, 43, 44, 45, 46, 47,
        32, 33, 34, 35, 36, 37, 38, 39,
        24, 25, 26, 27, 28, 29, 30, 31,
        16, 17, 18, 19, 20, 21, 22, 23,
        8,   9, 10, 11, 12, 13, 14, 15,
        0,   1,  2,  3,  4,  5,  6,  7    
    };


    /// <summary>
    /// Used to increment the game phase value.
    /// Indexed as follows:
    /// 0: pawn
    /// 1: knight
    /// 2: bishop
    /// 3: rook
    /// 4: queen
    /// 5: king
    /// </summary>
    public static readonly int[] gamePhaseInc = {0, 1, 1, 2, 4, 0};

    public static int cachedGamePhase = 24;

    /// <summary>
    /// Returns a static evaluation of the board.
    /// Works with the negamax framework, since the returned evaluation is relative to the player who's turn it is to move.
    /// i.e. a positive evaluation is always good for the player who's turn it is to move
    /// </summary>
    public static int EvaluateBoard(Board board)
    {
        int gamePhase = 0;

        int openingScore = 0;
        int endgameScore = 0;

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.WP), (pawnIndex) => {
            openingScore += materialWeights[0][0];
            openingScore += pieceSquareTable[0][0][pawnIndex];

            endgameScore += materialWeights[1][0];
            endgameScore += pieceSquareTable[1][0][pawnIndex];
        });

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.BP), (pawnIndex) => {
            openingScore += materialWeights[0][6];
            openingScore += -pieceSquareTable[0][0][mirror[pawnIndex]];

            endgameScore += materialWeights[1][6];
            endgameScore += -pieceSquareTable[1][0][mirror[pawnIndex]];
        });

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.WN), (knightIndex) => {
            openingScore += materialWeights[0][1];
            openingScore += pieceSquareTable[0][1][knightIndex];

            endgameScore += materialWeights[1][1];
            endgameScore += pieceSquareTable[1][1][knightIndex];

            gamePhase += gamePhaseInc[1];
        });

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.BN), (knightIndex) => {
            openingScore += materialWeights[0][7];
            openingScore += -pieceSquareTable[0][1][mirror[knightIndex]];

            endgameScore += materialWeights[1][7];
            endgameScore += -pieceSquareTable[1][1][mirror[knightIndex]];

            gamePhase += gamePhaseInc[1];
        });

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.WB), (bishopIndex) => {
            openingScore += materialWeights[0][2];
            openingScore += pieceSquareTable[0][2][bishopIndex];

            endgameScore += materialWeights[1][2];
            endgameScore += pieceSquareTable[1][2][bishopIndex];

            gamePhase += gamePhaseInc[2];
        });

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.BB), (bishopIndex) => {
            openingScore += materialWeights[0][8];
            openingScore += -pieceSquareTable[0][2][mirror[bishopIndex]];

            endgameScore += materialWeights[1][8];
            endgameScore += -pieceSquareTable[1][2][mirror[bishopIndex]];

            gamePhase += gamePhaseInc[2];
        });

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.WR), (rookIndex) => {
            openingScore += materialWeights[0][3];
            openingScore += pieceSquareTable[0][3][rookIndex];

            endgameScore += materialWeights[1][3];
            endgameScore += pieceSquareTable[1][3][rookIndex];

            gamePhase += gamePhaseInc[3];
        });

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.BR), (rookIndex) => {
            openingScore += materialWeights[0][9];
            openingScore += -pieceSquareTable[0][3][mirror[rookIndex]];

            endgameScore += materialWeights[1][9];
            endgameScore += -pieceSquareTable[1][3][mirror[rookIndex]];

            gamePhase += gamePhaseInc[3];
        });

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.WQ), (queenIndex) => {
            openingScore += materialWeights[0][4];
            openingScore += pieceSquareTable[0][4][queenIndex];

            endgameScore += materialWeights[1][4];
            endgameScore += pieceSquareTable[1][4][queenIndex];

            gamePhase += gamePhaseInc[4];
        });

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.BQ), (queenIndex) => {
            openingScore += materialWeights[0][10];
            openingScore += -pieceSquareTable[0][4][mirror[queenIndex]];

            endgameScore += materialWeights[1][10];
            endgameScore += -pieceSquareTable[1][4][mirror[queenIndex]];

            gamePhase += gamePhaseInc[4];
        });

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.WK), (kingIndex) => {
            openingScore += materialWeights[0][5];
            openingScore += pieceSquareTable[0][5][kingIndex];

            endgameScore += materialWeights[1][5];
            endgameScore += pieceSquareTable[1][5][kingIndex];
        });

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.BK), (kingIndex) => {
            openingScore += materialWeights[0][11];
            openingScore += -pieceSquareTable[0][5][mirror[kingIndex]];

            endgameScore += materialWeights[1][11];
            endgameScore += -pieceSquareTable[1][5][mirror[kingIndex]];
        });

        // Tapered evaluation:

        if (gamePhase > 24) gamePhase = 24; // In case of early promotion

        cachedGamePhase = gamePhase;

        int endGamePhase = 24 - gamePhase;

        return (openingScore * gamePhase + endgameScore * endGamePhase) / 24 * (board.IsWhiteToMove ? 1 : -1);
    }

    public static int EvaluateMove(Move move, Board board)
    {
        int score = 0;

        if (move.IsCapture())
        {
            score = 10*GetPieceValue(move.CapturedPiece) - GetPieceValue(move.MovingPiece);
        }


        if (move.IsPromotion())
        {
            switch (move.Flag)
            {
                    case Move.KnightPromoCaptureFlag:
                    case Move.KnightPromotionFlag:
                        score += GetPieceValue(PieceType.WN);
                        break;
                    case Move.BishopPromoCaptureFlag:
                    case Move.BishopPromotionFlag:
                        score += GetPieceValue(PieceType.WB);
                        break;
                    case Move.RookPromoCaptureFlag:
                    case Move.RookPromotionFlag:
                        score += GetPieceValue(PieceType.WR);
                        break;
                    case Move.QueenPromoCaptureFlag:
                    case Move.QueenPromotionFlag:
                        score += GetPieceValue(PieceType.WQ);
                        break;
            }
        }

        if (cachedGamePhase > 12)
        {
            score += GetOpeningPieceSquareScore(move.MovingPiece, move.To);
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

    public static int GetPieceValue(PieceType piece)
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
}