# Nate's ChessBot

A chess engine I coded in C# as a personal project.
It's still a work in progress, but I've managed to make significant improvements over the Python and Java based engines I tried creating in the past.

## Features

- UCI communication.
- Move generation using bitboards.
    - Magic bitboards for sliding pieces.
- Negamax with AB pruning.
    - Iterative deepening.
    - Aspiration window with gradual widening
    - Fail-soft alpha-beta pruning.
    - Simple late move reduction (LMR); all moves after the first ten are searched to depth reduced by 1 ply.
    - Unlimited [Quiescence Search](https://www.chessprogramming.org/Quiescence_Search) with [delta pruning](https://www.chessprogramming.org/Delta_Pruning) and a stand-pat score.
- A transposition table.
    - I hate these...
- Static evaluation function:
    - A piece-square table to evaluation positional advantages.
    - Seperate opening and endgame game-phase material weights.
    - A game-phase value, based on how much material is on the board, to taper the opening/endgame material and piece-square values.
    - A slight penalty for having undefended minor pieces.
- Move ordering:
    - [MVV-LVA](https://www.chessprogramming.org/MVV-LVA)
    - [Killer heuristic](https://www.chessprogramming.org/Killer_Heuristic)
    - Moves which place pieces onto tiles that are attacked by opponent pawns are ordered lower.
    - Some priority given to moves which place pieces on "good" tiles, according to the opening phase piece-square table.
- A very simple custom opening-book.
    - I "hand-wrote" a a custom file type for opening lines, which the engine loads into memory startup.
    - The engine checks if the given position is in the opening book, and if it is, then plays the move given by the opening book (or picks one at random, if there are multiple continuing lines).
    - I still need to add a lot more openings/lines. Currently it knows some of the French Defense, the Italian Game, the Ruy-Lopez, and some others.
- Time management.
    - Mostly follows this [formula](https://www.chessprogramming.org/Time_Management#Extra_Time).
    - The first five moves are played with half the calculated thinking time, because otherwise it gets a bit annoying when the engine is immediately out of book and takes really long to think of an opening/response (I will probably change this once the opening book contains enough opening lines).

## Playing against it

~~This bot is available to play against over here https://lichess.org/@/NatesBot.~~ (I've taken it down from lichess for the meantime)
The bot on lichess is currently accepting bullet, blitz and rapid games - and can only play one game at a time.

If you want to play against the bot locally, download the [executable](https://github.com/Nateml/ChessBot/releases) here. I suggest downloading a GUI like [Arena](http://www.playwitharena.de) and loading the engine onto there.

If you do play against it, I'd love to hear feedback on how the game went. Email me at nateml.mac@gmail.com.
