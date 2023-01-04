using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardSquare : MonoBehaviour
{
    public int x = 0;
    public int y = 0;
    public GameObject tile = null;
    public Image pieceImage;
    public Piece pieceOnSquare;

    public BoardSquare(BoardSquare board)
    {
        x = board.x;
        y = board.y;
        pieceImage = board.pieceImage;
        pieceOnSquare = board.pieceOnSquare;
    }
}
