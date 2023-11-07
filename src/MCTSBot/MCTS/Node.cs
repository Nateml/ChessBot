namespace ChessBot;

public class Node
{
    public Node(Board board, Node? parent, Move? move, bool whitePlayer)
    {
        Children = new List<Node>();
        Board = board;
        Parent = parent;
        Move = move;
        WhitePlayer = whitePlayer;
    }

    public List<Node> Children { get; }

    public Move? Move { get; set; }

    public Node? Parent { get; set; }

    public Board Board { get; set; }

    public int Simulations { get; private set; }

    public double Wins { get; private set; }

    public bool IsExpanded { get; set; }

    public bool IsTerminal { get; set; }

    public bool WhitePlayer { get; set; }

    public double UCTValue(double explorationParameter) {
        // UCT for MCTS 
        if (Simulations == 0) return double.MaxValue;
        if (Parent == null) return double.MaxValue;
        double exploitation = Wins / (double)Simulations;
        double exploration = Math.Sqrt(Math.Log(Parent.Simulations == 0 ? 1 : Parent.Simulations) / Simulations);
        double uct = exploitation + explorationParameter * exploration;
        if (uct.Equals(double.NaN)) 
        {
            Console.WriteLine("UCT is NaN");
            Console.WriteLine("Wins: " + Wins);
            Console.WriteLine("Simulations: " + Simulations);
            Console.WriteLine("Parent Simulations: " + Parent.Simulations);
            Console.WriteLine("Exploration Parameter: " + explorationParameter);
            Console.WriteLine("Log Parent Simulations: " + Math.Log(Parent.Simulations));
            Console.WriteLine("UCT: " + uct);
            Console.ReadKey();
        }
        return uct;
    }

    public void AddWin()
    {
        Wins += 1;
        Simulations++;
    }

    public void AddLoss()
    {
        Simulations++;
    }

    public void AddDraw()
    {
        Wins += 0.5;
        Simulations++;
    }

    public void AddChild(Node child)
    {
        Children.Add(child);
    }

    public override string ToString()
    {
        return "Node: " + Move + ", Wins: " + Wins + ", Simulations: " + Simulations;
    }
}