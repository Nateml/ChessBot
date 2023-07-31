namespace ChessBot;
public class BadFenStringException : Exception
{
    public BadFenStringException() : base("Invalid fen string.")
    {

    }

    public BadFenStringException(string message) : base(message)
    {

    }

}