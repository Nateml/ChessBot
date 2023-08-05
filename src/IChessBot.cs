namespace ChessBot;

public interface IChessBot
{
    public Move GetBestMove(Board board, int timeLimit, bool fixedTime = false, bool printToConsole = true);

    public void ResetGame();
}