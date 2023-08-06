namespace ChessBot;

using System.Diagnostics;
using System.Security.Cryptography;
using ChessBot;

public class MyBot : IChessBot
{

    public readonly TranspositionTable tTable = new(0x100000);

    private int nodesReached = 0;

    private readonly byte MaxDistance = 50;

    private readonly int QuiscenceDepth = 100;

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

    private Stopwatch clock = new Stopwatch();

    public MyBot()
    {
        killerMoves = new KillerMoves(MaxDistance+QuiscenceDepth);
    }

    public void ResetGame()
    {
        isOutOfBook = false;
        tTable.Clear();
        killerMoves.Clear();
    }

    public Move GetBestMove(Board board, int timeLeft, bool fixedTime = false, bool printToConsole = true)
    {
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

        this.printToConsole = printToConsole;
        exitSearch = false;
        timeLimit = timeLeft;
        //timeLimit = 2000;

        if (!fixedTime) timeLimit  = TimeManager.CalculateMaxTime(timeLeft, board.fullMoveCount);

        clock.Start();

        tTable.Clear();

        killerMoves.Clear();

        priorityMove = board.GetLegalMoves()[0];
        int alpha = -100000;
        int beta = 100000;
        int retryMultiplier = 0;
        for (byte distance = 1; distance < MaxDistance && !OutOfTime() && !exitSearch;)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();

            (Move newMove, int bestScore) = NegamaxAtRoot(board, distance, alpha, beta);

            stopwatch.Stop();

            if (!board.IsMoveLegal(newMove)) 
            {
                // I'm not sure why we sometimes get an illegal move, but maybe it has 
                // something to do with the transposition table.
                Console.WriteLine("Illegal move");
                tTable.Clear();
                distance = 1;
                continue;
            }

            // Check if eval was outside the aspiration window
            if (bestScore <= alpha)
            {
                retryMultiplier += 1;
                alpha -= 150 * retryMultiplier;
                beta += 50;
                continue;
            }
            if (bestScore >= beta)
            {
                retryMultiplier += 1;
                beta += 150 * retryMultiplier;
                alpha -= 50;
                continue;
            }

            // Only updating the "priority move" here, because I don't want to update it with a move that had a score outside of the aspiration window
            priorityMove = newMove;

            if (printToConsole)
            {
                Console.Write("info depth " + distance + " nodes " + nodesReached + " score cp " + bestScore + " nps " + ((int) (nodesReached / (stopwatch.ElapsedMilliseconds / 1000.0))) + " pv " );
                List<Move> pv = ExtractPV(board, priorityMove, distance);
                foreach (Move pvMove in pv)
                {
                   Console.Write(pvMove.ToString() + " ");
                }
                Console.WriteLine();
            }

            //retryMultiplier = 0;

            // Adjust the aspiration window
            alpha = bestScore - 30;
            beta = bestScore + 30;

            distance++;
        }

        clock.Stop();
        clock.Reset();
        return priorityMove;
    }

    private (Move, int) NegamaxAtRoot(Board board, byte depth, int alpha, int beta)
    {
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
        int bestScore = -1000000;

        for (int i = 0; i < moves.Length && !OutOfTime(); i++)
        {
            nodesReached++;
            PickMoveRoot(moves, i, board);
            Move move = moves[i];

            board.MakeMove(move);
            int score = -Negamax(board, (byte)(depth-1), 1, -beta, -alpha, board.IsWhiteToMove ? 1 : -1);
            board.UnmakeMove();

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

    private int Negamax(Board board, byte depth, int distanceFromRoot, int alpha, int beta, int colour)
    {
        //if (UCI.IsStopRequested()) return 0;

        nodesReached++;

        /*
        if (board.RepetitionHistory.Contains(board.ZobristHash))
        {
            // Detect draw by three-fold repetion.
            // This only checks if the position has been reached once before.
            // Implementation taken from Sebastian Lague's video "Coding Adventure: Making a Better Chess Bot"
            // Note from him: With this approach, the program might repeat a losing move 
            // (where the opponent missed the winning response before) because it thinks its already a draw.
            // On the other hand, if we only return a draw score when the position has already occured twice,
            // the program might often choose to repeat a position a single time before making a different move.
            // Note from me: With this approach, the implementation is also a whole lot easier because I can just
            // use a hash set of zobrist keys for a quick lookup, instead of having to use a different data structure
            // which allows duplicates...
            // One idea is to use an integer array with zobrist keys as a lookup index, wherein we just increment the
            // appropriate element every time we stumble upon it. However, there are bound to be collisions (there are
            // way more zobrist keys than I can fit into a reasonably sized array)...
            return 0;
        }
        else if (board.NumPlySincePawnMoveOrCapture >= 100)
        {
            // Check 50 move rule
            return 0;
        }
        */

        // Store the initial alpha to check node type later on
        int originalAlpha = alpha;

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
            //return Evaluation.EvaluateBoard(board) * colour;
            return Quiscence(board, QuiscenceDepth, distanceFromRoot+1, alpha, beta, colour);
        }

        Move[] moves = board.GetLegalMoves();

        if (moves.Length == 0)
        {
            if (board.IsKingInCheck(board.IsWhiteToMove))
            {
                return -100000 - depth;
            }
            return 0;
        }

        int bestScore = -1000001;
        for (int i = 0; i < moves.Length; i++)
        {
            PickMove(moves, i, board, distanceFromRoot, bestMove);
            Move move = moves[i];

            board.MakeMove(move);

            int val;

            if (i > 8 && depth >= 3 && !move.IsCapture())
            {
                // We make the assumption that because our move ordering is good (hopefully), that moves further down in the list are likely bad,
                //      so we search them at a reduced depth with a smaller aspiration window.
                const int reduceDepth = 1;
                val = -Negamax(board, (byte)(depth-1-reduceDepth), distanceFromRoot+1, -alpha-1, -alpha, -colour);

                // If we get an evaluation better than we expected, we have to research the node with the full depth
                if (val > alpha)
                {
                    val = -Negamax(board, (byte)(depth-1), distanceFromRoot+1, -beta, -alpha, -colour);
                }
            }
            else
            {
                val = -Negamax(board, (byte)(depth-1), distanceFromRoot+1, -beta, -alpha, -colour);
            }
            //val = -Negamax(board, depth-1, distanceFromRoot+1, -beta, -alpha, -colour);

            board.UnmakeMove();

            // Cut node (fail high)
            if (val >= beta) 
            {
                if (!move.IsCapture())
                {
                    // Store as a killer move
                    killerMoves.Insert(move, distanceFromRoot);
                }
                tTable.Put(board, depth, TranspositionData.LowerboundFlag, val, move);
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

    private int Quiscence(Board board, int depth, int distanceFromRoot, int alpha, int beta, int colour)
    {
        //if (UCI.IsStopRequested()) return 0;

        nodesReached++;

        // Transposition table lookup
        TranspositionData? tdata = tTable.Get(board);

        // We should only use the transposition if it was evaluated closer to the root node
        if (tdata != null)
        {
            // We should always use a transposition (no matter its depth) in the quiscence search
            transpositions++;
            if (tdata.Flag == TranspositionData.ExactFlag) return tdata.Eval;
        }

        if ( depth == 0 ) return Evaluation.EvaluateBoard(board) * colour;

        int standPat = Evaluation.EvaluateBoard(board) * colour;
        
        if ( standPat >= beta ) return standPat;

        // Delta pruning
        int BigDelta = 900;

        if ( standPat < alpha - BigDelta ) return standPat;

        alpha = Math.Max(alpha, standPat);

        Move[] moves = board.GetLegalMoves(true);
        for (int i = 0; i < moves.Length; i++)
        {
            PickMove(moves, i, board, distanceFromRoot);
            Move move = moves[i];

            // Skip this move if the value of the captured piece is not enough to raise alpha (within a 100 safety margin) 
            if (standPat < alpha - Evaluation.GetOpeningPieceValue(move.CapturedPiece) - 100)
            {
                continue;
            }

            board.MakeMove(move);
            int val = -Quiscence(board, depth-1, distanceFromRoot+1, -beta, -alpha, -colour);
            board.UnmakeMove();

            if (val >= beta) return val;

            standPat = Math.Max(val, standPat);

            alpha = Math.Max(alpha, standPat);
        }   

        return standPat;
    }

    private void PickMove(Move[] moves, int startingIndex, Board board, int distanceFromRoot, Move? bestMove = null)
    {
        for (int i = startingIndex+1; i < moves.Length; i++)
        {
            // For performance improvements, I'm only checking if this move == best move once during the first call
            if (startingIndex == 0 && bestMove != null && moves[i].Equals(bestMove))
            {
                (moves[i], moves[startingIndex]) = (moves[startingIndex], moves[i]);
            }
            else if (Evaluation.EvaluateMove(moves[i], board, killerMoves, distanceFromRoot) > Evaluation.EvaluateMove(moves[startingIndex], board, killerMoves, distanceFromRoot))
            {
                (moves[i], moves[startingIndex]) = (moves[startingIndex], moves[i]);
            }
        }
    }

    private void PickMoveRoot(Move[] moves, int startingIndex, Board board)
    {
        for (int i = startingIndex+1; i < moves.Length; i++)
        {
            if (moves[i].Equals(priorityMove))
            {
                // We should first search the best move from the previous search
                (moves[i], moves[startingIndex]) = (moves[startingIndex], moves[i]);
                break;
            }
            else if (Evaluation.EvaluateMove(moves[i], board, killerMoves, 0) > Evaluation.EvaluateMove(moves[startingIndex], board, killerMoves, 0))
            {
                (moves[i], moves[startingIndex]) = (moves[startingIndex], moves[i]);
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

}