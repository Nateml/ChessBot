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
            Game game = new(new TreestrapBot(), new TreestrapBot(), startingFen: fens[i % fens.Length]);

            // Load the NNUE weights for the white bot
            ((TreestrapBot)game.White).LoadWeights();
            ((TreestrapBot)game.Black).LoadWeights();

            // Play the game
            game.Play(GameTime, GameTime);

            // After each game the bot should save its NNUE weights
            if (i % 2 == 0) ((TreestrapBot)game.White).SaveWeights();
            else ((TreestrapBot)game.Black).SaveWeights();
        }
    }
}