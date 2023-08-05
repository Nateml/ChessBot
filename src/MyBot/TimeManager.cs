using System.ComponentModel.DataAnnotations;
using System.Runtime;

namespace ChessBot;

public static class TimeManager
{

    /// <summary>
    /// Calculates an "ideal" initial maximum search time based
    /// on how much time left there is in the game (milliseconds),
    /// and the number of moves that have been played.
    /// </summary>
    /// <param name="timeLeft"></param>
    /// <returns></returns>
    public static int CalculateMaxTime(int timeLeft, int movesPlayed)
    {
        int nMoves = Math.Min(movesPlayed, 10);
        double factor = movesPlayed < 5 ? 0.5 : 2 - nMoves / 10;
        int target = timeLeft / 50;
        return (int)(factor * target);
    }

    public static int AdjustMaxTime(int previousMaxTime, int numChangesInBestMove, int depthSearched)
    {
        return previousMaxTime - (numChangesInBestMove/depthSearched);
    }

}