namespace ChessBot;

public class MaterialEvaluation
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

    private int openingValue = 0;
    private int endgameValue = 0;

    public MaterialEvaluation(Board board)
    {
        int nWP = BitboardUtility.CountSetBits(board.GetBitboardByPieceType(PieceType.WP));
        int nWN = BitboardUtility.CountSetBits(board.GetBitboardByPieceType(PieceType.WN));
        int nWB = BitboardUtility.CountSetBits(board.GetBitboardByPieceType(PieceType.WB));
        int nWR = BitboardUtility.CountSetBits(board.GetBitboardByPieceType(PieceType.WR));
        int nWQ = BitboardUtility.CountSetBits(board.GetBitboardByPieceType(PieceType.WQ));
        int nWK = BitboardUtility.CountSetBits(board.GetBitboardByPieceType(PieceType.WK));
        int nBP = BitboardUtility.CountSetBits(board.GetBitboardByPieceType(PieceType.BP));
        int nBN = BitboardUtility.CountSetBits(board.GetBitboardByPieceType(PieceType.BN));
        int nBB = BitboardUtility.CountSetBits(board.GetBitboardByPieceType(PieceType.BB));
        int nBR = BitboardUtility.CountSetBits(board.GetBitboardByPieceType(PieceType.BR));
        int nBQ =  BitboardUtility.CountSetBits(board.GetBitboardByPieceType(PieceType.BQ));
        int nBK =  BitboardUtility.CountSetBits(board.GetBitboardByPieceType(PieceType.BK));

        openingValue += materialWeights[0][0] * nWP;
        openingValue += materialWeights[0][1] * nWN;
        openingValue += materialWeights[0][2] * nWB;
        openingValue += materialWeights[0][3] * nWR;
        openingValue += materialWeights[0][4] * nWQ;
        openingValue += materialWeights[0][5] * nWK;
        openingValue += materialWeights[0][6] * nBP;
        openingValue += materialWeights[0][7] * nBN;
        openingValue += materialWeights[0][8] * nBB;
        openingValue += materialWeights[0][9] * nBR;
        openingValue += materialWeights[0][10] * nBQ;
        openingValue += materialWeights[0][11] * nBK;

        endgameValue += materialWeights[1][0] * nWP;
        endgameValue += materialWeights[1][1] * nWN;
        endgameValue += materialWeights[1][2] * nWB;
        endgameValue += materialWeights[1][3] * nWR;
        endgameValue += materialWeights[1][4] * nWQ;
        endgameValue += materialWeights[1][5] * nWK;
        endgameValue += materialWeights[1][6] * nBP;
        endgameValue += materialWeights[1][7] * nBN;
        endgameValue += materialWeights[1][8] * nBB;
        endgameValue += materialWeights[1][9] * nBR;
        endgameValue += materialWeights[1][10] * nBQ;
        endgameValue += materialWeights[1][11] * nBK;
    }

    public void Undo(Move move)
    {
        if (!(move.IsCapture() || move.IsPromotion())) return;

        int colour = PieceUtility.Colour(move.MovingPiece);

        if (move.IsPromotion())
        {
            // Remove the promoted piece:
            openingValue -= materialWeights[0][colour == 0 ? (int)move.GetPromotionType() : (int)move.GetPromotionType() + 6];
            endgameValue -= materialWeights[1][colour == 0 ? (int)move.GetPromotionType() : (int)move.GetPromotionType() + 6];

            // Add back the pawn
            openingValue += materialWeights[0][colour == 0 ? 0 : 6];
            endgameValue += materialWeights[1][colour == 0 ? 0 : 6];
        }

        if (move.IsCapture())
        {
            // Add back the captured piece
            openingValue += materialWeights[0][(int)move.CapturedPiece];
            endgameValue += materialWeights[1][(int)move.CapturedPiece];
        }
    }

    public void Update(Move move)
    {
        if (!(move.IsCapture() || move.IsPromotion())) return; // No change in material, we don't have to do anything

        int colour = PieceUtility.Colour(move.MovingPiece);

        if (move.IsPromotion())
        {
            // Add the promoted piece:
            openingValue += materialWeights[0][colour == 0 ? (int)move.GetPromotionType() : (int)move.GetPromotionType() + 6];
            endgameValue += materialWeights[1][colour == 0 ? (int)move.GetPromotionType() : (int)move.GetPromotionType() + 6];
            
            // Remove the pawn
            openingValue -= materialWeights[0][colour == 0 ? 0 : 6];
            endgameValue -= materialWeights[1][colour == 0 ? 0 : 6];

        }

        if (move.IsCapture())
        {
            // Remove the captured piece
            openingValue -= materialWeights[0][(int)move.CapturedPiece];
            endgameValue -= materialWeights[1][(int)move.CapturedPiece];
        }
    }

    public int OpeningValue => openingValue;
    public int EndgameValue => endgameValue;

    public double TaperedValue(GamePhase gamePhase)
    {
        // Tapered evaluation:
        return (openingValue * gamePhase.Value + endgameValue * (24 - gamePhase.Value))/24.0;
    }
}