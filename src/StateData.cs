namespace ChessBot;

class StateData
{
    public Move lastMove;
    public bool CWK, CWQ, CBK, CBQ;
    public ulong epFile;
    public int fullMoveCount, halfMoveCount;

    public StateData(Move lastMove, bool CWK, bool CWQ, bool CBK, bool CBQ, ulong epFile, int fullMoveCount, int halfMoveCount)
    {
        this.lastMove = lastMove;;
        this.CWK = CWK;
        this.CWQ = CWQ;
        this.CBK = CBK;
        this.CBQ = CBQ;
        this.epFile = epFile;
        this.halfMoveCount = halfMoveCount;
        this.fullMoveCount = fullMoveCount;
    }
}