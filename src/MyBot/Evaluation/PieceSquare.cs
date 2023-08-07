namespace ChessBot;

public class PieceSquare 
{

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

    private int openingVal = 0;
    private int endgameVal = 0;

    public PieceSquare(Board board)
    {
        int openingPositionalScore = 0;
        int endgamePositionalScore = 0;

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.WP), (pawnIndex) => {
            openingPositionalScore += pieceSquareTable[0][0][pawnIndex];

            endgamePositionalScore += pieceSquareTable[1][0][pawnIndex];
        });

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.BP), (pawnIndex) => {
            openingPositionalScore += -pieceSquareTable[0][0][mirror[pawnIndex]];

            endgamePositionalScore += -pieceSquareTable[1][0][mirror[pawnIndex]];
        });

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.WN), (knightIndex) => {
            openingPositionalScore += pieceSquareTable[0][1][knightIndex];

            endgamePositionalScore += pieceSquareTable[1][1][knightIndex];
        });

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.BN), (knightIndex) => {
            openingPositionalScore += -pieceSquareTable[0][1][mirror[knightIndex]];

            endgamePositionalScore += -pieceSquareTable[1][1][mirror[knightIndex]];
        });

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.WB), (bishopIndex) => {
            openingPositionalScore += pieceSquareTable[0][2][bishopIndex];

            endgamePositionalScore += pieceSquareTable[1][2][bishopIndex];
        });

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.BB), (bishopIndex) => {
            openingPositionalScore += -pieceSquareTable[0][2][mirror[bishopIndex]];

            endgamePositionalScore += -pieceSquareTable[1][2][mirror[bishopIndex]];
        });

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.WR), (rookIndex) => {
            openingPositionalScore += pieceSquareTable[0][3][rookIndex];

            endgamePositionalScore += pieceSquareTable[1][3][rookIndex];
        });

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.BR), (rookIndex) => {
            openingPositionalScore += -pieceSquareTable[0][3][mirror[rookIndex]];

            endgamePositionalScore += -pieceSquareTable[1][3][mirror[rookIndex]];
        });

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.WQ), (queenIndex) => {
            openingPositionalScore += pieceSquareTable[0][4][queenIndex];

            endgamePositionalScore += pieceSquareTable[1][4][queenIndex];
        });

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.BQ), (queenIndex) => {
            openingPositionalScore += -pieceSquareTable[0][4][mirror[queenIndex]];

            endgamePositionalScore += -pieceSquareTable[1][4][mirror[queenIndex]];
        });

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.WK), (kingIndex) => {
            openingPositionalScore += pieceSquareTable[0][5][kingIndex];

            endgamePositionalScore += pieceSquareTable[1][5][kingIndex];
        });

        BitboardUtility.ForEachBitscanForward(board.GetBitboardByPieceType(PieceType.BK), (kingIndex) => {
            openingPositionalScore += -pieceSquareTable[0][5][mirror[kingIndex]];

            endgamePositionalScore += -pieceSquareTable[1][5][mirror[kingIndex]];
        });

        openingVal = openingPositionalScore;
        endgameVal = endgamePositionalScore;
    }


    public void Undo(Move move)
    {
        PieceType movingPiece = move.MovingPiece;
        int colour = PieceUtility.Colour(movingPiece);
        int to = move.To;
        int from = move.From;
        PieceType capturedPiece;

        if (move.GetPromotionType() != null)
        {
            if (colour == 0)
            {
                openingVal -= pieceSquareTable[0][(int)move.GetPromotionType()][to];
                endgameVal -= pieceSquareTable[1][(int)move.GetPromotionType()][to];

                // Add back the promoting pawn
                openingVal += pieceSquareTable[0][0][from];
                endgameVal += pieceSquareTable[1][0][from];

                if (!move.IsCapture()) return;

                // Add back the captured piece
                capturedPiece = move.CapturedPiece;
                openingVal += pieceSquareTable[0][(int)capturedPiece - 6][to];
                endgameVal += pieceSquareTable[1][(int)capturedPiece - 6][to];
            }
            else
            {
                openingVal += pieceSquareTable[0][(int)move.GetPromotionType()][mirror[to]];
                endgameVal += pieceSquareTable[1][(int)move.GetPromotionType()][mirror[to]];

                // Add back the promoting pawn
                openingVal -= pieceSquareTable[0][0][mirror[from]];
                endgameVal -= pieceSquareTable[1][0][mirror[from]];

                if (!move.IsCapture()) return;

                // Add back the captured piece
                capturedPiece = move.CapturedPiece;
                openingVal -= pieceSquareTable[0][(int)capturedPiece][mirror[to]];
                endgameVal -= pieceSquareTable[1][(int)capturedPiece][mirror[to]];
            }

            return;
        }

        if (colour == 0)
        {
            // Remove To
            openingVal -= pieceSquareTable[0][(int)movingPiece][to];
            endgameVal -= pieceSquareTable[1][(int)movingPiece][to];

            // Add from
            openingVal += pieceSquareTable[0][(int)movingPiece][from];
            endgameVal += pieceSquareTable[1][(int)movingPiece][from];

            if (!move.IsCapture()) return;

            // Add back the captured piece
            capturedPiece = move.CapturedPiece;
            openingVal += pieceSquareTable[0][(int)capturedPiece - 6][to];
            endgameVal += pieceSquareTable[1][(int)capturedPiece - 6][to];
        }
        else
        {
            // Remove To
            openingVal += pieceSquareTable[0][(int)movingPiece - 6][mirror[to]];
            endgameVal += pieceSquareTable[1][(int)movingPiece - 6][mirror[to]];

            // Add from
            openingVal -= pieceSquareTable[0][(int)movingPiece - 6][mirror[from]];
            endgameVal -= pieceSquareTable[1][(int)movingPiece - 6][mirror[from]];

            if (!move.IsCapture()) return;

            // Add back the captured piece
            capturedPiece = move.CapturedPiece;
            openingVal -= pieceSquareTable[0][(int)capturedPiece][mirror[to]];
            endgameVal -= pieceSquareTable[1][(int)capturedPiece][mirror[to]];
        }
    }

    public void Update(Move move)
    {
        PieceType movingPiece = move.MovingPiece;
        int colour = PieceUtility.Colour(movingPiece);
        int to = move.To;
        int from = move.From;
        PieceType capturedPiece;

        if (move.GetPromotionType() != null)
        {
            if (colour == 0)
            {
                // Add the promoting piece
                openingVal += pieceSquareTable[0][(int)move.GetPromotionType()][to];
                endgameVal += pieceSquareTable[1][(int)move.GetPromotionType()][to];

                // Remove the promoting pawn
                openingVal -= pieceSquareTable[0][0][from];
                endgameVal -= pieceSquareTable[1][0][from];

                if (!move.IsCapture()) return;

                // Remove the captured piece
                capturedPiece = move.CapturedPiece;
                openingVal -= pieceSquareTable[0][(int)capturedPiece - 6][to];
                endgameVal -= pieceSquareTable[1][(int)capturedPiece - 6][to];
            }
            else
            {
                // Add the promoting piece
                openingVal -= pieceSquareTable[0][(int)move.GetPromotionType()][mirror[to]];
                endgameVal -= pieceSquareTable[1][(int)move.GetPromotionType()][mirror[to]];

                // Remove the promoting pawn
                openingVal += pieceSquareTable[0][0][mirror[from]];
                endgameVal += pieceSquareTable[1][0][mirror[from]];

                if (!move.IsCapture()) return;

                // Remove the captured piece
                capturedPiece = move.CapturedPiece;
                openingVal += pieceSquareTable[0][(int)capturedPiece][mirror[to]];
                endgameVal += pieceSquareTable[1][(int)capturedPiece][mirror[to]];
            }

            return;
        }

        if (colour == 0)
        {
            // Add To
            openingVal += pieceSquareTable[0][(int)movingPiece][to];
            endgameVal += pieceSquareTable[1][(int)movingPiece][to];

            // Remove from
            openingVal -= pieceSquareTable[0][(int)movingPiece][from];
            endgameVal -= pieceSquareTable[1][(int)movingPiece][from];

            if (!move.IsCapture()) return;

            // Remove the captured piece
            capturedPiece = move.CapturedPiece;
            openingVal -= pieceSquareTable[0][(int)capturedPiece - 6][to];
            endgameVal -= pieceSquareTable[1][(int)capturedPiece - 6][to];
        }
        else
        {
            // Add to
            openingVal -= pieceSquareTable[0][(int)movingPiece - 6][mirror[to]];
            endgameVal -= pieceSquareTable[1][(int)movingPiece - 6][mirror[to]];

            // Remove from
            openingVal += pieceSquareTable[0][(int)movingPiece - 6][mirror[from]];
            endgameVal += pieceSquareTable[1][(int)movingPiece - 6][mirror[from]];

            if (!move.IsCapture()) return;

            // Remove the captured piece
            capturedPiece = move.CapturedPiece;
            openingVal += pieceSquareTable[0][(int)capturedPiece][mirror[to]];
            endgameVal += pieceSquareTable[1][(int)capturedPiece][mirror[to]];
        }
    }

    public int OpeningValue => openingVal;
    public int EndgameValue => endgameVal;

    public double TaperedValue(GamePhase gamePhase) 
    {
        // Tapered evaluation:
        return (openingVal * gamePhase.Value + endgameVal * (24 - gamePhase.Value))/24.0;
    }


}