using Microsoft.VisualBasic;

namespace ChessBot;

/// <summary>
/// Class for evaluating a position using a NNUE (Efficiently Updatable Neural Network) model.
/// Trained through reinforcement learning.
/// </summary>
public class NNUE
{

    // I will write the neural network model from scratch.

    // The neural network will have 4 layers:
    // 1. Input later: 768 binary features (64 squares * 12 piece types).
    // 2. Hidden layer 1: 1024 neurons.
    // 4. Output layer: 1 neuron, representing the evaluation of the position.

    // The neural network will be trained using reinforcement learning.
    public const int InputLayerSize = 768;
    public const int HiddenLayer1Size = 256;

    // Weights and biases for the neural network.
    private double[,] weights1; // [InputLayerSize X HiddenLayer1Size]
    private double[] biases1; // [HiddenLayer1Size]
    private double[] weights2; // [HiddenLayer1Size]
    private double biases2; // [1]

    private double[] hiddenLayer1Activations; // [HiddenLayer1Size]
    private double[] preActivation1; // [HiddenLayer1Size]
    private double[] input; // [InputLayerSize]

    public const string ModelPath = "nnue_weights.txt";

    public NNUE()
    {
        input = new double[InputLayerSize];
        hiddenLayer1Activations = new double[HiddenLayer1Size];
        preActivation1 = new double[HiddenLayer1Size];
        weights1 = new double[InputLayerSize, HiddenLayer1Size];
        biases1 = new double[HiddenLayer1Size];
        weights2 = new double[HiddenLayer1Size];
        biases2 = 0;

        // Initialize weights with small random values.
        Random random = new Random();
        // Initialize weights1 and biases1.
        for (int i = 0; i < HiddenLayer1Size; i++)
        {
            biases1[i] = 0;
            for (int j = 0; j < InputLayerSize; j++)
            {
                weights1[j, i] = (random.NextDouble() - 0.5) * 0.1;
            }
        }
        // Initialize output layer weights.
        for (int i = 0; i < HiddenLayer1Size; i++)
        {
            weights2[i] = (random.NextDouble() - 0.5) * 0.1;
        }
    }

    public void Initialize(double[] input)
    {
        Array.Copy(input, this.input, InputLayerSize);

        // Compute the pre-activation values for the hidden layer 1.
        for (int i = 0; i < HiddenLayer1Size; i++)
        {
            double sum = biases1[i];
            for (int j = 0; j < InputLayerSize; j++)
            {
               sum += weights1[j, i] * input[j]; 
            }
            preActivation1[i] = sum;
            // Apply ReLu
            hiddenLayer1Activations[i] = ReLu(sum);
        }
    }

    /// <summary>
    /// Perform a forward pass through the neural network using the current state.
    /// The input layer must be initialized before calling this method.
    /// Changes to the input layer should be made by calling UpdateFeature.
    /// </summary>
    /// <returns></returns>
    public double Forward()
    {
        // Compute output layer activation
        double output = biases2;
        for (int i = 0; i < HiddenLayer1Size; i++)
        {
            output += weights2[i] * hiddenLayer1Activations[i];
        }
        
        // I am going to keep the output linear,
        // so no activation function needed.
        // output = Math.Tanh(output);

        return output;
    }

    /// <summary>
    /// Performs a forward and backward pass through the neural network using the current state.
    /// Calculates the gradients which move the weights and biases in the direction of minimizing the loss,
    /// </summary>
    /// <returns></returns>
    public double Train(double target, double learningRate = 0.000001)
    {
        // === Forward Pass ===
        // Compute output layer (hidden layer has already been computed in the incremental update).
        double output = biases2;
        for (int i = 0; i < HiddenLayer1Size; i++)
        {
            output += weights2[i] * hiddenLayer1Activations[i];
        }

        // Compute mean squared error loss.
        double loss = 0.5 * (output - target) * (output - target);

        // === Backward Pass ===
        // Compute output delta.
        // Derivative of the loss with respect to the output is (output - target).
        double deltaOutput = output - target;

        // Calculate the gradients for the output layer weights and bias.
        for (int i = 0; i < HiddenLayer1Size; i++)
        {
            double grad = deltaOutput * hiddenLayer1Activations[i];
            weights2[i] -= grad * learningRate;
        }
        biases2 -= deltaOutput * learningRate;

        // Backpropagate to the hidden layer.
        double[] deltaHidden = new double[HiddenLayer1Size];
        for (int i = 0; i < HiddenLayer1Size; i++)
        {
            // ReLU derivative: 1 if preActivation > 0, else 0.
            double dReLU = preActivation1[i] > 0 ? 1.0 : 0.0;
            deltaHidden[i] = deltaOutput * weights2[i] * dReLU;
        }

        // Update hidden layer weights and biases.
        for (int i = 0; i < HiddenLayer1Size; i++)
        {
            for (int j = 0; j < InputLayerSize; j++)
            {
                double grad = deltaHidden[i] * input[j];
                weights1[j, i] -= grad * learningRate;
            }
            biases1[i] -= deltaHidden[i] * learningRate;
        }

        return loss;
    }

    /// <summary>
    /// Perform a forward pass through the neural network using the given input.
    /// Less efficient than incrementally updating the input using UpdateFeature
    ///  and then calling Forward().
    /// </summary>
    /// <param name="input">All values should be 1 or 0.</param>
    /// <returns>The evaluation of the position represented by the input</returns>
    public double Forward(double[] input)
    {
        if (input.Length != InputLayerSize)
        {
            throw new ArgumentException($"Input size must be {InputLayerSize}.");
        }

        // Compute hidden layer 1 activations
        for (int i = 0; i < HiddenLayer1Size; i++)
        {
            double sum = biases1[i];
            for (int j = 0; j < InputLayerSize; j++)
            {
                sum += weights1[j, i] * input[j];
            }
            hiddenLayer1Activations[i] = ReLu(sum);
        }

        // Compute output layer activation
        double output = biases2;
        for (int i = 0; i < HiddenLayer1Size; i++)
        {
            output += weights2[i] * hiddenLayer1Activations[i];
        }
        output = Math.Tanh(output);

        return output;
    }

    public void UpdateFeature(int featureIndex, double newValue)
    {
        if (featureIndex < 0 || featureIndex >= InputLayerSize)
        {
            throw new ArgumentException($"Feature index must be between 0 and {InputLayerSize - 1}. Got {featureIndex}.");
        }

        double oldValue = input[featureIndex];
        if (oldValue == newValue)
        {
            return; // No change
        }

        double delta = newValue - oldValue;
        input[featureIndex] = newValue;

        // Update each neuron's pre-activation
        for (int i = 0; i < HiddenLayer1Size; i++)
        {
            preActivation1[i] += weights1[featureIndex, i] * delta;
            hiddenLayer1Activations[i] = ReLu(preActivation1[i]);
        }
    }

    private double ReLu(double x)
    {
        return Math.Max(0, x);
    }

    public static double[] GetInput(Board board)
    {
        double[] input = new double[InputLayerSize];
        
        // loop through each piece type
        for (int pieceType = 0; pieceType < 12; pieceType++)
        {
            ulong bitboard = board.GetBitboardByPieceType((PieceType)pieceType);

            // loop through each square
            for (int square = 0; square < 64; square++)
            {
                double occupied = (bitboard & (1UL << square)) != 0 ? 1 : 0;
                input[pieceType * 64 + square] = occupied;
            }
        }
       return input;
    }

    public void ApplyMove(Move move)
    {
        // Update the input layer with the new board state.
        // Remove the piece from the source square.
        int sourceSquare = move.From;
        int pieceType = (int) move.MovingPiece;
        UpdateFeature(pieceType * 64 + sourceSquare, 0);

        int destSquare = move.To;

        // Add the piece to the destination square if it is not a promotion.
        if (move.IsPromotion())
        {   
            int promotedPiece = (int) move.GetPromotionType()!;
            UpdateFeature(promotedPiece * 64 + destSquare, 1);
        }
        else
        {
            UpdateFeature(pieceType * 64 + destSquare, 1);
        }

        // Remove the captured piece if there is one.
        if (move.IsCapture())
        {
            int capturedPiece = (int) move.CapturedPiece!;
            UpdateFeature(capturedPiece * 64 + destSquare, 0);

            if (move.IsEnPassant())
            {
                // Remove the captured pawn.
                int enPassantSquare = move.To + (PieceUtility.Colour(move.MovingPiece) == 0 ? -8 : 8);
                UpdateFeature((int) PieceType.WP * 64 + enPassantSquare, 0);
                UpdateFeature((int) PieceType.BP * 64 + enPassantSquare, 0);
            }
        }
        // Handle castling
        else if (move.IsKingsideCastle())
        {
            // Move the rook
            bool isWhite = PieceUtility.Colour(move.MovingPiece) == 0;
            int rookPiece = (int) (isWhite ? PieceType.WR : PieceType.BR);
            int rookSource = isWhite ? 63 : 7;
            int rookDest = isWhite ? 61 : 5;
            UpdateFeature(rookPiece * 64 + rookSource, 0);
            UpdateFeature(rookPiece * 64 + rookDest, 1);
        }
        else if (move.IsQueensideCastle())
        {
            // Move the rook
            bool isWhite = PieceUtility.Colour(move.MovingPiece) == 0;
            int rookPiece = (int) (isWhite ? PieceType.WR : PieceType.BR);
            int rookSource = isWhite ? 56 : 0;
            int rookDest = isWhite ? 59 : 3;
            UpdateFeature(rookPiece * 64 + rookSource, 0);
            UpdateFeature(rookPiece * 64 + rookDest, 1);
        }
    }

    public void UndoMove(Move move)
    {
        int sourceSquare = move.From;
        int destSquare = move.To;
        int pieceType = (int) move.MovingPiece;

        // 1. Undo the destination update:
        // Remove the piece from the destination square.
        if (move.IsPromotion())
        {
            // If it was a promotion, remove the promoted piece.
            int promotedPiece = (int) move.GetPromotionType()!;
            UpdateFeature(promotedPiece * 64 + destSquare, 0);
        }
        else
        {
            // Otherwise, remove the moving piece from the destination.
            UpdateFeature(pieceType * 64 + destSquare, 0);
        }

        // 2. If the move was a capture, restore the captured piece.
        if (move.IsCapture())
        {
            int capturedPiece = (int) move.CapturedPiece!;
            // Restore the captured piece on the destination square.
            UpdateFeature(capturedPiece * 64 + destSquare, 1);

            if (move.IsEnPassant())
            {
                // In en passant, the captured pawn is not on the destination square.
                int enPassantSquare = move.To + (PieceUtility.Colour(move.MovingPiece) == 0 ? -8 : 8);
                // For white moving, the captured pawn is black; for black moving, it’s white.
                if (PieceUtility.Colour(move.MovingPiece) == 0)
                {
                    UpdateFeature((int) PieceType.BP * 64 + enPassantSquare, 1);
                    // Ensure no white pawn is there.
                    UpdateFeature((int) PieceType.WP * 64 + enPassantSquare, 0);
                }
                else
                {
                    UpdateFeature((int) PieceType.WP * 64 + enPassantSquare, 1);
                    UpdateFeature((int) PieceType.BP * 64 + enPassantSquare, 0);
                }
            }
        }
        // 3. If the move was a castling move, undo the rook move.
        else if (move.IsKingsideCastle())
        {
            bool isWhite = PieceUtility.Colour(move.MovingPiece) == 0;
            int rookPiece = (int)(isWhite ? PieceType.WR : PieceType.BR);
            // In your update, for white kingside you moved the rook from square 63 to 61,
            // and for black from 7 to 5.
            int rookSource = isWhite ? 63 : 7;
            int rookDest = isWhite ? 61 : 5;
            // Reverse the rook move.
            UpdateFeature(rookPiece * 64 + rookDest, 0);
            UpdateFeature(rookPiece * 64 + rookSource, 1);
        }
        else if (move.IsQueensideCastle())
        {
            bool isWhite = PieceUtility.Colour(move.MovingPiece) == 0;
            int rookPiece = (int)(isWhite ? PieceType.WR : PieceType.BR);
            // For white queenside, the rook moved from square 56 to 59,
            // and for black from 0 to 3.
            int rookSource = isWhite ? 56 : 0;
            int rookDest = isWhite ? 59 : 3;
            UpdateFeature(rookPiece * 64 + rookDest, 0);
            UpdateFeature(rookPiece * 64 + rookSource, 1);
        }

        // 4. Finally, restore the moving piece to its source square.
        UpdateFeature(pieceType * 64 + sourceSquare, 1);
    }

    public void SaveWeights()
    {
        using StreamWriter writer = new(ModelPath);

        // Write the weights1
        for (int i = 0; i < HiddenLayer1Size; i++)
        {
            for (int j = 0; j < InputLayerSize; j++)
            {
                writer.WriteLine(weights1[j, i]);
            }
        }

        // Write the biases1
        for (int i = 0; i < HiddenLayer1Size; i++)
        {
            writer.WriteLine(biases1[i]);
        }

        // Write the weights2
        for (int i = 0; i < HiddenLayer1Size; i++)
        {
            writer.WriteLine(weights2[i]);
        }

        // Write the bias2
        writer.WriteLine(biases2);
    }

    public void LoadWeights()
    {
        try
        {
            using StreamReader reader = new(ModelPath);

            // Read the weights1
            for (int i = 0; i < HiddenLayer1Size; i++)
            {
                for (int j = 0; j < InputLayerSize; j++)
                {
                    weights1[j, i] = double.Parse(reader.ReadLine());
                }
            }

            // Read the biases1
            for (int i = 0; i < HiddenLayer1Size; i++)
            {
                biases1[i] = double.Parse(reader.ReadLine());
            }

            // Read the weights2
            for (int i = 0; i < HiddenLayer1Size; i++)
            {
                weights2[i] = double.Parse(reader.ReadLine());
            }

            // Read the bias2
            biases2 = double.Parse(reader.ReadLine());
            }
        catch
        {
            Console.WriteLine("Could not load weights. Using random weights.");
            return;
        }

    }

}