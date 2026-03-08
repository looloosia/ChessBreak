using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;

public class MoveChecker
{
    GameLogic _gameLogic;
    Dictionary<(int,int), Block> _blocks =  new Dictionary<(int,int), Block>();
    BoardController _boardController;
    private Constants.PlayerColor _pieceColor;
    Dictionary<(int, int), Constants.PieceType> _tempDic = new Dictionary<(int, int), Constants.PieceType>();
    public Dictionary<(int, int), Block> MoveableBlocks(Constants.PieceType pieceType, Constants.PlayerColor pieceColor, (int, int) index)
    {
        _gameLogic = GameManager.Instance.GameLogic;
        _boardController = _gameLogic.BoardController;
        _blocks = _boardController.Blocks;
        _pieceColor = pieceColor;
        Dictionary<(int, int), Block> moveableBlocks = new Dictionary<(int, int), Block>();
        
        int row = index.Item1;
        int  col = index.Item2;
        
        switch (pieceType)
        {
            case Constants.PieceType.Knight:
                
                int[] dr = { 1, 1, -1, -1, 2, 2, -2, -2 };
                int[] dc = { 2, -2, 2, -2, 1, -1, 1, -1 };
                int newRow, newCol;
                for (int i = 0; i < 8; i++)
                {
                    newRow =  row + dr[i];
                    newCol  =  col + dc[i];
                    if (newRow >= Constants.BOARD_SIZE || newRow < 0 || newCol >= Constants.BOARD_SIZE || newCol < 0)
                    {
                        continue;
                    }
                    _tempDic[(newRow, newCol)] = pieceType;
                }
                break;
            case Constants.PieceType.Bishop:
                List<(int, int)> bDiagonals = CalDiagonals(row, col, pieceType);
                break;
            case Constants.PieceType.Queen:
                List<(int, int)> qDiagonals = CalDiagonals(row, col, pieceType);
                List<(int, int)> qStraigths = CalStraights(row, col, pieceType);
                break;
            case Constants.PieceType.Rook:
                List<(int, int)> rookStraights = CalStraights(row, col, pieceType);
                break;
            case Constants.PieceType.Pawn:
                if (col + 1 >= Constants.BOARD_SIZE || col + 1 < 0)
                {
                    break;
                }
                _tempDic[(row, col + 1)] = pieceType; 
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
                        _tempDic[(row + i, col + j)] = pieceType;
                    }
                }
                break;
        }
        moveableBlocks.AddRange(FinalDic(_tempDic));
        return moveableBlocks;
    }

    List<(int, int)> CalDiagonals(int row, int col, Constants.PieceType pieceType)
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
                _tempDic[(row, col)] = pieceType;
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
                _tempDic[(row, col)] = pieceType;
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
                _tempDic[(row, col)] = pieceType;
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
                _tempDic[(row, col)] = pieceType;
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

    List<(int, int)> CalStraights(int row, int col, Constants.PieceType pieceType)
    {
        List<(int, int)> straights = new List<(int, int)>();
        for (int i = row - 1; i >= 0; i--)
        {
            _tempDic[(row, col)] = pieceType;
            straights.Add((i, col));
            if (_blocks[(i, col)].PieceInBlock != null)
            {
                break;
            }
        }

        for (int i = row + 1; i < Constants.BOARD_SIZE; i++)
        {
            _tempDic[(row, col)] = pieceType;
            straights.Add((i, col));
            if (_blocks[(i, col)].PieceInBlock != null)
            {
                break;
            }
        }

        for (int j = col - 1; j >= 0; j--)
        {
            _tempDic[(row, col)] = pieceType;
            straights.Add((row, j));
            if (_blocks[(row, j)].PieceInBlock != null)
            {
                break;
            }
        }

        for (int j = col + 1; j < Constants.BOARD_SIZE; j++)
        {
            _tempDic[(row, col)] = pieceType;
            straights.Add((row, j));
            if (_blocks[(row, j)].PieceInBlock != null)
            {
                break;
            }
        }
        return straights;
    }

    public Dictionary<(int, int), Block> FinalDic(Dictionary<(int, int), Constants.PieceType> tempDic)
    {
        Block block;
        Dictionary<(int, int), Block> finalDic = new Dictionary<(int, int), Block>();
        if (_pieceColor == Constants.PlayerColor.None || _pieceColor == null)
        {
            Debug.LogError("MoveChecker: _pieceColor is null or None");
            return null;
        }
        foreach (var pair in tempDic)
        {
            block = _boardController.FindWithTypeColor(pair.Value, _pieceColor);
            finalDic[pair.Key] = block;
        }

        return finalDic;
    }
}