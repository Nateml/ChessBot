namespace ChessBot;

public class MCTSBot : IChessBot
{

    private Node? rootNode = null;

    //private const double explorationParameter = 1.414;
    private const double explorationParameter = 1.2;

    private const int iterations = 10000;

    private Random random = new();

    public Move GetBestMove(Board board, int timeLimit, bool fixedTime = false, bool printToConsole = true)
    {
        Node rootNode = new(board, null, null, board.IsWhiteToMove);
        Node bestChild = MCTS(rootNode, iterations) ?? throw new Exception("No best move found");
        Move? bestMove = bestChild.Move;
        if (bestMove == null) 
        {
            BitboardUtility.PrintBitboard(bestChild.Board.WhitePiecesBitboard);
            Console.WriteLine("Move count: " + bestChild.Board.NumPlyPlayed);
            Console.WriteLine("Number of children of root node: " + rootNode.Children.Count);
            Console.WriteLine("Number of children of best child: " + bestChild.Children.Count);
            throw new Exception("Best move is null");
        }

        foreach (Node child in rootNode.Children)
        {
            Console.WriteLine(child);
        }

        return bestMove;
    }

    public void ResetGame()
    {
        
    }

    private Node MCTS(Node rootNode, int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            Node leaf = Traverse(rootNode);
            double simResult = Simulate(leaf);
            Backpropogate(leaf, simResult);
        }

        Node? bestChild = BestChild(rootNode);
        if (bestChild != null)
        {
            return bestChild;
        }
        else
        {
            Console.WriteLine("No best child found, returning root node");
            return rootNode;
        }
    }

    private Node Traverse(Node node)
    {
        while (node.IsExpanded)
        {
            node = BestUCT(node);
        }

        return Expand(node);
    }

    private Node Expand(Node node)
    {
        Board parentBoard = node.Board;
        Move[] moves = parentBoard.GetLegalMoves();

        if (moves.Length == 0)
        {
            node.IsTerminal = true;
            return node;
        }

        foreach (Move move in moves)
        {
            Board newBoard = parentBoard.Clone();
            newBoard.MakeMove(move);

            Node child = new(newBoard, node, move, !node.WhitePlayer);
            node.AddChild(child);
        }

        node.IsExpanded = true;

        // randomly select a child
        int randomIndex = random.Next(0, node.Children.Count);
        Node randomChild = node.Children[randomIndex];

        return randomChild;
    }

    private double Simulate(Node node)
    {
        int maxN = 500;
        Board board = node.Board.Clone();
        for (int i = 0; i < maxN; i++)
        {
            Move[] moves = board.GetLegalMoves();

            if (moves.Length == 0)
            {
                if (board.IsKingInCheck(true))
                {
                    if (node.WhitePlayer)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else if (board.IsKingInCheck(false))
                {
                    if (node.WhitePlayer)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }

                /*
                Console.WriteLine("Stalemate");
                Console.WriteLine("Simulated node is white to move: " + node.Board.IsWhiteToMove);
                DebugPrintBoard(board);
                ConsoleKeyInfo _ = Console.ReadKey();
                */
                return 0.5; // stalemate
            }

            int randomIndex = random.Next(0, moves.Length);
            Move randomMove = moves[randomIndex];

            board.MakeMove(randomMove);

            /*
            Console.WriteLine((board.IsWhiteToMove ? "Black" : "White") + " made move " + randomMove + " on ply " + board.NumPlyPlayed);
            Console.WriteLine("White pieces");
            BitboardUtility.PrintBitboard(board.WhitePiecesBitboard);
            Console.WriteLine("Black pieces");
            BitboardUtility.PrintBitboard(board.BlackPiecesBitboard);
            Thread.Sleep(15000);
            */

        }

        /*
        Console.WriteLine("Line after 500 moves: " + board.NumPlyPlayed + " plys");
        BitboardUtility.PrintBitboard(board.WhitePiecesBitboard);
        Thread.Sleep(5000);
        */
        return 0.5;
    }

    private void Backpropogate(Node node, double result)
    {

        if (result == 1)        
        {
            if (node.WhitePlayer) node.AddWin();
            else node.AddLoss();
        }
        else if (result == 0.5)
        {
            node.AddDraw();
        }
        else
        {
            if (node.WhitePlayer) node.AddLoss();
            else node.AddWin();
        }

        if (node.Parent == null) return;

        Backpropogate(node.Parent, result);
    }

    private Node BestUCT(Node node)
    {
        if (node.Children.Count == 0)
        {
            return node;
        }

        List<Node> children = node.Children;

        Node bestChild = children[random.Next(0, children.Count)];
        double bestUCT = bestChild.UCTValue(explorationParameter);
        //Console.WriteLine("Current best UCT value: " + bestUCT);

        for (int i = 1; i < children.Count; i++)
        {
            Node child = children[i];
            double childUCT = child.UCTValue(explorationParameter);
            /*
            Console.WriteLine(childUCT);
            if (childUCT.Equals(double.NaN)) 
            {
                Console.WriteLine(child);
                Console.ReadKey();
            }
            Console.ReadKey();
            */

            if (childUCT > bestUCT)
            {
                bestUCT = childUCT;
                bestChild = child;
                //Console.WriteLine("Found new best UCT value: " + bestUCT);
            }
        }

        return bestChild;
    }

    public Node? BestChild(Node node)
    {
        return node.Children.MaxBy(child => child.Simulations);
    }

    private void DebugPrintBoard(Board board)
    {
        Console.WriteLine("White to move: " + (board.IsWhiteToMove ? "true" : "false"));
        Console.WriteLine("Num ply: " + board.NumPlyPlayed);
        Console.WriteLine("White pieces");
        BitboardUtility.PrintBitboard(board.WhitePiecesBitboard);
        Console.WriteLine("Black pieces");
        BitboardUtility.PrintBitboard(board.BlackPiecesBitboard);
    }
}
