using System.Collections.Generic;
using TMPro;
using UnityEditorInternal;
using UnityEngine;

public class MoveChecker
{
    public List<(int, int)> MoveableBlocks(Constants.PieceType pieceType, int row, int col)
    {
        List<(int, int)> moveableBlocks = new List<(int, int)>();
        switch (pieceType)
        {
            case Constants.PieceType.Knight:
                int[] dr = { 1, 1, -1, -1, 2, 2, -2, -2 };
                int[] dc = { 2, -2, 2, -2, 1, -1, 1, -1 };
                for (int i = 0; i < 8; i++)
                {
                    moveableBlocks.Add((row + dr[i], col + dc[i]));
                }
                break;
            case Constants.PieceType.Bishop:
                moveableBlocks.AddRange(CalDiagonals(row, col));
                break;
            case Constants.PieceType.Queen:
                moveableBlocks.AddRange(CalDiagonals(row, col));
                moveableBlocks.AddRange(CalStraights(row, col));
                break;
            case Constants.PieceType.Rook:
                moveableBlocks.AddRange(CalStraights(row, col));
                break;
            case Constants.PieceType.Pawn:
                moveableBlocks.Add((row, col + 1));
                // 잡아먹거나 앙파상일경우, 1에 있으면 2칸이동가능도 추가
                break;
            case Constants.PieceType.King:
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (i == 0 && j == 0) continue;
                        moveableBlocks.Add((row + i, col + j));
                    }
                }
                break;
        }
        return moveableBlocks;
    }

    List<(int, int)> CalDiagonals(int row, int col)
    {
        List<(int, int)> diagonals = new List<(int, int)>();
        int upperCol = col;
        int underCol = col;
        // x축으로 왼쪽 방향
        for (int i = row - 1; i >= 0; i--)
        {
            if (++upperCol < Constants.BOARD_SIZE)
                diagonals.Add((i, upperCol));
            if (--underCol >= 0)
                diagonals.Add((i, underCol));
        }

        upperCol = col;
        underCol = col;
        // x축으로 오른쪽 방향
        for (int i = row + 1; i < Constants.BOARD_SIZE; i++)
        {
            if (++upperCol < Constants.BOARD_SIZE)
                diagonals.Add((i, upperCol));
            if (--underCol >= 0)
                diagonals.Add((i, underCol));
        }
        return diagonals;
    }

    List<(int, int)> CalStraights(int row, int col)
    {
        List<(int, int)> straights = new List<(int, int)>();
        for (int i = row - 1; i >= 0; i--)
        {
            straights.Add((i, col));
        }

        for (int i = row + 1; i < Constants.BOARD_SIZE; i++)
        {
            straights.Add((i, col));
        }

        for (int j = col - 1; j >= 0; j--)
        {
            straights.Add((row, j));
        }

        for (int j = col + 1; j < Constants.BOARD_SIZE; j++)
        {
            straights.Add((row, j));
        }
        return straights;
    }
}