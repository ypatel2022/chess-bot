using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

public class DragPiece : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    public Canvas canvas;
    public RectTransform rectTransform;

    public bool dragging = false;
    public Vector2 startPos;

    public BoardSquare thisSquare;

    public List<Move> legalMoves;

    public Color32 lightTileColour;
    public Color32 darkTileColour;

    public void OnDrag(PointerEventData eventData)
    {
        if (dragging)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
            //Debug.Log("dragging a piece");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Chess.newBoard[thisSquare.x, thisSquare.y] != PieceStuff.None && Chess.IsColour(Chess.newBoard[thisSquare.x, thisSquare.y], Chess.ColourToByte(Chess.colourToMove)))
        {
            // Debug.Log(Chess.kingsPosition[Colour.White].x + ", " + Chess.kingsPosition[Colour.White].y);
            //Debug.Log("Starting dragging");
            dragging = true;
            startPos = GetComponent<RectTransform>().anchoredPosition;

            // object being dragged should be rendered on top of everything else
            canvas.overrideSorting = true;
            canvas.sortingOrder = 1000;

            legalMoves = new List<Move>();
            //Debug.Log("Before dragging, the castling options are " + Chess.kingsCastling[Chess.colourToMove]);
            legalMoves = Chess.NewGenerateMovesForPiece(Chess.newBoard[thisSquare.x, thisSquare.y], Chess.newBoard, new Position(thisSquare.x, thisSquare.y));
            //Debug.Log("the current square is " + thisSquare.x + ", " + thisSquare.y);
            foreach (Move move in legalMoves)
            {
                Chess.boardUI[move.targetPos.x, move.targetPos.y].gameObject.GetComponent<Image>().color = Color.red;
            }
        }
        else
        {
            dragging = false;
            canvas.overrideSorting = false;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        foreach (Move move in legalMoves)
        {
            Chess.boardUI[move.targetPos.x, move.targetPos.y].gameObject.GetComponent<Image>().color = ((move.targetPos.x + move.targetPos.y) % 2 == 1) ? lightTileColour : darkTileColour;
        }

        // this is called last
        dragging = false;
        //rectTransform.anchoredPosition = startPos;
        rectTransform.anchoredPosition = Vector2.zero;
        canvas.overrideSorting = false;

        //Destroy(gameObject.GetComponent<Canvas>());
    }

    public void OnDrop(PointerEventData eventData)
    {
        // dont call the same getcomponent function back to back cause its really slow
        DragPiece draggedPiece = eventData.pointerDrag.GetComponent<DragPiece>();

        // dropped on this square
        if (draggedPiece.dragging)
        {
            draggedPiece.dragging = false;
            // other square is where the piece came from, the start square, this square is where it is being dropped
            BoardSquare otherSquare = eventData.pointerDrag.GetComponent<BoardSquare>();
            Move move = new Move(new Position(otherSquare.x, otherSquare.y), new Position(thisSquare.x, thisSquare.y));

            if (Chess.IsPieceType(Chess.newBoard[move.startPos.x, move.startPos.y], PieceStuff.Pawn) &&
                ((move.targetPos.y == 7 && Chess.IsColour(Chess.newBoard[move.startPos.x, move.startPos.y], PieceStuff.White)) ||
                 (move.targetPos.y == 0 && Chess.IsColour(Chess.newBoard[move.startPos.x, move.startPos.y], PieceStuff.Black))))
            {
                // pawn is promoting
                Debug.Log("The piece you are draggin shall promote");
                move.newPromotion = PieceStuff.Queen;
            }

            if (Chess.IsPieceType(Chess.newBoard[move.startPos.x, move.startPos.y], PieceStuff.King))
            {
                if (move.targetPos.x - move.startPos.x == 2)
                {
                    // castling kingside
                    move.castling = CastlingOptions.Kingside;
                    //Debug.Log("The current castling options are " + Chess.kingsCastling[Chess.board[move.startPos.x, move.startPos.y].colour]);
                }
                else if (move.targetPos.x - move.startPos.x == -2)
                {
                    move.castling = CastlingOptions.Queenside;
                    //Debug.Log("The current castling options are " + Chess.kingsCastling[Chess.board[move.startPos.x, move.startPos.y].colour]);
                }
            }

            if (Chess.NewIsLegalMove(Chess.newBoard, Chess.newBoard[otherSquare.x, otherSquare.y], move, Chess.ColourToByte(Chess.colourToMove)))
            {
                if (Chess.NewIsKingSafeAfterMove(Chess.newBoard, Chess.ColourToByte(Chess.colourToMove), move))
                {
                    //Chess.MakeMovePermanent(ref Chess.board, move);
                    Chess.colourToMove = Chess.SwitchColour(Chess.colourToMove);

                    // do the move
                    Chess.NewMakeMoveTemporary(ref Chess.newBoard, move, false);

                    Chess.UpdateChessBoardUI();
                }
            }
        }
    }
}
