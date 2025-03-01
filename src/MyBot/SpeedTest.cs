namespace ChessBot;

public class SpeedTest
{

    const String MidgameFen = "r2qr1k1/1bp1bppp/p1np1n2/1p6/3pP3/1BP2N1P/PP1N1PP1/R1BQR1K1 w - - 0 12";

    /// <summary>
    /// Performs a speed test on the chess bot.
    /// This test consists of letting the bot evaluate a position for a certain amount of time.
    /// </summary>
    /// <param name="bot"></param>
    public static void Test(IChessBot bot, int time, String position = MidgameFen)
    {
        Board board = new(position);
        System.Diagnostics.Stopwatch stopwatch = new();
        CancellationToken token = new(false);
        stopwatch.Start();
        bot.GetBestMove(board, time, token, true);
        stopwatch.Stop();
        Console.WriteLine("Done.");
        Console.WriteLine("Time taken: " + stopwatch.ElapsedMilliseconds + "ms");
    }

}