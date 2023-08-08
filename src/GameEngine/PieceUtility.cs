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
}
