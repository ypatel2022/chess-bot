using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Chess
{

    public static readonly string defaultBoardFenString = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public static readonly Vector2Int[] knightOffsets = {
        new Vector2Int(1,2),
        new Vector2Int(-1,2),
        new Vector2Int(1,-2),
        new Vector2Int(-1,-2),
        new Vector2Int(2,1),
        new Vector2Int(2,-1),
        new Vector2Int(-2,1),
        new Vector2Int(-2,-1)
    };

    public static Dictionary<char, Piece> letterToPiece = new Dictionary<char, Piece>()
    {
        ['k'] = new Piece(PieceType.King, Colour.Black),
        ['q'] = new Piece(PieceType.Queen, Colour.Black),
        ['r'] = new Piece(PieceType.Rook, Colour.Black),
        ['b'] = new Piece(PieceType.Bishop, Colour.Black),
        ['n'] = new Piece(PieceType.Knight, Colour.Black),
        ['p'] = new Piece(PieceType.Pawn, Colour.Black),
        ['K'] = new Piece(PieceType.King, Colour.White),
        ['Q'] = new Piece(PieceType.Queen, Colour.White),
        ['R'] = new Piece(PieceType.Rook, Colour.White),
        ['B'] = new Piece(PieceType.Bishop, Colour.White),
        ['N'] = new Piece(PieceType.Knight, Colour.White),
        ['P'] = new Piece(PieceType.Pawn, Colour.White)
    };

    public static Dictionary<char, byte> letterToByte = new Dictionary<char, byte>()
    {
        ['k'] = PieceStuff.Black + PieceStuff.King,
        ['q'] = PieceStuff.Black + PieceStuff.Queen,
        ['r'] = PieceStuff.Black + PieceStuff.Rook,
        ['b'] = PieceStuff.Black + PieceStuff.Bishop,
        ['n'] = PieceStuff.Black + PieceStuff.Knight,
        ['p'] = PieceStuff.Black + PieceStuff.Pawn,
        ['K'] = PieceStuff.White + PieceStuff.King,
        ['Q'] = PieceStuff.White + PieceStuff.Queen,
        ['R'] = PieceStuff.White + PieceStuff.Rook,
        ['B'] = PieceStuff.White + PieceStuff.Bishop,
        ['N'] = PieceStuff.White + PieceStuff.Knight,
        ['P'] = PieceStuff.White + PieceStuff.Pawn
    };

    public static Dictionary<Piece, GameObject> pieceToPrefab = new Dictionary<Piece, GameObject>();

    public static Dictionary<byte, SpriteRenderer> byteToPrefab = new Dictionary<byte, SpriteRenderer>();

    public static BoardSquare[,] boardUI = new BoardSquare[8, 8];

    public static Piece[,] board = new Piece[8, 8];

    public static byte[,] newBoard = new byte[8, 8];

    public static Colour colourToMove = Colour.White;

    public static Dictionary<Colour, Position> kingsPosition = new Dictionary<Colour, Position>()
    {
        [Colour.White] = new Position(4, 0),
        [Colour.Black] = new Position(4, 7)
    };

    public static Dictionary<Colour, CastlingOptions> kingsCastling = new Dictionary<Colour, CastlingOptions>()
    {
        [Colour.White] = CastlingOptions.Both,
        [Colour.Black] = CastlingOptions.Both
    };

    public static byte[,] CloneBoard(byte[,] boardToClone)
    {
        return (byte[,])boardToClone.Clone();
    }

    public static List<Piece> piecesOnBoard = new List<Piece>();

    public static string fenString;
    public static string bwString;

    public static bool IsColour(byte piece, byte colour)
    {
        if (piece == PieceStuff.None)
        {
            return false;
        }

        // white
        if (colour == PieceStuff.White)
        {
            return piece < PieceStuff.Black;
        }

        // black
        return piece > PieceStuff.Black;
    }

    public static byte GetColour(byte piece)
    {
        if (piece > PieceStuff.Black)
        {
            return PieceStuff.Black;
        }
        return PieceStuff.White;
    }

    public static byte GetPieceType(byte piece)
    {
        if (piece == PieceStuff.None)
        {
            return PieceStuff.None;
        }

        byte colour = GetColour(piece);
        return (byte)(piece - colour);
    }

    public static bool IsPieceType(byte piece, byte type)
    {
        if (piece == PieceStuff.None)
        {
            return false;
        }

        byte colour = GetColour(piece);

        return piece == (byte)(type + colour);
    }

    public static byte ColourToByte(Colour colour)
    {
        if (colour == Colour.Black)
        {
            return PieceStuff.Black;
        }
        return PieceStuff.White;
    }

    public static void ConvertFenString(string fenString)
    {
        // loop through each tile and set the pieces tile.transform.GetChild(0).transform;
        // https://en.wikipedia.org/wiki/Forsyth%E2%80%93Edwards_Notation
        // lowercase is black and uppercase is white
        //https://www.chess.com/terms/fen-chess
        // fen notation starts from black's side

        string[] rows = fenString.Split('/');
        foreach (string row in rows)
        {
            int number = Array.IndexOf(rows, row);
            //Debug.Log(row);
            int counter = 0;
            for (int i = 0; i < row.Length; i++)
            {
                if (i > 7)
                {
                    // its the extra info about the turn and castling
                }
                else
                {
                    if (char.IsDigit(row[i]))
                    {
                        // it's a number indicating the blanks
                        int oldCounter = counter;
                        counter += int.Parse(row[i].ToString());

                        for (int k = oldCounter; k < counter; k++)
                        {
                            // clear the squares that are supposed to be empty
                            if (k < 8)
                            {
                                board[k, 7 - number] = new Piece();
                                newBoard[k, 7 - number] = PieceStuff.None;
                            }
                        }
                    }
                    else if (char.IsLetter(row[i]))
                    {
                        // there is a piece
                        board[counter, 7 - number] = letterToPiece[row[i]];
                        board[counter, 7 - number].position = new Position(counter, 7 - number);

                        newBoard[counter, 7 - number] = letterToByte[row[i]];

                        counter++;
                    }
                }

            }
        }

        UpdateChessBoardUI();
    }

    public static void UpdateChessBoardUI()
    {
        piecesOnBoard = new List<Piece>();
        for (int i = 0; i < 8; i++)
        {

            for (int j = 0; j < 8; j++)
            {
                if (newBoard[i, j] == PieceStuff.None)
                {
                    boardUI[i, j].pieceImage.enabled = false;
                    boardUI[i, j].pieceImage.sprite = null;
                }
                else
                {
                    boardUI[i, j].pieceImage.enabled = true;
                    byte piece = newBoard[i, j];
                    boardUI[i, j].pieceImage.sprite = byteToPrefab[piece].sprite;
                    newBoard[i, j] = piece;

                    if (IsPieceType(piece, PieceStuff.King))
                    {
                        kingsPosition[(IsColour(piece, PieceStuff.White) ? Colour.White : Colour.Black)] = new Position(i, j);
                    }
                }

            }
        }

        // convert board to fen string
        fenString = "";

        for (int i = 0; i < 8; i++)
        {
            int counter = 0;
            for (int j = 0; j < 8; j++)
            {
                if (IsPieceType(newBoard[j, i], PieceStuff.None))
                {
                    counter++;
                    continue;
                }

                // if on a real piece and counter is greater than 0
                if (counter > 0)
                {
                    fenString += counter.ToString();
                    counter = 0;
                }

                if (IsColour(newBoard[j, i], PieceStuff.White))
                {
                    fenString += BytePieceCharacter(GetPieceType(newBoard[j, i])).ToUpper();
                }
                else
                {
                    fenString += BytePieceCharacter(GetPieceType(newBoard[j, i])).ToLower();
                }

            }
            if (counter > 0)
            {
                fenString += counter.ToString();
            }
            if (i < 7)
            {
                fenString += "/";
            }
        }

        // bwString
        bwString = "";
        for (int i = 0; i < 8; i++)
        {
            int counter = 0;
            for (int j = 0; j < 8; j++)
            {
                if (IsPieceType(newBoard[j, i], PieceStuff.None))
                {
                    counter++;
                    continue;
                }

                // if on a real piece and counter is greater than 0
                if (counter > 0)
                {
                    bwString += counter.ToString();
                    counter = 0;
                }

                if (IsColour(newBoard[j, i], PieceStuff.White))
                {
                    bwString += "W";
                }
                else if (IsColour(newBoard[j, i], PieceStuff.Black))
                {
                    bwString += "B";
                }
                else
                {
                    bwString += "0";
                }


            }
            if (counter > 0)
            {
                bwString += counter.ToString();
            }
            if (i < 7)
            {
                bwString += "/";
            }
        }



    }

    public static string BytePieceCharacter(byte type)
    {
        switch (type)
        {
            case PieceStuff.Pawn:
                return "p";

            case PieceStuff.Rook:
                return "r";

            case PieceStuff.Knight:
                return "n";

            case PieceStuff.Bishop:
                return "b";

            case PieceStuff.Queen:
                return "q";

            case PieceStuff.King:
                return "k";

            case PieceStuff.None:
                return "0";

            default:
                return " ";
        }
    }

    public static string PieceCharacter(PieceType type)
    {
        switch (type)
        {
            case PieceType.Pawn:
                return "p";

            case PieceType.Rook:
                return "r";

            case PieceType.Knight:
                return "n";

            case PieceType.Bishop:
                return "b";

            case PieceType.Queen:
                return "q";

            case PieceType.King:
                return "k";

            default:
                return " ";
        }
    }

    public static Colour SwitchColour(Colour colour)
    {
        return (colour == Colour.White) ? Colour.Black : Colour.White;
    }

    public static CastlingOptions ChangeCastlingOptions(CastlingOptions currentCastling, CastlingOptions optionToRemove)
    {
        if (currentCastling == CastlingOptions.None)
        {
            return currentCastling;
        }
        else if (currentCastling == CastlingOptions.Both)
        {
            if (optionToRemove == CastlingOptions.None)
            {
                return currentCastling;
            }
            else if (optionToRemove == CastlingOptions.Kingside)
            {
                return CastlingOptions.Queenside;
            }
            else if (optionToRemove == CastlingOptions.Queenside)
            {
                return CastlingOptions.Kingside;
            }
            else
            {
                return CastlingOptions.None;
            }
        }
        else if (currentCastling == CastlingOptions.Kingside)
        {
            if (optionToRemove == CastlingOptions.None)
            {
                return currentCastling;
            }
            else if (optionToRemove == CastlingOptions.Kingside)
            {
                return CastlingOptions.None;
            }
            else if (optionToRemove == CastlingOptions.Queenside)
            {
                return currentCastling;
            }
            else
            {
                return CastlingOptions.None;
            }
        }
        else if (currentCastling == CastlingOptions.Queenside)
        {
            // queenside castling
            if (optionToRemove == CastlingOptions.None)
            {
                return currentCastling;
            }
            else if (optionToRemove == CastlingOptions.Kingside)
            {
                return currentCastling;
            }
            else if (optionToRemove == CastlingOptions.Queenside)
            {
                return CastlingOptions.None;
            }
            else
            {
                return CastlingOptions.None;
            }
        }
        return currentCastling;

    }

    public static int PawnDirection(Colour colour)
    {
        if (colour == Colour.White)
        {
            return 1;
        }
        return -1;
    }

    public static Colour ByteToColour(byte colour)
    {
        if (colour == PieceStuff.Black)
        {
            return Colour.Black;
        }
        return Colour.White;
    }

    public static byte SwitchByteColour(byte colour)
    {
        if (colour == PieceStuff.Black)
        {
            return PieceStuff.White;
        }
        return PieceStuff.Black;
    }

    public static void MakeMoveTemporary(ref byte[,] chessBoard, Move move, bool undo, byte capturedPieceForUndo = PieceStuff.None, CastlingOptions oldCastling = CastlingOptions.None)
    {

        if (!undo)
        {
            byte colourMovingByte = GetColour(chessBoard[move.startPos.x, move.startPos.y]);
            byte pieceTypeMoving = GetPieceType(chessBoard[move.startPos.x, move.startPos.y]);
            Colour colourMoving = ByteToColour(colourMovingByte);


            chessBoard[move.targetPos.x, move.targetPos.y] = chessBoard[move.startPos.x, move.startPos.y];
            chessBoard[move.startPos.x, move.startPos.y] = PieceStuff.None;


            if (pieceTypeMoving == PieceStuff.King)
            {
                kingsPosition[colourMoving] = new Position(move.targetPos.x, move.targetPos.y);
                // king can no longer castle after moving
                kingsCastling[colourMoving] = CastlingOptions.None;
            }
            else if (pieceTypeMoving == PieceStuff.Rook && kingsCastling[colourMoving] != CastlingOptions.None)
            {
                if (move.startPos.x == 7 && (kingsCastling[colourMoving] == CastlingOptions.Both || kingsCastling[colourMoving] == CastlingOptions.Kingside))
                {
                    // your boy the king side rook moved
                    kingsCastling[colourMoving] = (kingsCastling[colourMoving] == CastlingOptions.Both) ? CastlingOptions.Queenside : CastlingOptions.None;
                }
                else if (move.startPos.x == 0 && (kingsCastling[colourMoving] == CastlingOptions.Both || kingsCastling[colourMoving] == CastlingOptions.Queenside))
                {
                    // your boy the queen side rook moved
                    kingsCastling[colourMoving] = (kingsCastling[colourMoving] == CastlingOptions.Both) ? CastlingOptions.Kingside : CastlingOptions.None;
                }
            }

            if (move.castling == CastlingOptions.Kingside)
            {
                chessBoard[5, move.targetPos.y] = chessBoard[7, move.targetPos.y];
                chessBoard[7, move.targetPos.y] = PieceStuff.None;
            }
            else if (move.castling == CastlingOptions.Queenside)
            {
                chessBoard[3, move.targetPos.y] = chessBoard[0, move.targetPos.y];
                chessBoard[0, move.targetPos.y] = PieceStuff.None;
            }
            else if (move.newPromotion != PieceStuff.None)
            {
                chessBoard[move.targetPos.x, move.targetPos.y] = (byte)(move.newPromotion + colourMovingByte);
            }
            else if (move.enPassant)
            {
                // do en passant
                // piece behind the target square is captured
                chessBoard[move.targetPos.x, move.targetPos.y - PawnDirection(colourMoving)] = PieceStuff.None;
            }
        }
        else
        {
            byte colourMovingByte = GetColour(chessBoard[move.targetPos.x, move.targetPos.y]);
            byte pieceTypeMoving = GetPieceType(chessBoard[move.targetPos.x, move.targetPos.y]);
            Colour colourMoving = ByteToColour(colourMovingByte);

            chessBoard[move.startPos.x, move.startPos.y] = chessBoard[move.targetPos.x, move.targetPos.y];
            chessBoard[move.targetPos.x, move.targetPos.y] = capturedPieceForUndo;

            if (pieceTypeMoving == PieceStuff.King)
            {
                kingsPosition[colourMoving] = new Position(move.startPos.x, move.startPos.y);
            }

            if (move.castling == CastlingOptions.Kingside)
            {
                chessBoard[7, move.startPos.y] = chessBoard[5, move.startPos.y];
                chessBoard[5, move.startPos.y] = PieceStuff.None;
            }
            else if (move.castling == CastlingOptions.Queenside)
            {
                chessBoard[0, move.startPos.y] = chessBoard[3, move.startPos.y];
                chessBoard[3, move.startPos.y] = PieceStuff.None;
            }
            else if (move.promotion != PieceType.None)
            {
                chessBoard[move.startPos.x, move.startPos.y] = (byte)(colourMovingByte + PieceStuff.Pawn);
            }
            else if (move.enPassant)
            {
                // do en passant
                // piece behind the target square is captured
                chessBoard[move.targetPos.x, move.targetPos.y - PawnDirection(colourMoving)] = (byte)(PieceStuff.Pawn + SwitchByteColour(colourMovingByte));
            }
            kingsCastling[colourMoving] = oldCastling;

        }
    }

    public static void MakeMoveTemporaryf(ref Piece[,] chessBoard, Move move, Piece targetPiece, bool hasPieceToReverse)
    {
        Colour colourMoving = chessBoard[move.startPos.x, move.startPos.y].colour;
        PieceType typeMoving = chessBoard[move.startPos.x, move.startPos.y].type;
        // replace target position square with the piece that is being moved
        chessBoard[move.targetPos.x, move.targetPos.y] = chessBoard[move.startPos.x, move.startPos.y];
        chessBoard[move.targetPos.x, move.targetPos.y].position = new Position(move.targetPos.x, move.targetPos.y);

        if (move.promotion != PieceType.None && !hasPieceToReverse)
        {
            chessBoard[move.targetPos.x, move.targetPos.y].type = move.promotion;
        }

        if (move.enPassant && !hasPieceToReverse)
        {
            // do en passant
            // piece behind the target square is captured
            chessBoard[move.targetPos.x, move.targetPos.y - PawnDirection(colourMoving)] = new Piece();
        }

        if (typeMoving == PieceType.King)
        {
            kingsPosition[chessBoard[move.targetPos.x, move.targetPos.y].colour] = new Position(move.targetPos.x, move.targetPos.y);
            // king can no longer castle after moving
            kingsCastling[chessBoard[move.targetPos.x, move.targetPos.y].colour] = CastlingOptions.None;

            if (!hasPieceToReverse)
            {
                if (move.castling == CastlingOptions.Kingside)
                {
                    Move rookMove = new Move(new Position(7, move.targetPos.y), new Position(5, move.targetPos.y));
                    MakeMoveTemporaryf(ref chessBoard, rookMove, new Piece(), false);
                }
                else if (move.castling == CastlingOptions.Queenside)
                {
                    Move rookMove = new Move(new Position(0, move.targetPos.y), new Position(3, move.targetPos.y));
                    MakeMoveTemporaryf(ref chessBoard, rookMove, new Piece(), false);
                }
            }

        }
        else if (typeMoving == PieceType.Rook && kingsCastling[chessBoard[move.targetPos.x, move.targetPos.y].colour] != CastlingOptions.None)
        {
            if (move.startPos.x == 7)
            {
                // your boy the king side rook moved
                kingsCastling[chessBoard[move.targetPos.x, move.targetPos.y].colour] = ChangeCastlingOptions(kingsCastling[chessBoard[move.targetPos.x, move.targetPos.y].colour],
                    CastlingOptions.Kingside);
            }
            else if (move.startPos.x == 0)
            {
                // your boy the queen side rook moved
                kingsCastling[chessBoard[move.targetPos.x, move.targetPos.y].colour] = ChangeCastlingOptions(kingsCastling[chessBoard[move.targetPos.x, move.targetPos.y].colour],
                    CastlingOptions.Queenside);
            }
        }

        // change position on the the target piece variable
        chessBoard[move.startPos.x, move.startPos.y] = targetPiece;
        if (hasPieceToReverse)
        {
            targetPiece.position = new Position(move.startPos.x, move.startPos.y);

            if (move.castling == CastlingOptions.Kingside)
            {
                // we have to undo castling
                Move rookMove = new Move(new Position(5, move.targetPos.y), new Position(7, move.targetPos.y));
                MakeMoveTemporaryf(ref chessBoard, rookMove, new Piece(), false);
            }
            else if (move.castling == CastlingOptions.Queenside)
            {
                // we have to undo castling
                Move rookMove = new Move(new Position(3, move.targetPos.y), new Position(0, move.targetPos.y));
                MakeMoveTemporaryf(ref chessBoard, rookMove, new Piece(), false);
            }

            if (chessBoard[move.targetPos.x, move.targetPos.y].type == PieceType.King)
            {
                kingsPosition[targetPiece.colour] = new Position(move.targetPos.x, move.targetPos.y);
            }

            if (move.promotion != PieceType.None)
            {
                chessBoard[move.targetPos.x, move.targetPos.y].type = PieceType.Pawn;
            }
            else if (move.enPassant)
            {
                chessBoard[move.startPos.x, move.startPos.y - PawnDirection(colourMoving)] = new Piece(PieceType.Pawn, SwitchColour(colourMoving),
                    new Position(move.startPos.x, move.startPos.y - PawnDirection(colourMoving)));
            }
        }
    }

    public static bool SquareExists(int x, int y)
    {
        return x >= 0 && x <= 7 && y >= 0 && y <= 7;
    }

    public static void PromotePawn(ref Piece[,] board, Piece pawn, PieceType type = PieceType.Queen)
    {
        // this is after the pawn moves
        board[pawn.position.x, pawn.position.y].type = type;
    }

    public static Piece[,] DuplicateBoard(Piece[,] boardToCopy)
    {
        Piece[,] copy = new Piece[8, 8];

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                copy[i, j] = new Piece(boardToCopy[i, j]);
            }
        }
        return copy;
    }

    public static List<Move> GenerateMovesForColour(byte colourToMove, byte[,] board)
    {
        List<Move> movesForColour = new List<Move>();

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (GetColour(board[i, j]) == colourToMove && GetPieceType(board[i, j]) != PieceStuff.None)
                {
                    List<Move> movesForPiece = GenerateMovesForPiece(board[i, j], board, new Position(i, j));

                    if (movesForPiece.Count > 0)
                    {
                        movesForColour.AddRange(movesForPiece);
                    }
                }
            }
        }
        return movesForColour;
    }

    
    public static List<Move> GenerateMovesForPiece(byte piece, byte[,] board, Position position)
    {
        List<Move> legalMoves = new List<Move>();
        int x = position.x;
        int y = position.y;
        byte colourByte = GetColour(piece);
        Colour colour = ByteToColour(colourByte);
        byte type = GetPieceType(piece);

        if (IsPieceType(piece, PieceStuff.King))
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    // prevent 0,0 offset where king does not move
                    if (i == 0 && j == 0)
                    {
                        continue;
                    }

                    Move move = new Move(position, new Position(x + i, y + j));
                    if (SquareExists(x + i, y + j) && IsLegalMove(board, piece, move, colourByte) && IsKingSafeAfterMove(board, colourByte, move))
                    {
                        //Debug.Log("A king move");
                        //if (!move.targetPos.Equals(move.startPos))
                        //{
                        legalMoves.Add(move);
                        //}

                    }
                }
            }
            if (kingsCastling[colour] == CastlingOptions.Kingside || kingsCastling[colour] == CastlingOptions.Both)
            {
                // can castle kingside
                if (IsPieceType(board[5, y], PieceStuff.None) && IsPieceType(board[6, y], PieceStuff.None) && IsPieceType(board[7, y], PieceStuff.Rook) && IsColour(board[7, y], colourByte))
                {
                    // add kingside castling move
                    Move move = new Move(position, new Position(x + 2, y), PieceType.None, CastlingOptions.Kingside);

                    if (IsLegalMove(board, piece, move, colourByte) && IsKingSafeAfterMove(board, colourByte, move))
                    {
                        // Debug.Log("Can castle kingside");
                        legalMoves.Add(move);

                    }
                }
            }
            if (kingsCastling[colour] == CastlingOptions.Queenside || kingsCastling[colour] == CastlingOptions.Both)
            {
                // can castle queenside
                if (IsPieceType(board[1, y], PieceStuff.None) && IsPieceType(board[2, y], PieceStuff.None) && IsPieceType(board[3, y], PieceStuff.None) && IsPieceType(board[0, y], PieceStuff.Rook) && IsColour(board[0, y], colourByte))
                {
                    // add queenside castling move
                    Move move = new Move(position, new Position(x - 2, y), PieceType.None, CastlingOptions.Queenside);

                    if (IsLegalMove(board, piece, move, colourByte) && IsKingSafeAfterMove(board, colourByte, move))
                    {
                        // Debug.Log("Can castle queenside");
                        legalMoves.Add(move);

                    }
                }
            }
        }
        else if (type == PieceStuff.Knight)
        {
            foreach (Vector2Int offset in knightOffsets)
            {
                Move move = new Move(position, new Position(x + offset.x, y + offset.y));
                if (SquareExists(x + offset.x, y + offset.y) && IsLegalMove(board, piece, move, colourByte) && IsKingSafeAfterMove(board, colourByte, move))
                {
                    if (!move.targetPos.Equals(move.startPos))
                    {
                        legalMoves.Add(move);
                    }
                }
            }
        }
        else if (type == PieceStuff.Bishop)
        {
            for (int i = -x; i < 8 - x; i++)
            {
                for (int j = -1; j <= 1; j += 2)
                {
                    Move move = new Move(position, new Position(x + i, y + i * j));
                    if (SquareExists(x + i, y + i * j) && IsLegalMove(board, piece, move, colourByte) && IsKingSafeAfterMove(board, colourByte, move))
                    {
                        //Debug.Log("A bishop move");
                        if (!move.targetPos.Equals(move.startPos))
                        {
                            legalMoves.Add(move);
                        }
                    }
                }
            }
        }
        else if (type == PieceStuff.Rook)
        {
            for (int i = -x; i < 8 - x; i++)
            {
                Move move = new Move(position, new Position(x + i, y));
                if (SquareExists(i + x, y) && IsLegalMove(board, piece, move, colourByte) && IsKingSafeAfterMove(board, colourByte, move))
                {
                    //Debug.Log("A rook move");
                    if (!move.targetPos.Equals(move.startPos))
                    {
                        legalMoves.Add(move);
                    }
                }
                /*else
                    break;*/
            }

            for (int j = 0 - y; j < 8 - y; j++)
            {
                Move move = new Move(position, new Position(x, y + j));
                if (SquareExists(x, y + j) && IsLegalMove(board, piece, move, colourByte) && IsKingSafeAfterMove(board, colourByte, move))
                {
                    //Debug.Log("A rook move");
                    if (!move.targetPos.Equals(move.startPos))
                    {
                        legalMoves.Add(move);
                    }
                }
                /*else
                    break;*/
            }
        }
        else if (type == PieceStuff.Queen)
        {
            for (int i = 0 - x; i < 8 - x; i++)
            {
                for (int j = -1; j <= 1; j += 2)
                {
                    Move move = new Move(position, new Position(x + i, y + i * j));
                    if (SquareExists(x + i, y + i * j) && IsLegalMove(board, piece, move, colourByte) && IsKingSafeAfterMove(board, colourByte, move))
                    {
                        //Debug.Log("A bishop move");
                        if (!move.targetPos.Equals(move.startPos))
                        {
                            legalMoves.Add(move);
                        }
                    }
                }
            }

            for (int i = 0 - x; i < 8 - x; i++)
            {
                Move move = new Move(position, new Position(x + i, y));
                if (SquareExists(i + x, y) && IsLegalMove(board, piece, move, colourByte) && IsKingSafeAfterMove(board, colourByte, move))
                {
                    //Debug.Log("A queen rook-like move");
                    if (!move.targetPos.Equals(move.startPos))
                    {
                        legalMoves.Add(move);
                    }
                }
                /*else
                    break;*/
            }

            for (int j = 0 - y; j < 8 - y; j++)
            {
                Move move = new Move(position, new Position(x, y + j));
                if (SquareExists(x, y + j) && IsLegalMove(board, piece, move, colourByte) && IsKingSafeAfterMove(board, colourByte, move))
                {
                    //Debug.Log("A queen rook-like move");
                    if (!move.targetPos.Equals(move.startPos))
                    {
                        legalMoves.Add(move);
                    }
                }
                /*else
                    break;*/
            }
        }
        else if (type == PieceStuff.Pawn)
        {
            int direction = 1;
            if (colourByte == PieceStuff.Black)
            {
                direction = -1;
            }

            for (int i = -1; i <= 1; i++)
            {
                Move move = new Move(position, new Position(x + i, y + direction));
                if (SquareExists(x + i, y + direction))
                {
                    if (IsLegalMove(board, piece, move, colourByte))
                    {
                        if (IsKingSafeAfterMove(board, colourByte, move))
                        {
                            if (!move.targetPos.Equals(move.startPos))
                            {
                                if ((direction == 1 && y == 6) || (direction == -1 && y == 1))
                                {
                                    // pawn is promoting
                                    Move promotion = new Move(move.startPos, move.targetPos, PieceType.None, CastlingOptions.None, false, PieceStuff.Queen);
                                    legalMoves.Add(promotion);
                                    promotion = new Move(move.startPos, move.targetPos, PieceType.None, CastlingOptions.None, false, PieceStuff.Rook);
                                    legalMoves.Add(promotion);
                                    promotion = new Move(move.startPos, move.targetPos, PieceType.None, CastlingOptions.None, false, PieceStuff.Bishop);
                                    legalMoves.Add(promotion);
                                    promotion = new Move(move.startPos, move.targetPos, PieceType.None, CastlingOptions.None, false, PieceStuff.Knight);
                                    legalMoves.Add(promotion);
                                }
                                else
                                    legalMoves.Add(move);
                            }
                            //else
                            //Debug.Log((x + 1) + ", " + (y + direction) + " same pos");

                        }
                        //else
                        //Debug.Log((x + 1) + ", " + (y + direction) + " not king safe");

                    }
                    //else
                    //Debug.Log((x + 1) + ", " + (y + direction) + " is not a legal move");
                }
                //else
                //Debug.Log((x + 1) + ", " + (y + direction) + " does not exist");


                move = new Move(position, new Position(x + i, y + direction * 2));
                if (SquareExists(x + i, y + direction * 2))
                {
                    if (IsLegalMove(board, piece, move, colourByte))
                    {
                        if (IsKingSafeAfterMove(board, colourByte, move))
                        {
                            legalMoves.Add(move);
                        }
                    }
                }
            }
        }
        //Debug.Log("There are " + legalMoves.Count + " legal moves");
        return legalMoves;
    }


    public static bool IsKingSafeAfterMove(byte[,] originalBoard, byte colourToMoveByte, Move move)
    {
        byte[,] chessBoardClone = (byte[,])originalBoard.Clone();

        Colour colourToMove = ByteToColour(colourToMoveByte);


        CastlingOptions oldCastlingOptions = kingsCastling[colourToMove];


        MakeMoveTemporary(ref chessBoardClone, move, false);

        Position thisKingPosition = kingsPosition[colourToMove];

        Colour oppositeColour = SwitchColour(colourToMove);
        byte oppositeColourByte = SwitchByteColour(colourToMoveByte);

        // check all knights first
        for (int i = 0; i < knightOffsets.Length; i++)
        {
            if (SquareExists(thisKingPosition.x + knightOffsets[i].x, thisKingPosition.y + knightOffsets[i].y))
            {
                if (IsColour(chessBoardClone[thisKingPosition.x + knightOffsets[i].x, thisKingPosition.y + knightOffsets[i].y], oppositeColourByte))
                {
                    if (IsPieceType(chessBoardClone[thisKingPosition.x + knightOffsets[i].x, thisKingPosition.y + knightOffsets[i].y], PieceStuff.Knight))
                    {
                        kingsCastling[colourToMove] = oldCastlingOptions;

                        return false;
                    }
                }
            }
        }


        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                {
                    // this is the same square as the king
                    continue;
                }

                // the king is safe here
                // friendly piece blocking and protecting the king from other pieces
                if (SquareExists(thisKingPosition.x + i, thisKingPosition.y + j))
                {
                    if (IsColour(chessBoardClone[thisKingPosition.x + i, thisKingPosition.y + j], colourToMoveByte))
                    {
                        continue;
                    }
                    else if (IsColour(chessBoardClone[thisKingPosition.x + i, thisKingPosition.y + j], oppositeColourByte))
                    {
                        // if black king is around
                        if (IsPieceType(chessBoardClone[thisKingPosition.x + i, thisKingPosition.y + j], PieceStuff.King))
                        {
                            kingsCastling[colourToMove] = oldCastlingOptions;
                            // Debug.Log("King Around");
                            return false;
                        }
                    }
                }

                // check up, down, left, right
                if ((i == 0 && j != 0) || (j == 0 && i != 0))
                {
                    for (int k = 1; k < 8; k++)
                    {
                        bool blackChecked = false;

                        if (SquareExists(thisKingPosition.x + i * k, thisKingPosition.y + j * k) && !blackChecked)
                        {
                            if (IsColour(chessBoardClone[thisKingPosition.x +i *k, thisKingPosition.y+j*k], colourToMoveByte))
                            {
                                break;
                            }


                            if (IsColour(chessBoardClone[thisKingPosition.x + i * k, thisKingPosition.y + j * k], oppositeColourByte))
                            {
                                blackChecked = true;

                                if (IsPieceType(chessBoardClone[thisKingPosition.x + i * k, thisKingPosition.y + j * k], PieceStuff.Rook) ||
                                    IsPieceType(chessBoardClone[thisKingPosition.x + i * k, thisKingPosition.y + j * k], PieceStuff.Queen))
                                {

                                    kingsCastling[colourToMove] = oldCastlingOptions;

                                    return false;
                                }
                            }
                        }
                        else
                        {
                            // square does not exist
                            break;
                        }
                    }
                }
                else if (i != 0 && j != 0)
                {
                    // diagonal
                    // pawns

                    // check if pawn
                    if (SquareExists(thisKingPosition.x + i, thisKingPosition.y + j))
                    {
                        if (IsPieceType(chessBoardClone[thisKingPosition.x + i, thisKingPosition.y + j], PieceStuff.Pawn))
                        {
                            // if opposite color is black
                            if (oppositeColour == Colour.Black)
                            {
                                // check if pawn is above king (it will be a diagonal for sure already)
                                if (thisKingPosition.y < thisKingPosition.y + j)
                                {
                                    kingsCastling[colourToMove] = oldCastlingOptions;
                                    return false;
                                }
                            }
                            else
                            {
                                // check if pawn is below king (it will be a diagonal for sure already)
                                if (thisKingPosition.y > thisKingPosition.y + j)
                                {
                                    kingsCastling[colourToMove] = oldCastlingOptions;
                                    return false;
                                }
                            }
                        }
                    }

                    // check diagnol for bishop, queen, and king
                    for (int k = 1; k < 8; k++)
                    {
                        bool blackChecked = false;
                        
                        if (SquareExists(thisKingPosition.x + i * k, thisKingPosition.y + j * k))
                        {
                            if (IsColour(chessBoardClone[thisKingPosition.x + i * k, thisKingPosition.y + j * k], colourToMoveByte))
                            {
                                break;
                            }

                            if (IsColour(chessBoardClone[thisKingPosition.x + i * k, thisKingPosition.y + j * k], oppositeColourByte))
                            {
                                blackChecked = true;
                                if (IsPieceType(chessBoardClone[thisKingPosition.x + i * k, thisKingPosition.y + j * k], PieceStuff.Queen) ||
                                    IsPieceType(chessBoardClone[thisKingPosition.x + i * k, thisKingPosition.y + j * k], PieceStuff.Bishop))
                                {
                                    kingsCastling[colourToMove] = oldCastlingOptions;
                                    return false;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        kingsCastling[colourToMove] = oldCastlingOptions;
        return true;
    }

    
    public static int Sign(int n)
    {
        if (n >= 0)
        {
            return 1;
        }
        else
        {
            return -1;
        }
    }

    public static bool IsLegalMove(byte[,] board, byte pieceToMove, Move move, byte colourToMoveByte)
    {
        int startX = move.startPos.x;
        int startY = move.startPos.y;
        int targetX = move.targetPos.x;
        int targetY = move.targetPos.y;
        Colour colourToMove = ByteToColour(colourToMoveByte);

        if (startX == targetX && startY == targetY)
        {
            return false;
        }

        if (IsColour(board[targetX, targetY], colourToMoveByte))
        {
            return false;
        }

        if (!IsColour(pieceToMove, colourToMoveByte))
        {
            return false;
        }

        // if king is moving 1
        if (IsPieceType(pieceToMove, PieceStuff.King) && Mathf.Abs(startX - targetX) <= 1 && Mathf.Abs(startY - targetY) <= 1)
        {
            return true;
        }

        if (IsPieceType(pieceToMove, PieceStuff.King) && startY == targetY && Mathf.Abs(startX - targetX) == 2 && kingsCastling[colourToMove] != CastlingOptions.None)
        {

            // castling shenanigans
            if ((kingsCastling[colourToMove] == CastlingOptions.Kingside || kingsCastling[colourToMove] == CastlingOptions.Both) && targetX == 6)
            {
                if (IsPieceType(board[5, startY], PieceStuff.None) && IsPieceType(board[6, startY], PieceStuff.None))
                {
                    if (IsPieceType(board[7, startY], PieceStuff.Rook) && IsColour(board[7, startY], colourToMoveByte))
                    {
                        return true;
                    }
                }
            }
            if ((kingsCastling[colourToMove] == CastlingOptions.Queenside || kingsCastling[colourToMove] == CastlingOptions.Both) && targetX == 2)
            {
                if (IsPieceType(board[1, startY], PieceStuff.None) && IsPieceType(board[2, startY], PieceStuff.None) && IsPieceType(board[3, startY], PieceStuff.None) && IsPieceType(board[0, startY], PieceStuff.Rook) && IsColour(board[0, startY], colourToMoveByte))
                {
                    // add queenside castling move
                    return true;
                }
            }
        }
        else if (IsPieceType(pieceToMove, PieceStuff.Knight))
        {
            if (Mathf.Abs(startX - targetX) == 1 && Mathf.Abs(startY - targetY) == 2)
            {
                return true;
            }
            else if (Mathf.Abs(startX - targetX) == 2 && Mathf.Abs(startY - targetY) == 1)
            {
                return true;
            }
        }
        else if (IsPieceType(pieceToMove, PieceStuff.Bishop))
        {
            if (Mathf.Abs(startX - targetX) == Mathf.Abs(startY - targetY))
            {
                for (int i = 1; i < Mathf.Abs(startX - targetX); i++)
                {
                    if (board[startX + i * Sign(targetX - startX), startY + i * Sign(targetY - startY)] != PieceStuff.None)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        else if (IsPieceType(pieceToMove, PieceStuff.Rook))
        {
            if (startX - targetX == 0 && Mathf.Abs(startY - targetY) != 0)
            {
                for (int i = 1; i < Mathf.Abs(startY - targetY); i++)
                {
                    if (board[startX, startY + i * Sign(targetY - startY)] != PieceStuff.None)
                    {
                        return false;
                    }
                }
                return true;
            }
            else if (startY - targetY == 0 && Mathf.Abs(startX - targetX) != 0)
            {
                for (int i = 1; i < Mathf.Abs(startX - targetX); i++)
                {
                    if (board[startX + i * Sign(targetX - startX), startY] != PieceStuff.None)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        else if (IsPieceType(pieceToMove, PieceStuff.Queen))
        {
            if (startX - targetX == 0 && Mathf.Abs(startY - targetY) != 0)
            {
                // rook Y
                for (int i = 1; i < Mathf.Abs(startY - targetY); i++)
                {
                    if (board[startX, startY + i * Sign(targetY - startY)] != PieceStuff.None)
                    {
                        return false;
                    }
                }
                return true;
            }
            else if (startY - targetY == 0 && Mathf.Abs(startX - targetX) != 0)
            {
                // rook X
                for (int i = 1; i < Mathf.Abs(startX - targetX); i++)
                {
                    if (board[startX + i * Sign(targetX - startX), startY] != PieceStuff.None)
                    {
                        return false;
                    }
                }
                return true;
            }
            else if (Mathf.Abs(startX - targetX) == Mathf.Abs(startY - targetY))
            {
                // Bishop
                for (int i = 1; i < Mathf.Abs(startX - targetX); i++)
                {
                    if (board[startX + i * Sign(targetX - startX), startY + i * Sign(targetY - startY)] != PieceStuff.None)
                    {
                        return false;
                    }

                }
                return true;
            }
        }
        else if (IsPieceType(pieceToMove, PieceStuff.Pawn))
        {
            int direction;
            bool canMoveTwoSquares;
            byte captureColour;

            if (IsColour(pieceToMove, PieceStuff.White))
            {
                canMoveTwoSquares = (startY == 1) ? true : false;
                direction = 1;
                captureColour = PieceStuff.Black;
            }
            else
            {
                canMoveTwoSquares = (startY == 6) ? true : false;
                direction = -1;
                captureColour = PieceStuff.White;
            }

            if (canMoveTwoSquares)
            {
                if (targetY - startY != direction && targetY - startY != direction * 2)
                {
                    return false;
                }
            }
            else
            {
                if (targetY - startY != direction)
                {
                    return false;
                }
            }

            if (targetX - startX == 0)
            {
                // moving in a straight line, the square in front cannot be occupied by any piece
                if (canMoveTwoSquares)
                {
                    if (targetY - startY == direction)
                    {
                        // one square forward
                        if (board[targetX, targetY] != PieceStuff.None)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        // two squares forward
                        if (board[targetX, targetY - direction] != PieceStuff.None || board[targetX, targetY] != PieceStuff.None)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    if (board[targetX, targetY] != PieceStuff.None)
                    {
                        // one square forward
                        return false;
                    }
                }
            }

            if (targetX - startX != 0 && targetY - startY != direction)
            {
                // there has been a capture, and during a capture, a pawn can only move one square forward
                return false;
            }

            if (targetX - startX != 0 && !IsColour(board[targetX, targetY], captureColour))
            {
                return false;
            }
            return true;
        }

        return false;
    }

}
