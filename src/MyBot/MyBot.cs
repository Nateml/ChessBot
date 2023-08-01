namespace ChessBot;

using System.ComponentModel;
using System.Diagnostics;
using ChessBot;
using Microsoft.VisualBasic;

public class MyBot : IChessBot
{

    public readonly TranspositionTable tTable = new(0x200000);

    private int nodesReached = 0;

    private int MaxDistance = 40;

    private int transpositions = 0;

    /// <summary>
    /// Max amount of time to spend searching in milliseconds.
    /// </summary>
    private int timeLimit = 5000;

    Move bestMove;

    private Stopwatch clock = new Stopwatch();

    public Move GetBestMove(Board board, int timeLimit)
    {
        this.timeLimit  = timeLimit;

        clock.Start();

        tTable.Clear();

        bestMove = board.GetLegalMoves()[0];
        for (int distance = 1; distance < MaxDistance && !OutOfTime(); distance++)
        {
            bestMove = NegamaxAtRoot(board, distance);
        }

        clock.Stop();
        clock.Reset();
        return bestMove;
    }

    private Move NegamaxAtRoot(Board board, int depth)
    {
        nodesReached = 0;
        transpositions = 0;

        Stopwatch stopwatch = new();
        stopwatch.Start();

        Move[] moves = board.GetLegalMoves();

        if (moves.Length == 0) throw new Exception("No legal moves to make.");

        Move bestMove = moves[0];
        int bestScore = -1000000;

        for (int i = 0; i < moves.Length && !OutOfTime(); i++)
        {
            nodesReached++;
            PickMoveRoot(moves, i, board);
            Move move = moves[i];

            board.MakeMove(move);
            int val = -Negamax(board, depth-1, -100000, 100000);
            board.UnmakeMove();

            if (val > bestScore)
            {
                bestScore = val;
                bestMove = move;
            }
        }

        stopwatch.Stop();
        Console.WriteLine("info depth " + depth + " nodes " + nodesReached + " score cp " + bestScore + " nps " + ((int) (nodesReached / (stopwatch.ElapsedMilliseconds / 1000.0))) + " pv " + bestMove.ToString());
        Console.WriteLine("transposition: " + transpositions);
        Console.WriteLine("move predicted score: " + Evaluation.EvaluateMove(bestMove, board));

        return bestMove;
    }

    private int Negamax(Board board, int depth, int alpha, int beta)
    {
        // Store the initial alpha to check node type later on
        int originalAlpha = alpha;

        // Transposition table lookup
        if (tTable.Contains(board))
        {
            TranspositionData tdata = tTable.Get(board);


            // We should only use the transposition if it was evaluated closer to the root node
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
        }

        //if ( depth == 0 ) return Quiscence(board, 4, alpha, beta);
        if ( depth == 0 ) 
        {
            nodesReached++;
            return Evaluation.EvaluateBoard(board);
        }

        Move[] moves = board.GetLegalMoves();

        if (moves.Length == 0)
        {
            if (board.IsKingInCheck(board.IsWhiteToMove))
            {
                return -1000000;
            }
            return 0;
        }

        Move bestMove = moves[0];

        for (int i = 0; i < moves.Length; i++)
        {
            //nodesReached++;
            PickMove(moves, i, board);
            Move move = moves[i];

            board.MakeMove(move);
            int val = -Negamax(board, depth-1, -beta, -alpha);
            board.UnmakeMove();

            if (val >= alpha)
            {
                bestMove = moves[i];
                alpha = val;
            }

            // Fail 
            if (val >= beta) 
            {
                alpha = beta;
                break;
            }

        }

        // Transposition table store
        // Replace scheme: if same depth or deeper
        if ((tTable.Contains(board) && depth >= tTable.Get(board).Depth) || !tTable.Contains(board))
        {
            int flag;
            if (alpha <= originalAlpha)
            {
                flag = TranspositionData.UpperboundFlag;
            }
            else if (alpha >= beta)
            {
                flag = TranspositionData.LowerboundFlag;
            }
            else 
            {
                flag = TranspositionData.ExactFlag;
            }

            tTable.Put(board, depth, flag, alpha, flag == TranspositionData.ExactFlag ? bestMove : null);
        }

        return alpha;
    }

    private int Quiscence(Board board, int depth, int alpha, int beta)
    {
        if ( depth == 0 ) return Evaluation.EvaluateBoard(board);

        int standPat = Evaluation.EvaluateBoard(board);
        
        if ( standPat >= beta ) return beta;

        // Delta pruning
        int BigDelta = 900;

        if ( standPat < alpha - BigDelta ) return alpha;

        alpha = Math.Max(alpha, standPat);

        Move[] moves = board.GetLegalMoves(true);
        for (int i = 0; i < moves.Length; i++)
        {
            nodesReached++;
            PickMove(moves, i, board);
            Move move = moves[i];

            // Skip this move if the value of the captured piece is not enough to raise alpha (within a 100 safety margin) 
            if (standPat < alpha - Evaluation.GetPieceValue(move.CapturedPiece))
            {
                continue;
            }

            board.MakeMove(move);
            int val = -Quiscence(board, depth-1, -beta, -alpha);
            board.UnmakeMove();

            if (val >= beta) return beta;

            alpha = Math.Max(alpha, val);
        }   

        return alpha;
    }

    private void PickMove(Move[] moves, int startingIndex, Board board)
    {
        for (int i = startingIndex+1; i < moves.Length; i++)
        {
            if (Evaluation.EvaluateMove(moves[i], board) > Evaluation.EvaluateMove(moves[startingIndex], board))
            {
                (moves[i], moves[startingIndex]) = (moves[startingIndex], moves[i]);
            }
        }
    }

    private void PickMoveRoot(Move[] moves, int startingIndex, Board board)
    {
        for (int i = startingIndex+1; i < moves.Length; i++)
        {
            if (moves[i].Equals(bestMove))
            {
                // We should first search the best move from the previous search
                (moves[i], moves[startingIndex]) = (moves[startingIndex], moves[i]);
                break;
            }
            else if (Evaluation.EvaluateMove(moves[i], board) > Evaluation.EvaluateMove(moves[startingIndex], board))
            {
                (moves[i], moves[startingIndex]) = (moves[startingIndex], moves[i]);
            }
        }
    }

    private bool OutOfTime()
    {
        return clock.ElapsedMilliseconds > timeLimit;
    }
}