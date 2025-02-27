namespace ChessBot;

using System.Numerics;

class FenParser
{
    public readonly ulong WP = 0ul;
    public readonly ulong WN = 0ul;
    public readonly ulong WB = 0ul;
    public readonly ulong WR = 0ul;
    public readonly ulong WQ = 0ul;
    public readonly ulong WK = 0ul;
    public readonly ulong BP = 0ul;
    public readonly ulong BN = 0ul;
    public readonly ulong BB = 0ul;
    public readonly ulong BR = 0ul;
    public readonly ulong BQ = 0ul;
    public readonly ulong BK = 0ul;

    public readonly bool isWhiteToMove, cwk, cwq, cbk, cbq;
    public readonly byte epFile;
    public readonly int halfMoveCount, fullMoveCount;

    public FenParser(string fen)
    {
        string[] fields = fen.Split(' ');

        string[] ranks = fields[0].Split('/');
        int index = 0;
        for (int rank = 0; rank < ranks.Length; rank++)
        {
            string rankString = ranks[rank];
            for (int tile = 0; tile < rankString.Length; tile++)
            {
                char tileChar = rankString[tile];        
                switch (tileChar)
                {
                    case 'p':
                        BP |= 1ul << index;
                        break;
                    case 'n':
                        BN |= 1ul << index;
                        break;
                    case 'b':
                        BB |= 1ul << index;
                        break;
                    case 'r':
                        BR |= 1ul << index;
                        break;
                    case 'q':
                        BQ |= 1ul << index;
                        break;
                    case 'k':
                        BK |= 1ul << index;
                        break;
                    case 'P':
                        WP |= 1ul << index;
                        break;
                    case 'N':
                        WN |= 1ul << index;
                        break;
                    case 'B':
                        WB |= 1ul << index;
                        break;
                    case 'R':
                        WR |= 1ul << index;
                        break;
                    case 'Q':
                        WQ |= 1ul << index;
                        break;
                    case 'K':
                        WK |= 1ul << index;
                        break;
                    default:
                        if (char.IsNumber(tileChar) && char.GetNumericValue(tileChar) <=8 && char.GetNumericValue(tileChar) >= 1)
                        {
                            index += (int)char.GetNumericValue(tileChar)-1;
                        }
                        else
                        {
                            throw new BadFenStringException("Invalid piece character/number present in first field.");
                        }
                        break;
                }

                index++;
            }
        }

        if (fields[1] == "w")
        {
            isWhiteToMove = true;
        }
        else if (fields[1] == "b")
        {
            isWhiteToMove = false;
        }
        else
        {
            throw new BadFenStringException("Expected either 'w' or 'b' as the second field");
        }

        cwk = false;
        cwq = false;
        cbk = false;
        cbq = false;

        for (int i = 0; i < fields[2].Length; i++)
        {
            switch (fields[2][i])
            {
                case 'K':
                    cwk = true;
                    break;
                case 'Q':
                    cwq = true;
                    break;
                case 'k':
                    cbk = true;
                    break;
                case 'q':
                    cbq = true;
                    break;
                case '-':
                    break;
                default:
                    throw new BadFenStringException("Expected either 'K', 'Q', 'k', 'q' or '-' as characters in the third field.");
            }
        }

        epFile = fields[3][0] switch
        {
            'a' => 0,
            'b' => 1,
            'c' => 2,
            'd' => 3,
            'e' => 4,
            'f' => 5,
            'g' => 6,
            'h' => 7,
            '-' => 8,
            _ => throw new BadFenStringException("Invalid en passant file."),
        };

        fullMoveCount = int.Parse(fields[4]);
        halfMoveCount = int.Parse(fields[5]);
    }

    
}