using System.Linq;
using Xunit;
using ChessBot;

namespace ChessBot.Tests;

public class BoardTests
{
    [Fact]
    public void LoadPositionFromFen_ParsesSideAndCastlingRights()
    {
        string fen = "rnbqkbnr/pp1ppppp/2p5/8/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 1 2";
        Board board = new(fen);

        Assert.False(board.IsWhiteToMove);
        Assert.True(board.CanWhiteCastleKingside());
        Assert.True(board.CanWhiteCastleQueenside());
        Assert.True(board.CanBlackCastleKingside());
        Assert.True(board.CanBlackCastleQueenside());
    }

    [Fact]
    public void MakeAndUnmakeMove_RestoresBoardState()
    {
        Board reference = new();
        Board test = new();
        var move = test.GetLegalMoves().First();
        test.MakeMove(move);
        test.UnmakeMove();

        Assert.True(reference.Bitboards.SequenceEqual(test.Bitboards));
        Assert.Equal(reference.IsWhiteToMove, test.IsWhiteToMove);
        Assert.Equal(reference.EpFile, test.EpFile);
    }
}
