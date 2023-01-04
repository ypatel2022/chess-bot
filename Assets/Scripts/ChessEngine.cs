using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class ChessEngine : MonoBehaviour
{
    Dictionary<byte, int> pieceValue = new Dictionary<byte, int>()
    {
        {PieceStuff.None, 0},
        {PieceStuff.Pawn, 10},
        {PieceStuff.Knight, 30},
        {PieceStuff.Bishop, 30},
        {PieceStuff.Rook, 50},
        {PieceStuff.Queen, 90},
        {PieceStuff.King, 900}
    };

    [Range(0, 6)] public int depthToSearch = 3;

    public Colour colourToPlayAs;

    public int positionsEvaluated = 0;

    public Dictionary<BoardInfo, float> transpositionTable = new Dictionary<BoardInfo, float>();

    public TMP_Text positionsEvaluatedText;

    bool isSearching = false;
    void Update()
    {
        if (isSearching)
        {
            return;
        }

        //Debug.Log(EvaluateBoard());

        if (Chess.colourToMove == colourToPlayAs)
        {
            List<Move> theMovesThatCanBeMade = Chess.NewGenerateMovesForColour(Chess.ColourToByte(colourToPlayAs), Chess.newBoard);

            if (theMovesThatCanBeMade.Count == 0)
            {
                //Debug.Log("Bot be checkmated");
                Debug.Break();
            }
            else
            {
                positionsEvaluated = 0;

                // find best move
                //Piece[,] copy = Chess.DuplicateBoard(Chess.board);
                byte[,] boardCopy = Chess.CloneBoard(Chess.newBoard);

                CastlingOptions oldCastling = Chess.kingsCastling[colourToPlayAs];

                Move bestMove = GetBestMove(theMovesThatCanBeMade, boardCopy);

                positionsEvaluatedText.text = $"{positionsEvaluated} Positions Evaluated";


                Chess.kingsCastling[colourToPlayAs] = oldCastling;

                Chess.NewMakeMoveTemporary(ref Chess.newBoard, bestMove, false);

                Chess.UpdateChessBoardUI();
                Chess.colourToMove = Chess.SwitchColour(Chess.colourToMove);
            }
        }
    }

    Move GetBestMove(List<Move> possibleMoves, byte[,] board)
    {
        isSearching = true;

        float bestScore = int.MinValue;
        Move bestMove = possibleMoves[0];

        foreach (Move m in possibleMoves)
        {
            // reverse move
            //Move reverseMove = new Move(new Position(m.targetPos.x, m.targetPos.y), new Position(m.startPos.x, m.startPos.y));
            // killed piece
            //Piece oldPiece = new Piece(board[m.targetPos.x, m.targetPos.y]);
            //CastlingOptions oldCastling = Chess.kingsCastling[colourToPlayAs];

            byte[,] boardClone = Chess.CloneBoard(board);

            float score = 0;


            score = AlphaBetaMiniMax(m, depthToSearch, float.MinValue, float.MaxValue, false, boardClone);


            // reverse move
            //Chess.MakeMoveTemporary(ref Chess.board, reverseMove, oldPiece.type != PieceType.None ? oldPiece : new Piece(), oldPiece.type != PieceType.None);
            // Chess.MakeMoveTemporary(ref board, m, true, oldPiece, oldCastling);


            if (score > bestScore)
            {
                bestScore = score;
                bestMove = new Move(m.startPos, m.targetPos);
            }

        }

        isSearching = false;


        return bestMove;
    }

    float AlphaBetaMiniMax(Move move, int depth, float alpha, float beta, bool isMaximisingPlayer, byte[,] board)
    {
        //Chess.MakeMoveTemporary(ref Chess.board, move, new Piece(), false);
        Chess.NewMakeMoveTemporary(ref board, move, false);

        // check depth is 0 first so possible moves arent checked
        if (depth == 0)
        {
            return EvaluateBoard(board, isMaximisingPlayer ? colourToPlayAs : Chess.SwitchColour(colourToPlayAs));
        }

        // check if game is over
        List<Move> theMovesThatCanBeMade = Chess.NewGenerateMovesForColour(isMaximisingPlayer ? Chess.ColourToByte(colourToPlayAs) : Chess.ColourToByte(Chess.SwitchColour(colourToPlayAs)), board);


        if (theMovesThatCanBeMade.Count == 0)
        {
            // if checkmate and no moves can be made as black
            if (isMaximisingPlayer)
            {
                return int.MinValue;
            }

            return EvaluateBoard(board, isMaximisingPlayer ? colourToPlayAs : Chess.SwitchColour(colourToPlayAs));
        }

        if (isMaximisingPlayer)
        {
            float maxScore = float.MinValue;

            foreach (Move m in theMovesThatCanBeMade)
            {
                //int additionalScore = pieceValue[Chess.board[m.targetPos.x, m.targetPos.y].pieceOnSquare.type];

                // reverse move
                //Move reverseMove = new Move(new Position(m.targetPos.x, m.targetPos.y), new Position(m.startPos.x, m.startPos.y), m.promotion, m.castling, m.enPassant);
                // killed piece
                //Piece oldPiece = new Piece(board[m.targetPos.x, m.targetPos.y]);
                //CastlingOptions oldCastling = Chess.kingsCastling[colourToPlayAs];

                byte[,] boardClone = Chess.CloneBoard(board);

                float eval = AlphaBetaMiniMax(m, depth - 1, alpha, beta, false, boardClone);

                maxScore = Mathf.Max(maxScore, eval);
                alpha = Mathf.Max(alpha, eval);

                // reverse move
                //Chess.MakeMoveTemporary(ref Chess.board, reverseMove, oldPiece.type != PieceType.None ? oldPiece : new Piece(), oldPiece.type != PieceType.None);
                //Chess.MakeMoveTemporary(ref Chess.board, reverseMove, oldPiece, true);
                //Chess.kingsCastling[isMaximisingPlayer ? colourToPlayAs : Chess.SwitchColour(colourToPlayAs)] = oldCastling;
                //Chess.MakeMoveTemporary(ref board, m, true, oldPiece, oldCastling);

                // beta cutoff
                if (beta <= alpha)
                {
                    break;
                }

            }
            return maxScore;
        }
        else
        {
            float minScore = float.MaxValue;

            foreach (Move m in theMovesThatCanBeMade)
            {

                // reverse move
                // reverse move
                //Move reverseMove = new Move(new Position(m.targetPos.x, m.targetPos.y), new Position(m.startPos.x, m.startPos.y), m.promotion, m.castling, m.enPassant);
                // killed piece
                //Piece oldPiece = new Piece(board[m.targetPos.x, m.targetPos.y]);
                //CastlingOptions oldCastling = Chess.kingsCastling[Chess.SwitchColour(colourToPlayAs)];

                byte[,] boardClone = Chess.CloneBoard(board);


                float eval = AlphaBetaMiniMax(m, depth - 1, alpha, beta, true, boardClone);
                minScore = Mathf.Min(minScore, eval);
                beta = Mathf.Min(beta, eval);


                // reverse move
                //Chess.MakeMoveTemporary(ref Chess.board, reverseMove, oldPiece.type != PieceType.None ? oldPiece : new Piece(), oldPiece.type != PieceType.None);
                //Chess.MakeMoveTemporary(ref Chess.board, reverseMove, oldPiece, true);
                //Chess.kingsCastling[isMaximisingPlayer ? colourToPlayAs : Chess.SwitchColour(colourToPlayAs)] = oldCastling;
                //Chess.MakeMoveTemporary(ref board, m, true, oldPiece, oldCastling);

                // alpha cutoff
                if (beta <= alpha)
                {
                    break;
                }
            }
            return minScore;
        }
    }

    public float EvaluateBoard(byte[,] board, Colour playingColour)
    {
        positionsEvaluated++;

        BoardInfo boardInfo = new BoardInfo(Chess.CloneBoard(board), Chess.kingsCastling);
        if (transpositionTable.ContainsKey(boardInfo))
        {
            return transpositionTable[boardInfo];
        }

        float score = 0;

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (Chess.GetColour(board[i, j]) == Chess.ColourToByte(colourToPlayAs))
                {
                    score += pieceValue[Chess.GetPieceType(board[i, j])];
                }
                else
                {
                    score -= pieceValue[Chess.GetPieceType(board[i, j])];
                }
            }
        }

        transpositionTable[boardInfo] = score;


        return score;
    }

    public void SetDepthToSearch(float n)
    {
        depthToSearch = (int)n;
    }

}

public struct BoardInfo
{
    public byte[,] board;
    public Dictionary<Colour, CastlingOptions> kingsCastling;

    public BoardInfo(byte[,] board, Dictionary<Colour, CastlingOptions> kingsCastling)
    {
        this.board = board;
        this.kingsCastling = kingsCastling;
    }
}