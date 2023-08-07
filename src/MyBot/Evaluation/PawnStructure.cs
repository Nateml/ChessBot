namespace ChessBot;

public static class PawnStructure
{
    struct PawnStructureData
    {
        ulong zobristHash;
        int eval; 
        bool valid = false;

        public PawnStructureData()
        {
            valid = false;
        }

        public PawnStructureData(ulong zobristHash, int eval)
        {
            this.zobristHash = zobristHash;
            this.eval = eval;
            valid = true;
        }

        public bool Valid => valid;

        public int Eval => eval;

        public ulong ZobristHash => zobristHash;
        
    }

    public const int HashTableSize = 500000; 
    private static readonly PawnStructureData[] PawnHashTable = new PawnStructureData[HashTableSize];

    private const int passedPawnBonus = 20;
    private const int doubledPawnPenalty = 20;
    private const int connectedPawnWeight = 3;

    public static int EvaluatePawnStructure(Board board)
    {
        // Check if its in the table
        PawnStructureData tableEntry = PawnHashTable[board.PawnZobristHash % HashTableSize];
        if (tableEntry.Valid && tableEntry.ZobristHash == board.PawnZobristHash) return tableEntry.Eval;

        int eval = 0;

        int numPassedPawns = 0;

        ulong whitePawnBitboard = board.GetBitboardByPieceType(PieceType.WP);
        ulong blackPawnBitboard = board.GetBitboardByPieceType(PieceType.BP);
        for (int i = 0; i < 8; i++)
        {
            int numWhitePawns = BitboardUtility.CountSetBits(whitePawnBitboard & MoveGenData.FileMasks[i]);
            int numBlackPawns = BitboardUtility.CountSetBits(blackPawnBitboard & MoveGenData.FileMasks[i]);
            
            if (numWhitePawns == 0 && numBlackPawns >= 1) numPassedPawns--; 
            else if (numBlackPawns == 0 && numWhitePawns >= 1) numPassedPawns++;

            if (numWhitePawns > 1) eval -= doubledPawnPenalty;
            if (numBlackPawns > 1) eval += doubledPawnPenalty;
        }

        eval += passedPawnBonus * numPassedPawns;

        BitboardUtility.ForEachBitscanForward(whitePawnBitboard, pawnIndex => {
            int defendingPawns = BitboardUtility.CountSetBits(MoveGenData.whitePawnAttacks[pawnIndex] & whitePawnBitboard);
            eval += defendingPawns * connectedPawnWeight;
        });

        BitboardUtility.ForEachBitscanForward(blackPawnBitboard, pawnIndex => {
            int defendingPawns = BitboardUtility.CountSetBits(MoveGenData.blackPawnAttacks[pawnIndex] & blackPawnBitboard);
            eval -= defendingPawns * connectedPawnWeight;
        });

        // Store in the table
        PawnHashTable[board.PawnZobristHash % HashTableSize] = new PawnStructureData(board.PawnZobristHash, eval);

        return eval;
    }
}