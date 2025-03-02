namespace ChessBot;

class StateData
{
    public Move lastMove;
    public bool CWK, CWQ, CBK, CBQ;
    public byte epFile;
    public ulong zobristHash;
    public int numPlySincePawnMoveOrCapture;
    public double fullMoveCount;

    public StateData(Move lastMove, bool CWK, bool CWQ, bool CBK, bool CBQ, byte epFile, double fullMoveCount, ulong zobristHash, int numPlySincePawnMoveOrCapture)
    {
        this.lastMove = lastMove;;
        this.CWK = CWK;
        this.CWQ = CWQ;
        this.CBK = CBK;
        this.CBQ = CBQ;
        this.epFile = epFile;
        this.fullMoveCount = fullMoveCount;
        this.zobristHash = zobristHash;
        this.numPlySincePawnMoveOrCapture = numPlySincePawnMoveOrCapture;
    }
}