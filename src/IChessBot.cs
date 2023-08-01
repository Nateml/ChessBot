namespace ChessBot;

public interface IChessBot
{
    public Move GetBestMove(Board board, int timeLimit);
}