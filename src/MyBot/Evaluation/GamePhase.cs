namespace ChessBot;

public class GamePhase
{
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
    
    private int gamePhaseValue = 0;

    public GamePhase(Board board)
    {
        int gamePhaseValue = 0;

        gamePhaseValue += gamePhaseInc[1] * BitboardUtility.CountSetBits(board.GetBitboardByPieceType(PieceType.WN) | board.GetBitboardByPieceType(PieceType.BN));
        gamePhaseValue += gamePhaseInc[2] * BitboardUtility.CountSetBits(board.GetBitboardByPieceType(PieceType.WB) | board.GetBitboardByPieceType(PieceType.BB));
        gamePhaseValue += gamePhaseInc[3] * BitboardUtility.CountSetBits(board.GetBitboardByPieceType(PieceType.WR) | board.GetBitboardByPieceType(PieceType.BR));
        gamePhaseValue += gamePhaseInc[4] * BitboardUtility.CountSetBits(board.GetBitboardByPieceType(PieceType.WQ) | board.GetBitboardByPieceType(PieceType.BQ));

        if (gamePhaseValue > 24) gamePhaseValue = 24;

        this.gamePhaseValue = gamePhaseValue;
    }

    public void Undo(Move move)
    {
        if (!(move.IsCapture() || move.IsPromotion())) return;

        switch (move.CapturedPiece)
        {
            case PieceType.WN:
            case PieceType.BN:
                gamePhaseValue += gamePhaseInc[1];
                break;
            case PieceType.WB:
            case PieceType.BB:
                gamePhaseValue += gamePhaseInc[2];
                break;
            case PieceType.WR:
            case PieceType.BR:
                gamePhaseValue += gamePhaseInc[3];
                break;
            case PieceType.WQ:
            case PieceType.BQ:
                gamePhaseValue += gamePhaseInc[4];
                break;
        }

        switch (move.GetPromotionType())
        {
            case null:
                break;
            case PieceType.WN:
                gamePhaseValue -= gamePhaseInc[1];
                break;
            case PieceType.WB:
                gamePhaseValue -= gamePhaseInc[2];
                break;
            case PieceType.WR:
                gamePhaseValue -= gamePhaseInc[3];
                break;
            case PieceType.WQ:
                gamePhaseValue -= gamePhaseInc[4];
                break;
        }

        if (gamePhaseValue > 24) gamePhaseValue = 24;
    }

    public void Update(Move move)
    {
        if (!(move.IsCapture() || move.IsPromotion())) return;

        switch (move.CapturedPiece)
        {
            case PieceType.WN:
            case PieceType.BN:
                gamePhaseValue -= gamePhaseInc[1];
                break;
            case PieceType.WB:
            case PieceType.BB:
                gamePhaseValue -= gamePhaseInc[2];
                break;
            case PieceType.WR:
            case PieceType.BR:
                gamePhaseValue -= gamePhaseInc[3];
                break;
            case PieceType.WQ:
            case PieceType.BQ:
                gamePhaseValue -= gamePhaseInc[4];
                break;
        }

        switch (move.GetPromotionType())
        {
            case null:
                break;
            case PieceType.WN:
                gamePhaseValue += gamePhaseInc[1];
                break;
            case PieceType.WB:
                gamePhaseValue += gamePhaseInc[2];
                break;
            case PieceType.WR:
                gamePhaseValue += gamePhaseInc[3];
                break;
            case PieceType.WQ:
                gamePhaseValue += gamePhaseInc[4];
                break;
        }

        if (gamePhaseValue > 24) gamePhaseValue = 24;
    }

    public int Value => gamePhaseValue;

}