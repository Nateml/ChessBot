class MoveGen
{
    // Maintain an instance of Board for appropriate move generation
    Board board;
    
    public MoveGen(Board board)
    {
        this.board = board;
    }

    /// <summary>
    /// Generates all legal moves in the current board position.
    /// </summary>
    public Move[] GenerateLegalMoves()
    {
        Move[] moves = new Move[128];
        return moves;
    }
}