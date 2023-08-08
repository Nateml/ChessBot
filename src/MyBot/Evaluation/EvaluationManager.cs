namespace ChessBot;

public class EvaluationManager
{
    private readonly GamePhase gamePhase;
    private readonly MaterialEvaluation material;
    private readonly PieceSquare pieceSquare;

    Stack<Move> moveHistory = new Stack<Move>();

    public EvaluationManager(Board board)
    {
        gamePhase = new(board);
        material = new(board);
        pieceSquare = new(board);
    }

    public void Update(Move move)
    {
        gamePhase.Update(move);
        material.Update(move);
        pieceSquare.Update(move);



        moveHistory.Push(move);
    }

    public void Undo()
    {
        Move move = moveHistory.Pop();
        gamePhase.Undo(move);
        material.Undo(move);
        pieceSquare.Undo(move);
    }

    public double PieceSquareScore => pieceSquare.TaperedValue(gamePhase);

    public double MaterialScore => material.TaperedValue(gamePhase);

    public int GamePhase => gamePhase.Value;
}