using System.Drawing;
using System.Numerics;
using Microsoft.VisualBasic;

namespace ChessBot;

/// <summary>
/// Class for evaluating a position using a NNUE (Efficiently Updatable Neural Network) model.
/// Trained through reinforcement learning.
/// </summary>
public class NNUE
{

    class State(double[] inputW, double[] inputB, double[] accumulatorActivationsW, double[] accumulatorActivationsB, double[] preactivationsW, double[] preactivationsB)
    {
        public readonly double[] InputW = inputW;
        public readonly double[] InputB = inputB;
        public readonly double[] AccumulatorActivationsW = accumulatorActivationsW;
        public readonly double[] AccumulatorActivationsB = accumulatorActivationsB;
        public readonly double[] PreactivationsW = preactivationsW;
        public readonly double[] PreactivationsB = preactivationsB;
    }

    private Stack<State> stateHistory;

    // I will write the neural network model from scratch.
    // The neural network will be trained using reinforcement learning.

    public const int InputLayerSize = 40960 + 640; // 64 * 64 * 5 * 2 = 40960, + 640 for the virtual features.
    public const int AccumulatorSize = 256; // Size of each accumulator, actual hidden layer size is 2 * AccumulatorSize.

    private double[][] accumulatorWeights; // These are the same for both accumulators.
    private double[] accumulatorBiases;
    private double[][] accumulatorWeightsVirtual; // These are "virtual" weights, used for a denser feature representation.

    // Weights and biases for the neural network.
    private double[] outputWeights; // [AccumulatorSize * 2]
    private double outputBias; // [1]

    private double[] accumulatorActivationsW; // [AccumulatorSize]
    private double[] accumulatorActivationsB; // [AccumulatorSize]
    private double[] preactivationsW; // [AccumulatorSize]
    private double[] preactivationsB; // [AccumulatorSize]
    private double[] inputW; // input for accumulator W [InputLayerSize]
    private double[] inputB; // input for accumulator B [InputLayerSize]

    public const string ModelPath = "nnue_weights.txt";

    public NNUE()
    {
        inputW = new double[InputLayerSize];
        inputB = new double[InputLayerSize];

        accumulatorActivationsW = new double[AccumulatorSize];
        accumulatorActivationsB = new double[AccumulatorSize];
        preactivationsW = new double[AccumulatorSize];
        preactivationsB = new double[AccumulatorSize];
        accumulatorBiases = new double[AccumulatorSize];
        outputWeights = new double[AccumulatorSize * 2]; // There are two accumulators which have identical input weights but different output weights.
        outputBias = 0;

        // Allocate jagged arrays
        accumulatorWeights = new double[AccumulatorSize][];
        accumulatorWeightsVirtual = new double[AccumulatorSize][];
        for (int i = 0; i < AccumulatorSize; i++)
        {
            accumulatorWeights[i] = new double[InputLayerSize];
            accumulatorWeightsVirtual[i] = new double[InputLayerSize / 64];
        }

        // Initialize state history stack
        stateHistory = new Stack<State>();

        // Initialize weights with small random values.
        Random random = new Random();

        // Initialize weights1 and biases1.
        for (int i = 0; i < AccumulatorSize; i++)
        {
            accumulatorBiases[i] = 0;
            for (int j = 0; j < InputLayerSize; j++)
            {
                accumulatorWeights[i][j] = (random.NextDouble() - 0.5) * 0.1;
            }
        }

        // Initialize output layer weights.
        for (int i = 0; i < AccumulatorSize; i++)
        {
            outputWeights[i] = (random.NextDouble() - 0.5) * 0.1;
        }
    }

    public void Initialize(double[] inputW, double[] inputB)
    {
        Array.Copy(inputW, this.inputW, InputLayerSize);
        Array.Copy(inputB, this.inputB, InputLayerSize);

        // Compute the pre-activation values for each accumulator.
        for (int i = 0; i < AccumulatorSize; i++)
        {
            double sumW = accumulatorBiases[i];
            double sumB = accumulatorBiases[i];

            for (int j = 0; j < InputLayerSize; j++)
            {
               sumW += accumulatorWeights[i][j] * inputW[j]; 
               sumB += accumulatorWeights[i][j] * inputB[j];
            }
            preactivationsW[i] = sumW;
            preactivationsB[i] = sumB;

            // Apply CReLu
            accumulatorActivationsW[i] = CReLu(sumW);
            accumulatorActivationsB[i] = CReLu(sumB);
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
        double output = outputBias;

        for (int i = 0; i < AccumulatorSize; i++)
        {
            output += outputWeights[i] * accumulatorActivationsW[i];
            output += outputWeights[i + AccumulatorSize] * accumulatorActivationsB[i];
        }

        output = TanH(output);

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
        double output = Forward();

        // Compute mean squared error loss.
        double loss = 0.5 * (output - target) * (output - target);

        // === Backward Pass ===
        // Compute output delta.
        double deltaOutput = (output - target) * DTanH(output);

        // Calculate the gradients for the output layer weights and bias.
        for (int i = 0; i < AccumulatorSize; i++)
        {
            double grad = deltaOutput * accumulatorActivationsW[i];
            outputWeights[i] -= grad * learningRate;

            grad = deltaOutput * accumulatorActivationsB[i];
            outputWeights[i + AccumulatorSize] -= grad * learningRate;
        }
        outputBias -= deltaOutput * learningRate;

        // Backpropagate to the accumulators.
        double[] deltaAccumulatorW = new double[AccumulatorSize];
        double[] deltaAccumulatorB = new double[AccumulatorSize];
        for (int i = 0; i < AccumulatorSize; i++)
        {
            deltaAccumulatorW[i] = deltaOutput * outputWeights[i] * DerivativeCReLu(preactivationsW[i]);
            deltaAccumulatorB[i] = deltaOutput * outputWeights[i + AccumulatorSize] * DerivativeCReLu(preactivationsB[i]);
        }

        // Update accumulator weights and biases.
        for (int i = 0; i < AccumulatorSize; i++)
        {
            for (int j = 0; j < InputLayerSize; j++)
            {
                double gradW = deltaAccumulatorW[i] * inputW[j];
                double gradB = deltaAccumulatorB[i] * inputB[j];
                accumulatorWeights[i][j] -= (gradW + gradB) * learningRate; // I am training the input weights by summing the gradients from both accumulators (not sure if this is correct).
            }
            accumulatorBiases[i] -= (deltaAccumulatorW[i] + deltaAccumulatorB[i]) * learningRate; // See above comment, doing the same here.
        }

        return loss;
    }

    /// <summary>
    /// Updates the feature at the given index with the new value.
    /// The accumulator is the index of the accumulator to update.
    /// The accumulator is either 0 or 1.
    /// </summary>
    /// <param name="featureIndex"></param>
    /// <param name="newValue"></param>
    /// <param name="accumulator"></param>
    /// <exception cref="ArgumentException"></exception>
    public void UpdateFeature(int featureIndex, double newValue, int accumulator)
    {
        if (featureIndex < 0 || featureIndex >= InputLayerSize)
        {
            throw new ArgumentException($"Feature index must be between 0 and {InputLayerSize - 1}. Got {featureIndex}.");
        }

        double[] input = accumulator == 0 ? inputW : inputB;

        double oldValue = input[featureIndex]; 
        if (oldValue == newValue)
        {
            return; // No change
        }

        double delta = newValue - oldValue;
        input[featureIndex] = newValue; // Should change inputW/inputB because it is a reference type.

        // Update each neuron's pre-activation
        for (int i = 0; i < AccumulatorSize; i++)
        {
            preactivationsW[i] += accumulatorWeights[i][featureIndex] * delta;
            accumulatorActivationsW[i] = CReLu(preactivationsW[i]);
        }
    }


    private double DerivativeCReLu(double x)
    {
        // Derivative of the Clamped ReLu activation function.
        return x > 0 && x < 1 ? 1 : 0;
    }

    private double CReLu(double x)
    {
        // Clamps x to the range [0, 1]
        return Math.Max(0, Math.Min(1, x));
    }

    private double ReLu(double x)
    {
        return Math.Max(0, x);
    }

    private double Sigmoid(double x)
    {
        return 1 / (1 + Math.Exp(-x));
    }

    private double DSigmoid(double x)
    {
        return Sigmoid(x) * (1 - Sigmoid(x));
    }

    private double TanH(double x)
    {
        return Math.Tanh(x);
    }

    private double DTanH(double x)
    {
        return 1 - x * x;
    }


    /// <summary>
    /// Returns the input layer for a *single accumulator*.
    /// The input layer is a 64*64*5*2 = 40960 element array.
    /// </summary>
    /// <param name="board"></param>
    /// <param name="whiteToMove"></param>
    /// <returns></returns>
    public static double[] GetInput(Board board, bool whiteToMove)
    {
        // Get the HalfKP? feature vector for the given board.
        double[] input = new double[InputLayerSize];

        int kingSquare = whiteToMove ? board.WhiteKingSquare : board.BlackKingSquare;

        // Loop through each piece type
        for (int pieceType = 0; pieceType < 5; pieceType++)
        {
            // Loop through each colour
            for (int colour = 0; colour < 2; colour++)
            {
                ulong bitboard = board.GetBitboardByPieceType((PieceType)(pieceType + colour * 6));                

                colour = whiteToMove ? colour : 1 - colour;

                int pieceIndex = pieceType * 2 + colour;

                // Loop through each square
                for (int square = 0; square < 64; square++)
                {
                    int targetSquare = whiteToMove ? square : 63 - square; // Flip the square if black to move
                    double occupied = BitboardUtility.IsBitSet(bitboard, targetSquare) ? 1 : 0; 
                    input[square + (pieceIndex + kingSquare * 10) * 64] = occupied;
                }
            }
        }

        return input;
    }

    public int GetFeatureIndex(int kingsquare, PieceType piecetype, int square, bool whitePerspective, bool virtualFeature = false)
    {
        if (kingsquare < 0 || kingsquare >= 64)
        {
            throw new ArgumentException("King square must be between 0 and 63.");
        }

        int pieceColour = PieceUtility.Colour(piecetype); // 0 for white, 1 for black

        int piecetype_int = (int) piecetype;

        if (!whitePerspective)
        {
            square = 63 - square; // Flip the square
            kingsquare = 63 - kingsquare;

            piecetype_int = (int)PieceUtility.GetOppositePiece(piecetype);

            pieceColour = 1 - pieceColour;
        }

        if (pieceColour == 1)
        {
            piecetype_int -= 1;
        }

        if (virtualFeature)
        {
            return 40960 + (piecetype_int * 2 + pieceColour) * 64 + square;
        }

        int idx = square + (piecetype_int * 2 + pieceColour + kingsquare * 10) * 64;
        if (idx < 0 || idx >= InputLayerSize)
        {
            Console.WriteLine("Index out of bounds: " + idx);
            Console.WriteLine("Kingsquare: " + kingsquare);
            Console.WriteLine("PieceType: " + piecetype);
            Console.WriteLine("Square: " + square);
            Console.WriteLine("White Perspective: " + whitePerspective);
        }
        return idx;
    }

    public void Refresh(Board board, bool whitePerspective)
    {
        if (whitePerspective)
        {
            inputW = GetInput(board, whitePerspective);
        }
        else
        {
            inputB = GetInput(board, whitePerspective);
        }
    }

    
    public void ApplyMove(Move move, Board board)
    {
        // Push the current state to the stack
        stateHistory.Push(new(inputW, inputB, accumulatorActivationsW, accumulatorActivationsB, preactivationsW, preactivationsB));

        // Remove the piece from the source square.
        int sourceSquare = move.From;
        PieceType pieceType = move.MovingPiece;

        bool whiteMovingPiece = PieceUtility.Colour(pieceType) == 0;

        // Check if the king is moving, if so refresh the input layer.
        // if (pieceType == PieceType.WK)Refresh(board, true);
        //else if (pieceType == PieceType.BK) Refresh(board, false);
        if (pieceType == PieceType.WK || pieceType == PieceType.BK)
        {
            Refresh(board, true);
            Refresh(board, false);
            return;
        }

        int whiteKingSquare = board.WhiteKingSquare;
        int blackKingSquare = board.BlackKingSquare;

        int idx, idxV;
        if (whiteMovingPiece)
        {
            idx = GetFeatureIndex(whiteKingSquare, pieceType, sourceSquare, true, virtualFeature: false); // Get feature idx for white's perspective
            idxV = GetFeatureIndex(whiteKingSquare, pieceType, sourceSquare, true, virtualFeature: true); // Get feature idx for white's perspective
            UpdateFeature(idx, 0, 0); // Update feature in accumulator 0
            UpdateFeature(idxV, 0, 0); // Update feature in accumulator 0
        }
        else
        {
            idx = GetFeatureIndex(blackKingSquare, pieceType, sourceSquare, false, virtualFeature: false); // Get feature idx for black
            idxV = GetFeatureIndex(blackKingSquare, pieceType, sourceSquare, false, virtualFeature: true); // Get feature idx for black
            UpdateFeature(idx, 0, 1); // Update feature in accumulator 1
            UpdateFeature(idxV, 0, 1); // Update feature in accumulator 1
        }

        int destSquare = move.To;

        // Add the piece to the destination square if it is not a promotion.
        PieceType placedPiece;
        if (move.IsPromotion())
        {   
            placedPiece = (PieceType)move.GetPromotionType()!;
            placedPiece = whiteMovingPiece ? placedPiece : PieceUtility.GetOppositePiece(placedPiece);
        }
        else
        {
            placedPiece = pieceType;
        }

        if (whiteMovingPiece)
        {
            idx = GetFeatureIndex(whiteKingSquare, placedPiece, destSquare, true, virtualFeature: false);
            idxV = GetFeatureIndex(whiteKingSquare, placedPiece, destSquare, true, virtualFeature: true);
            UpdateFeature(idx, 1, 0);
            UpdateFeature(idxV, 1, 0);
        }
        else
        {
            idx = GetFeatureIndex(blackKingSquare, placedPiece, destSquare, false, virtualFeature: false);
            idxV = GetFeatureIndex(blackKingSquare, placedPiece, destSquare, false, virtualFeature: true);
            UpdateFeature(idx, 1, 1);
            UpdateFeature(idxV, 1, 1);
        }

        int rookSource = -1, rookDest = -1;

        // Remove the captured piece if there is one.
        if (move.IsCapture())
        {
            if (whiteMovingPiece)
            {
                idx = GetFeatureIndex(blackKingSquare, move.CapturedPiece!, destSquare, false, virtualFeature: false);
                idxV = GetFeatureIndex(blackKingSquare, move.CapturedPiece!, destSquare, false, virtualFeature: true);
                UpdateFeature(idx, 0, 1);
                UpdateFeature(idxV, 0, 1);
            }
            else
            {
                idx = GetFeatureIndex(whiteKingSquare, move.CapturedPiece!, destSquare, true, virtualFeature: false);
                idxV = GetFeatureIndex(whiteKingSquare, move.CapturedPiece!, destSquare, true, virtualFeature: true);
                UpdateFeature(idx, 0, 0);
                UpdateFeature(idxV, 0, 0);
            }

            if (move.IsEnPassant())
            {
                // Remove the captured pawn.
                int enPassantSquare = move.To + (PieceUtility.Colour(move.MovingPiece) == 0 ? -8 : 8);

                if (whiteMovingPiece)
                {
                    idx = GetFeatureIndex(blackKingSquare, PieceType.BP, enPassantSquare, false, virtualFeature: false);
                    idxV = GetFeatureIndex(blackKingSquare, PieceType.BP, enPassantSquare, false, virtualFeature: true);
                    UpdateFeature(idx, 0, 1);
                    UpdateFeature(idxV, 0, 1);
                }
                else
                {
                    idx = GetFeatureIndex(whiteKingSquare, PieceType.WP, enPassantSquare, true, virtualFeature: false);
                    idxV = GetFeatureIndex(whiteKingSquare, PieceType.WP, enPassantSquare, true, virtualFeature: true);
                    UpdateFeature(idx, 0, 0);
                    UpdateFeature(idxV, 0, 0);
                }

            }
        }
        // Handle castling
        else if (move.IsKingsideCastle())
        {
            // Move the rook
            bool isWhite = PieceUtility.Colour(move.MovingPiece) == 0;
            rookSource = isWhite ? 63 : 7;
            rookDest = isWhite ? 61 : 5;
        }
        else if (move.IsQueensideCastle())
        {
            // Move the rook
            bool isWhite = PieceUtility.Colour(move.MovingPiece) == 0;
            rookSource = isWhite ? 56 : 0;
            rookDest = isWhite ? 59 : 3;
        }

        if (rookSource != -1 && rookDest != -1)
        {
            if (whiteMovingPiece)
            {
                // Removing the rook
                idx = GetFeatureIndex(whiteKingSquare, PieceType.WR, rookSource, true, virtualFeature: false);
                idxV = GetFeatureIndex(whiteKingSquare, PieceType.WR, rookSource, true, virtualFeature: true);
                UpdateFeature(idx, 0, 0);
                UpdateFeature(idxV, 0, 0);
                
                // Adding the rook
                idx = GetFeatureIndex(whiteKingSquare, PieceType.WR, rookDest, true, virtualFeature: false);
                idxV = GetFeatureIndex(whiteKingSquare, PieceType.WR, rookDest, true, virtualFeature: true);
                UpdateFeature(idx, 1, 0);
                UpdateFeature(idxV, 1, 0);
            }
            else
            {
                // Removing the rook
                idx = GetFeatureIndex(blackKingSquare, PieceType.BR, rookSource, false, virtualFeature: false);
                idxV = GetFeatureIndex(blackKingSquare, PieceType.BR, rookSource, false, virtualFeature: true);
                UpdateFeature(idx, 0, 1);
                UpdateFeature(idxV, 0, 1);
                
                // Adding the rook
                idx = GetFeatureIndex(blackKingSquare, PieceType.BR, rookDest, false, virtualFeature: false);
                idxV = GetFeatureIndex(blackKingSquare, PieceType.BR, rookDest, false, virtualFeature: true);
                UpdateFeature(idx, 1, 1);
                UpdateFeature(idxV, 1, 1);
            }
        }
    }

    public void UndoMove()
    {
        // Pop the state from the stack
        State state = stateHistory.Pop();
        inputW = state.InputW;
        inputB = state.InputB;
        accumulatorActivationsW = state.AccumulatorActivationsW;
        accumulatorActivationsB = state.AccumulatorActivationsB;
        preactivationsW = state.PreactivationsW;    
        preactivationsB = state.PreactivationsB;
    }

    public void SaveWeights()
    {
        using StreamWriter writer = new(ModelPath);

        // Write the weights1
        for (int i = 0; i < AccumulatorSize; i++)
        {
            for (int j = 0; j < InputLayerSize; j++)
            {
                writer.WriteLine(accumulatorWeights[i][j]);
            }
        }

        // Write the biases1
        for (int i = 0; i < AccumulatorSize; i++)
        {
            writer.WriteLine(accumulatorBiases[i]);
        }

        // Write the weights2
        for (int i = 0; i < AccumulatorSize; i++)
        {
            writer.WriteLine(outputWeights[i]);
        }

        // Write the bias2
        writer.WriteLine(outputBias);
    }

    public void LoadWeights()
    {
        try
        {
            using StreamReader reader = new(ModelPath);

            // Read the weights1
            for (int i = 0; i < AccumulatorSize; i++)
            {
                for (int j = 0; j < InputLayerSize; j++)
                {
                    accumulatorWeights[i][j] = double.Parse(reader.ReadLine());
                }
            }

            // Read the biases1
            for (int i = 0; i < AccumulatorSize; i++)
            {
                accumulatorBiases[i] = double.Parse(reader.ReadLine());
            }

            // Read the weights2
            for (int i = 0; i < AccumulatorSize; i++)
            {
                outputWeights[i] = double.Parse(reader.ReadLine());
            }

            // Read the bias2
            outputBias = double.Parse(reader.ReadLine());
            }
        catch
        {
            Console.WriteLine("Could not load weights. Using random weights.");
            return;
        }

    }

}