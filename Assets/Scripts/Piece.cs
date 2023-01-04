using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Piece
{
    public PieceType type;
    public Colour colour;
    public Position position;
    public float value;

    public Piece(Piece piece)
    {
        type = piece.type;
        colour = piece.colour;
        value = piece.value;
        position = piece.position;
    }

    public Piece(PieceType type = PieceType.None, Colour colour = Colour.None, Position position = new Position())
    {
        this.type = type;
        this.colour = colour;
        this.position = position;

        if (type == PieceType.Pawn)
        {
            value = 1;
        }
        else if (type == PieceType.Knight)
        {
            value = 3;
        }
        else if (type == PieceType.Bishop)
        {
            value = 3;
        }
        else if (type == PieceType.Rook)
        {
            value = 5;
        }
        else if (type == PieceType.Queen)
        {
            value = 9;
        }
        else if (type == PieceType.King)
        {
            value = 10000;
        }
        else
        {
            value = 0;
        }
    }
}

public enum PieceType { None, Pawn, Knight, Bishop, Rook, Queen, King }
public enum Colour { None, White, Black }

public struct Move
{
    public Position startPos;
    public Position targetPos;
    public PieceType promotion;
    public byte newPromotion;
    public CastlingOptions castling;
    public bool enPassant;

    public Move(Position startPos, Position targetPos, PieceType promotion = PieceType.None, CastlingOptions castling = CastlingOptions.None, bool enPassant = false, byte newPromotion = PieceStuff.None)
    {
        this.startPos = startPos;
        this.targetPos = targetPos;
        this.promotion = promotion;
        this.castling = castling;
        this.enPassant = enPassant;
        this.newPromotion = newPromotion;
    }
}

public struct PieceStuff
{
    public const byte None = 0;
    public const byte Pawn = 1;
    public const byte Knight = 2;
    public const byte Bishop = 3;
    public const byte Rook = 4;
    public const byte Queen = 5;
    public const byte King = 6;

    public const byte White = 8;
    public const byte Black = 16;
}

public struct Position
{
    public int x;
    public int y;

    public Position(int x = 9, int y = 9)
    {
        this.x = x;
        this.y = y;
    }
}



public enum CastlingOptions { Kingside, Queenside, None, Both}