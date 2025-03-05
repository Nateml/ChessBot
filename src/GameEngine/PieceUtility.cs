namespace ChessBot;

public static class PieceUtility
{
    /// <summary>
    ///  Retrieves the colour of a given piece.
    /// Returns 0 for white and 1 for black. 
    /// </summary>
    public static int Colour(PieceType piece)
    {
        if ((int)piece <= 5) return 0;
        else return 1;
    }

    public static PieceType GetOppositePiece(PieceType piece)
    {
        return piece switch
        {
            PieceType.WP => PieceType.BP,
            PieceType.WN => PieceType.BN,
            PieceType.WB => PieceType.BB,
            PieceType.WR => PieceType.BR,
            PieceType.WQ => PieceType.BQ,
            PieceType.WK => PieceType.BK,
            PieceType.BP => PieceType.WP,
            PieceType.BN => PieceType.WN,
            PieceType.BB => PieceType.WB,
            PieceType.BR => PieceType.WR,
            PieceType.BQ => PieceType.WQ,
            PieceType.BK => PieceType.WK,
            _ => PieceType.EMPTY,
        };
    }
}
