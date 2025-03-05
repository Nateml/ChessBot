namespace ChessBot;

public class SelfPlay
{

    public const int GameTime = 1000;

    public static void Main()
    {
        int numGames = 1000;

        Positions.LoadEndgames();
        string[] fens = Positions.Fens!;

        // Randomly shuffle the fens
        Random random = new();
        for (int i = 0; i < fens.Length; i++)
        {
            int j = random.Next(i, fens.Length);
            string temp = fens[i];
            fens[i] = fens[j];
            fens[j] = temp;
        }

        for (int i = 0; i < numGames; i++)
        {
            // Create a new game
            IChessBot white = i % 2 == 0 ? new TreestrapBot() : new MyBot();
            IChessBot black = i % 2 == 0 ? new MyBot() : new TreestrapBot();
            Game game = new(white, black, startingFen: fens[i % fens.Length]);

            // Load the NNUE weights
            if (white is TreestrapBot) ((TreestrapBot)game.White).LoadWeights();
            if (black is TreestrapBot) ((TreestrapBot)game.Black).LoadWeights();

            // Play the game
            game.Play(GameTime, GameTime);

            // After each game the bot should save its NNUE weights
            if (i % 2 == 0) ((TreestrapBot)game.White).SaveWeights();
            else ((TreestrapBot)game.Black).SaveWeights();
        }
    }
}