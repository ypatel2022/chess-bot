using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardGenerator : MonoBehaviour
{
    public GameObject darkTilePrefab;
    public GameObject lightTilePrefab;
    public GameObject tilePrefab;

    public Color32 darkTileColour;
    public Color32 lightTileColour;

    private void Awake()
    {
        foreach (KeyValuePair<char, Piece> letterToPiece in Chess.letterToPiece)
        {
            GameObject piece;
            if (letterToPiece.Value.colour == Colour.White)
            {
                piece = Resources.Load<GameObject>($"White/{letterToPiece.Key}");
            }
            else
            {
                piece = Resources.Load<GameObject>($"Black/{letterToPiece.Key}");
            }

            Chess.pieceToPrefab[letterToPiece.Value] = piece;
        }

        foreach (KeyValuePair<char, byte> pair in Chess.letterToByte)
        {
            GameObject piece;
            if (Chess.IsColour(pair.Value, PieceStuff.White))
            {
                piece = Resources.Load<GameObject>($"White/{pair.Key}");
            }
            else
            {
                piece = Resources.Load<GameObject>($"Black/{pair.Key}");
            }

            Chess.byteToPrefab[pair.Value] = piece;
        }
    }

    void Start()
    {
        // generate board
        for (int j = 0; j < 8; j++)
        {
            for (int i = 0; i < 8; i++)
            {
                GameObject tile;
                Vector3 position = new Vector3(i, j, 0) * 1.024f;

                if ((i + j) % 2 == 0)
                {
                    //tile = Instantiate(darkTilePrefab, transform);
                    tile = Instantiate(tilePrefab, transform);
                    tile.GetComponent<Image>().color = darkTileColour;
                    // 121, 78, 35
                    //tile.GetComponent<Image>().color = new Color(121, 78, 35, 255);
                }
                else
                {
                    //tile = Instantiate(lightTilePrefab, transform);
                    tile = Instantiate(tilePrefab, transform);
                    tile.GetComponent<Image>().color = lightTileColour;
                    // 200, 178, 158
                    //tile.GetComponent<Image>().color = new Color(200, 178, 158, 255);
                }
                BoardSquare square = tile.GetComponent<BoardSquare>();

                square.x = i;
                square.y = j;
                square.pieceOnSquare = new Piece();
                square.pieceImage.enabled = false;
                square.pieceImage.sprite = null;
                square.pieceImage.useSpriteMesh = true;
                square.pieceImage.preserveAspect = true;
                //square.tile.transform.parent = transform;
                square.tile.name = "Tile " + i + ", " + j;
                Chess.boardUI[i, j] = square;
                Chess.board[i, j] = square.pieceOnSquare;
                Chess.newBoard[i, j] = PieceStuff.None;
            }
        }
        UpdateBoard(Chess.defaultBoardFenString);
        //UpdateBoard("r1b1k1nr/p2p1pNp/n2B4/1p1NP2P/6P1/3P1Q2/P1P1K3/q5b1");
    }

    public void UpdateBoard(string fenString)
    {
        Chess.ConvertFenString(fenString);
    }


}
