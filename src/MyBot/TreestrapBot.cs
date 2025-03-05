namespace ChessBot;

using System.Diagnostics;
using System.Security.Cryptography;
using ChessBot;

public class TreestrapBot : IChessBot
{

    public readonly TranspositionTable tTable = new(8034709);

    private int nodesReached = 0;

    private readonly byte MaxDistance = 50;

    private readonly int QuiscenceDepth = 6;

    private int transpositions = 0;

    private KillerMoves killerMoves;

    /// <summary>
    /// Max amount of time to spend searching in milliseconds.
    /// </summary>
    private int timeLimit = 3000;

    Move? priorityMove;

    private bool exitSearch = false;

    private bool isOutOfBook = false;

    private bool printToConsole = true;

    private EvaluationManager evalManager;

    private Move? bestMove;

    private Stopwatch clock = new Stopwatch();
    private NNUE nnue = new NNUE();
    private const bool Training = true;

    public TreestrapBot()
    {
        killerMoves = new KillerMoves(MaxDistance+QuiscenceDepth);
    }

    public void ResetGame()
    {
        isOutOfBook = false;
        tTable.Clear();
        killerMoves.Clear();
    }

    public Move GetBestMove(Board board, int timeLeft, CancellationToken cancellationToken, bool fixedTime = false, bool printToConsole = true)
    {
        double loss = 0;
        // Play a book move if we can
        if (!isOutOfBook)
        {
            List<Move>? possibleOpeningMoves = Openings.GetOpeningBook().GetMoves(board.ZobristHash);
            if (possibleOpeningMoves == null) isOutOfBook = true;
            else
            {
                return possibleOpeningMoves[new Random().Next(possibleOpeningMoves.Count)];
            }
        }

        evalManager = new(board);

        this.printToConsole = printToConsole;
        exitSearch = false;
        timeLimit = timeLeft;
        //timeLimit = 2000;

        if (!fixedTime) timeLimit  = TimeManager.CalculateMaxTime(timeLeft, board.FullMoveCount + (Training ? 5 : 0)); // +5 for training to avoid speeding up the initial moves

        clock.Start();
        tTable.Clear();
        killerMoves.Clear();

        priorityMove = board.GetLegalMoves()[0];

        double alpha = double.MinValue;
        double beta = double.MaxValue;
        int retryMultiplier = 0;

        int finalDepth = 0;

        for (byte distance = 1; distance < MaxDistance && !OutOfTime() && !exitSearch && !cancellationToken.IsCancellationRequested;)
        {
            finalDepth = distance;

            Stopwatch stopwatch = new();
            stopwatch.Start();

            (Move newMove, double bestScore) = NegamaxAtRoot(board, distance, alpha, beta, cancellationToken);

            if (OutOfTime() || cancellationToken.IsCancellationRequested) break;

            stopwatch.Stop();

            // Store the root in the transposition table
            tTable.Put(board, MaxDistance, TranspositionData.ExactFlag, bestScore, priorityMove);

            if (printToConsole)
            {
                Console.Write("info depth " + distance + " nodes " + nodesReached + " score cp " + bestScore + " nps " + ((int) (nodesReached / (stopwatch.ElapsedMilliseconds / 1000.0))) + " pv " );
                List<Move> pv = ExtractPV(board, newMove, distance);
                foreach (Move pvMove in pv)
                {
                   Console.Write(pvMove.ToString() + " ");
                }
                Console.WriteLine();
            }

            // Check if eval was outside the aspiration window
            if (!Training)
            {
                if (bestScore <= alpha)
                {
                    retryMultiplier += 1;
                    alpha -= 150 * retryMultiplier;
                    beta += 20;
                    continue;
                }
                if (bestScore >= beta)
                {
                    retryMultiplier += 1;
                    beta += 150 * retryMultiplier;
                    alpha -= 20;
                    continue;
                }

                retryMultiplier = 0;

                // Adjust the aspiration window
                alpha = bestScore - 30;
                beta = bestScore + 30;
            }

            // Only updating the "priority move" here, because I don't want to update it with a move that had a score outside of the aspiration window
            priorityMove = newMove;

            distance++;
        }

        clock.Stop();
        clock.Reset();


        // Time to train the NNUE
        loss = 0;
        if (Training)
        {
            loss += UpdateFromTranspositionTable(board, Math.Max(finalDepth - 2, 1), new HashSet<ulong>());
        }

        // Console.WriteLine("Loss: " + loss);

        return priorityMove;
    }

    private (Move, double) NegamaxAtRoot(Board board, byte depth, double alpha, double beta, CancellationToken cancellationToken)
    {
        // Initialize NNUE
        nnue.Initialize(NNUE.GetInput(board, true), NNUE.GetInput(board, false));

        nodesReached = 0;
        transpositions = 0;

        Move[] moves = board.GetLegalMoves();

        if (moves.Length == 0) throw new Exception("No legal moves to make.");
        else if (moves.Length == 1) 
        {
            exitSearch = true;
            return (moves[0], (alpha+beta)/2);
        }

        Move bestMove = moves[0];
        double bestScore = -1;

        for (int i = 0; i < moves.Length && !OutOfTime() && !cancellationToken.IsCancellationRequested; i++)
        {
            nodesReached++;
            PickMoveRoot(moves, i, board);
            Move move = moves[i];

            board.MakeMove(move);
            //evalManager.Update(move);
            // Update the NNUE input
            nnue.ApplyMove(move, board);

            double score = -Negamax(board, (byte)(depth-1), 1, -beta, -alpha, board.IsWhiteToMove ? 1 : -1, cancellationToken);
            board.UnmakeMove();
            //evalManager.Undo();
            nnue.UndoMove();

            if (score > bestScore)
            {
                bestMove = move;
                bestScore = score;
            }

            if (score > alpha)
            {
                alpha = bestScore;
            }
        }


        return (bestMove, bestScore);
    }

    private double Negamax(Board board, byte depth, int distanceFromRoot, double alpha, double beta, int colour, CancellationToken cancellationToken)
    {
        if (OutOfTime() || cancellationToken.IsCancellationRequested) return 0;
        //if (UCI.IsStopRequested()) return 0;

        nodesReached++;

        // Check for 3-fold repetition and 50 move rule
        // Draws rarely happen before 4 ply since an irreverisble move,
        // so we can save some time by not checking before that
        if (board.NumPlySincePawnMoveOrCapture >= 4)
        {
            if (board.NumPlySincePawnMoveOrCapture >= 100) // 50 move rule
            {
                return 0; // Draw
            }

            // Check for 3-fold repetition

            int repetitions = CountRepetitions(board.History, board.ZobristHash, board.NumPlySincePawnMoveOrCapture);
            // If we have repeated the position three times, or if we have repeated the position twice and we are two ply away from the root
            // then we return a draw.
            if (repetitions >= 2 || (repetitions >= 1 && distanceFromRoot > 2)) 
            {
                return 0; // Draw
            }
        }

        // Store the initial alpha to check node type later on
        double originalAlpha = alpha;

        Move? bestMove = null;

        // Transposition table lookup
        TranspositionData? tdata = tTable.Get(board);

        // We should only use the transposition if it was evaluated closer to the root node
        if (tdata != null)
        {
            if (tdata.Depth >= depth)
            {
                transpositions++;
                switch (tdata.Flag)
                {
                    case TranspositionData.ExactFlag:
                        return tdata.Eval;
                    case TranspositionData.LowerboundFlag:
                        alpha = Math.Max(alpha, tdata.Eval);
                        break;
                    case TranspositionData.UpperboundFlag:
                        beta = Math.Min(beta, tdata.Eval);
                        break;
                }

                if (alpha > beta) 
                {
                    return tdata.Eval;
                }
            }

            // Get the first move to search to hopefully increase pruning
            if (tdata.Flag == TranspositionData.ExactFlag)
            {
                bestMove = tdata.BestMove;
            }
        }

        if ( depth == 0 ) 
        {
            // return Evaluation.EvaluateBoard(board) * colour;
            // return Evaluation.EvaluateBoard(board, evalManager) * colour;
            return Quiescence(board, QuiscenceDepth, distanceFromRoot+1, alpha, beta, colour);
        }

        // Null Move Pruning (only when not in training mode)
        if (!Training && depth >= 5 && !board.IsKingInCheck(board.IsWhiteToMove) && !board.IsKingInCheck(!board.IsWhiteToMove))
        {
            const int R = 3;
            board.MakeMove(Move.MakeNullMove());
            double val = -Negamax(board, (byte)(depth-1-R), distanceFromRoot+1, -beta, -beta+1, -colour, cancellationToken);
            board.UnmakeMove();
            if (val >= beta) return val;
        }

        Move[] moves = board.GetLegalMoves();

        if (moves.Length == 0)
        {
            if (board.IsKingInCheck(board.IsWhiteToMove))
            {
                return -10000 - depth;
                //return -1 - (depth/100.0);
            }
            return 0;
        }

        //MoveScores cachedMoveScores = new();

        double bestScore = -1;
        for (int i = 0; i < moves.Length; i++)
        {

            PickMove(moves, i, board, distanceFromRoot, bestMove);
            Move move = moves[i];

            board.MakeMove(move);
            nnue.ApplyMove(move, board);

            double val;

            if (i > 5 && depth >= 3 && !move.IsCapture())
            {
                // We make the assumption that because our move ordering is good (hopefully), that moves further down in the list are likely bad,
                //      so we search them at a reduced depth with a smaller aspiration window.
                const int reduceDepth = 1;
                val = -Negamax(board, (byte)(depth-1-reduceDepth), distanceFromRoot+1, -alpha-1, -alpha, -colour, cancellationToken);

                // If we get an evaluation better than we expected, we have to research the node with the full depth
                if (val > alpha)
                {
                    val = -Negamax(board, (byte)(depth-1), distanceFromRoot+1, -beta, -alpha, -colour, cancellationToken);
                }
            }
            else
            {
                val = -Negamax(board, (byte)(depth-1), distanceFromRoot+1, -beta, -alpha, -colour, cancellationToken);
            }

            board.UnmakeMove();
            nnue.UndoMove();

            if (OutOfTime() || cancellationToken.IsCancellationRequested) return 0; // Exit early if we are out of time

            // Cut node (fail high)
            if (val >= beta) 
            {
                if (!move.IsCapture())
                {
                    // Store as a killer move
                    killerMoves.Insert(move, distanceFromRoot);
                }
                tTable.Put(board, depth, TranspositionData.LowerboundFlag, (int)val, move);
                return val;
            }

            if (val >= bestScore)
            {
                bestMove = move;
                bestScore = val;
                if (val >= alpha)
                {
                    alpha = val;
                }
            }

        }

        // Transposition table store
        byte flag;
        if (bestScore <= originalAlpha)
        {
            flag = TranspositionData.UpperboundFlag;
        }
        else 
        {
            flag = TranspositionData.ExactFlag;
        }

        // We store the "best move" only if we found an exact evaluation, or if the move was good enough to cause a cutoff
        tTable.Put(board, depth, flag, bestScore, !(flag == TranspositionData.UpperboundFlag) ? bestMove : null);

        return bestScore;
    }

    private double Quiescence(Board board, int depth, int distanceFromRoot, double alpha, double beta, int colour)
    {
        if (OutOfTime()) return 0;

        nodesReached++;

        // Transposition table lookup
        TranspositionData? tdata = tTable.Get(board);

        if (tdata != null && tdata.Flag == TranspositionData.ExactFlag)
        {
            // We should always use a transposition (no matter its depth) in the quiscence search
            transpositions++;
            return tdata.Eval;
        }

        if (depth == 0 ) return nnue.Forward() * colour;

        double standPat = nnue.Forward() * colour;
        
        if ( standPat >= beta ) return standPat;

        // Delta pruning
        // int BigDelta = 900;

        // if ( standPat < alpha - BigDelta ) return standPat;

        alpha = Math.Max(alpha, standPat);

        Move[] moves = board.GetLegalMoves(true);
        for (int i = 0; i < moves.Length; i++)
        {
            PickMove(moves, i, board, distanceFromRoot);
            Move move = moves[i];

            board.MakeMove(move);
            nnue.ApplyMove(move, board);

            double val = -Quiescence(board, depth-1, distanceFromRoot+1, -beta, -alpha, -colour);

            board.UnmakeMove();
            nnue.UndoMove();

            if (OutOfTime()) return 0; // Exit early if we are out of time

            if (val >= beta) return val;

            standPat = Math.Max(val, standPat);

            alpha = Math.Max(alpha, standPat);
        }   

        return standPat;
    }

    private void PickMove(Move[] moves, int startingIndex, Board board, int distanceFromRoot, Move? bestMove = null)
    {
        //int scoreAtStartingIndex = Evaluation.EvaluateMove(moves[startingIndex], board, killerMoves, distanceFromRoot, evalManager.GamePhase);
        //int scoreAtStartingIndex = moves[startingIndex].GetScore(board, killerMoves, distanceFromRoot, evalManager.GamePhase);
        for (int i = startingIndex+1; i < moves.Length; i++)
        {
            // For performance improvements, I'm only checking if this move == best move once during the first call
            if (startingIndex == 0 && bestMove != null)
            {
                if (moves[i].Equals(bestMove))
                {
                    (moves[i], moves[startingIndex]) = (moves[startingIndex], moves[i]);
                    return; // We have found the "best swap", so we can return early
                }
                continue;
            }
            else 
            {
                return;
                //int scoreAtCurrentMove = Evaluation.EvaluateMove(moves[i], board, killerMoves, distanceFromRoot, evalManager.GamePhase);
                //int scoreAtCurrentMove = moves[i].GetScore(board, killerMoves, distanceFromRoot, evalManager.GamePhase);

                //if (scoreAtCurrentMove > scoreAtStartingIndex)
                //{
                    //(moves[i], moves[startingIndex]) = (moves[startingIndex], moves[i]);
                    //scoreAtStartingIndex = scoreAtCurrentMove;
                //}

                // return 
            }
        }
    }

    private void PickMoveRoot(Move[] moves, int startingIndex, Board board)
    {
        int scoreAtStartingIndex = Evaluation.EvaluateMove(moves[startingIndex], board, killerMoves, 0, evalManager.GamePhase);
        for (int i = startingIndex+1; i < moves.Length; i++)
        {
            if (moves[i].Equals(priorityMove))
            {
                // We should first search the best move from the previous search
                (moves[i], moves[startingIndex]) = (moves[startingIndex], moves[i]);
                break;
            }
            else 
            {
                int scoreAtCurrentMove = Evaluation.EvaluateMove(moves[i], board, killerMoves, 0, evalManager.GamePhase);
                if (scoreAtCurrentMove > scoreAtStartingIndex)
                {
                    (moves[i], moves[startingIndex]) = (moves[startingIndex], moves[i]);
                    scoreAtStartingIndex = scoreAtCurrentMove;
                }
            }
        }
    }

    private bool OutOfTime()
    {
        return clock.ElapsedMilliseconds > timeLimit;
    }

    private List<Move> ExtractPV(Board board, Move firstPVMove, int depth)
    {
        List<Move> pv = new()
        {
            firstPVMove
        };

        board.MakeMove(firstPVMove);
        TranspositionData? nextPositionFromTTable = tTable.Get(board);
        while (nextPositionFromTTable != null && nextPositionFromTTable.Depth != 0 && depth != 0)
        {
            if (nextPositionFromTTable.BestMove == null) break;
            pv.Add(nextPositionFromTTable.BestMove);
            board.MakeMove(nextPositionFromTTable.BestMove);
            nextPositionFromTTable = tTable.Get(board);
            depth--;
        }
        for (int i = 0; i < pv.Count; i++)
        {
            board.UnmakeMove();
        }

        return pv;
    }

    /// <summary>
    /// Used to check for 3-fold repetition.
    /// Taken from https://groups.google.com/g/rec.games.chess.computer/c/ft82tUpHJn0/m/FJNPi4KWjRYJ
    /// </summary>
    public static int CountRepetitions(LinkedList<ulong> hashHistory, ulong currentHash, int numPlySincePawnMoveOrCapture, bool debug = false)
    {
        int count = 0;

        // Iterate backwards through the history
        LinkedListNode<ulong>? node = hashHistory.Last!.Previous;
        if (debug)
        {
            Console.WriteLine("Current hash: " + currentHash);
        }
        while (numPlySincePawnMoveOrCapture >= 0 && node != null)
        {
            if (debug)
            {
                Console.WriteLine("Num ply since pawn move or capture: " + numPlySincePawnMoveOrCapture);
                Console.WriteLine("Node hash: " + node.Value);
                Console.WriteLine("Match: " + (node.Value == currentHash));
            }
            if (node.Value == currentHash)
            {
                count++;
            }
            if (count == 2) return count;

            // I have to iterate two nodes at a time because 
            // I want to always look at it from the perspective of the player
            // who made the last move
            node = node.Previous;
            if (node == null) break;
            node = node.Previous;

            numPlySincePawnMoveOrCapture -= 2;
        }
        return count;
    }

    private double UpdateFromTranspositionTable(Board board, int d, HashSet<ulong> visited, int currentDepth = 0, int maxDepth = 10)
    {
        // Probe the transposition table
        TranspositionData? entry = tTable.Get(board);     

        // If the entry is null or if it was evaluated at a shallower depth than our minimum depth d, we don't use it
        if (entry == null || entry.Depth < d)
        {
            return 0;
        }

        // Avoiding cycles
        if (visited.Contains(board.ZobristHash))
        {
            return 0;
        }

        if (currentDepth >= maxDepth)
        {
            return 0;
        }

        visited.Add(board.ZobristHash);

        // Determine whose turn it is
        bool isWhite = board.IsWhiteToMove;

        double eval = entry.Eval * (isWhite ? 1 : -1);

        double loss = nnue.Train(eval);

        // Crawl through the transposition table
        // Get all successor states of the current state
        Move[] moves = board.GetLegalMoves();
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            nnue.ApplyMove(move, board);
            loss += UpdateFromTranspositionTable(board, d, visited, currentDepth: currentDepth+1, maxDepth: maxDepth);
            board.UnmakeMove();
            nnue.UndoMove();
        }

        // Check if loss is NaN or greater than 10000
        if (double.IsNaN(loss) || loss > 10000)
        {
            loss = 10000;
        }

        visited.Remove(board.ZobristHash);

        return loss;
    }

    public void SaveWeights()
    {
        nnue.SaveWeights();
    }

    public void LoadWeights()
    {
        nnue.LoadWeights();
    }

    public Move? BestMove { get { return bestMove; }}

}