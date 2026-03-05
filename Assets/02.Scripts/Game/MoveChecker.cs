using System.Collections.Generic;
using TMPro;
using UnityEditorInternal;
using UnityEngine;

public class MoveChecker
{
    GameLogic _gameLogic;
    Dictionary<(int,int), Block> _blocks =  new Dictionary<(int,int), Block>();
    public List<(int, int)> MoveableBlocks(Constants.PieceType pieceType, (int, int) index)
    {
        _gameLogic = GameManager.Instance.GameLogic;
        _blocks = _gameLogic.BoardController.Blocks;
        List<(int, int)> moveableBlocks = new List<(int, int)>();

        int row = index.Item1;
        int  col = index.Item2;
        
        switch (pieceType)
        {
            case Constants.PieceType.Knight:
                int[] dr = { 1, 1, -1, -1, 2, 2, -2, -2 };
                int[] dc = { 2, -2, 2, -2, 1, -1, 1, -1 };
                for (int i = 0; i < 8; i++)
                {
                    if (row + dr[i] >= Constants.BOARD_SIZE || row + dr[i] < 0 || col + dc[i] >= Constants.BOARD_SIZE || col + dc[i] < 0)
                    {
                        continue;
                    }
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
                if (col + 1 >= Constants.BOARD_SIZE || col + 1 < 0)
                    break;
                moveableBlocks.Add((row, col + 1));
                // 잡아먹거나 앙파상일경우, 1에 있으면 2칸이동가능도 추가
                break;
            case Constants.PieceType.King:
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (row + i >= Constants.BOARD_SIZE || row + i < 0 || col + j >= Constants.BOARD_SIZE || col + j < 0)
                        {
                            continue;
                        }
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
        (int, int) finalGoalBlock;
        int checkCol = col;
        int upper = 1;
        int under = -1;
        // x축으로 왼쪽 방향
        for (int i = row - 1; i >= 0; i--)
        {
            checkCol += upper;
            if (checkCol < Constants.BOARD_SIZE)
            {
                diagonals.Add((i, checkCol));
                if (_blocks[(i, checkCol)].PieceInBlock != null)
                {
                    break;
                }

                continue;
            }
            break;
        }
        checkCol = col;

        for (int i = row - 1; i >= 0; i--)
        {
            checkCol += under;
            if (checkCol >= 0)
            {
                diagonals.Add((i, checkCol));
                if (_blocks[(i, checkCol)].PieceInBlock != null)
                {
                    break;
                }

                continue;
            }

            break;
        }
        checkCol = col;
        
        // x축으로 오른쪽 방향
        for (int i = row + 1; i < Constants.BOARD_SIZE; i++)
        {
            checkCol += upper;
            if (checkCol < Constants.BOARD_SIZE)
            {
                diagonals.Add((i, checkCol));
                if (_blocks[(i, checkCol)].PieceInBlock != null)
                {
                    break;
                }

                continue;
            }

            break;
        }
        checkCol = col;
        
        for (int i = row + 1; i < Constants.BOARD_SIZE; i++)
        {
            checkCol += under;
            if (checkCol >= 0)
            {
                diagonals.Add((i, checkCol));
                if (_blocks[(i, checkCol)].PieceInBlock != null)
                {
                    break;
                }

                continue;
            }

            break;
        }
        return diagonals;
    }

    List<(int, int)> CalStraights(int row, int col)
    {
        List<(int, int)> straights = new List<(int, int)>();
        for (int i = row - 1; i >= 0; i--)
        {
            straights.Add((i, col));
            if (_blocks[(i, col)].PieceInBlock != null)
            {
                break;
            }
        }

        for (int i = row + 1; i < Constants.BOARD_SIZE; i++)
        {
            straights.Add((i, col));
            if (_blocks[(i, col)].PieceInBlock != null)
            {
                break;
            }
        }

        for (int j = col - 1; j >= 0; j--)
        {
            straights.Add((row, j));
            if (_blocks[(row, j)].PieceInBlock != null)
            {
                break;
            }
        }

        for (int j = col + 1; j < Constants.BOARD_SIZE; j++)
        {
            straights.Add((row, j));
            if (_blocks[(row, j)].PieceInBlock != null)
            {
                break;
            }
        }
        return straights;
    }
}