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
    private bool _isValidating;

    public bool IsValidating
    {
        get => _isValidating;
        set => _isValidating = value;
    }

    public Dictionary<(int, int), Piece> MoveableBlocks(Piece piece, Constants.PlayerColor pieceColor, (int, int) index)
    {
        Dictionary<(int, int), Constants.PieceType> tempDic = new Dictionary<(int, int), Constants.PieceType>();
        tempDic.Clear();
        _gameLogic = GameManager.Instance.GameLogic;
        _boardController = _gameLogic.BoardController;
        _blocks = _boardController.Blocks;
        _pieceColor = pieceColor;
        Dictionary<(int, int), Piece> moveableBlocks = new Dictionary<(int, int), Piece>();
        
        int row = index.Item1;
        int  col = index.Item2;

        Constants.PieceType pieceType = piece.Data.pieceType;
        
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
                    tempDic[(newRow, newCol)] = pieceType;
                }
                break;
            case Constants.PieceType.Bishop:
                List<(int, int)> bDiagonals = CalDiagonals(row, col, pieceType, piece, tempDic);
                break;
            case Constants.PieceType.Queen:
                List<(int, int)> qDiagonals = CalDiagonals(row, col, pieceType, piece, tempDic);
                List<(int, int)> qStraigths = CalStraights(row, col, pieceType, piece, tempDic);
                break;
            case Constants.PieceType.Rook:
                List<(int, int)> rookStraights = CalStraights(row, col, pieceType, piece, tempDic);
                break;
            case Constants.PieceType.Pawn:
                // 전진
                int newPawnCol = pieceColor == Constants.PlayerColor.White ? col + 1 : col - 1;
                int firstRank = pieceColor == Constants.PlayerColor.White ? 1 : Constants.BOARD_SIZE - 2;
                
                if (newPawnCol < Constants.BOARD_SIZE && newPawnCol >= 0)
                {
                    AddIfPossible(piece, (row, newPawnCol), tempDic);
                }
                if (col == firstRank)
                {
                    newPawnCol = pieceColor == Constants.PlayerColor.White ? col + 2 : col - 2;
                    AddIfPossible(piece, (row, newPawnCol), tempDic);
                }
                
                // 대각선 기물먹기
                List<(int, int)> capturables = new List<(int, int)>();
                int newPawnRow;
                newPawnCol = pieceColor == Constants.PlayerColor.White ? col + 1 : col - 1;
                
                if (row + 1 < Constants.BOARD_SIZE && row + 1 >= 0)
                {
                    newPawnRow = row + 1;
                    capturables.Add((newPawnRow, newPawnCol));
                }
                if (row - 1 < Constants.BOARD_SIZE && row - 1 >= 0)
                {
                    newPawnRow = row - 1;
                    capturables.Add((newPawnRow, newPawnCol));
                }

                foreach (var capturable in capturables)
                {
                    Piece capturablePiece = _blocks[capturable].PieceInBlock;
                    if (capturablePiece != null)
                    {
                        if (capturablePiece.Data.pieceColor != pieceColor)
                        {
                            AddIfPossible(piece, capturable, tempDic);
                        }
                    }
                }
                // TODO:앙파상 추가
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
                        AddIfPossible(piece, (row + i, col + j), tempDic);
                    }
                }
                break;
        }
        moveableBlocks.AddRange(FinalDic(tempDic));
        return moveableBlocks;
    }

    List<(int, int)> CalDiagonals(int row, int col, Constants.PieceType pieceType, Piece piece, Dictionary<(int, int), Constants.PieceType> tempDic)
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
                AddIfPossible(piece, (i, checkCol), tempDic);
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
                AddIfPossible(piece,  (i, checkCol), tempDic);
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
                AddIfPossible(piece, (i, checkCol), tempDic);
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
                AddIfPossible(piece, (i, checkCol), tempDic);
                continue;
            }

            break;
        }
        return diagonals;
    }

    List<(int, int)> CalStraights(int row, int col, Constants.PieceType pieceType, Piece piece, Dictionary<(int, int), Constants.PieceType> tempDic)
    {
        List<(int, int)> straights = new List<(int, int)>();
        for (int i = row - 1; i >= 0; i--)
        {
            AddIfPossible(piece, (i, col), tempDic);
        }

        for (int i = row + 1; i < Constants.BOARD_SIZE; i++)
        {
            AddIfPossible(piece, (i, col), tempDic);
        }

        for (int j = col - 1; j >= 0; j--)
        {
            AddIfPossible(piece, (row, j), tempDic);
        }

        for (int j = col + 1; j < Constants.BOARD_SIZE; j++)
        {
            AddIfPossible(piece, (row, j), tempDic);
        }
        return straights;
    }

    public Dictionary<(int, int), Piece> FinalDic(Dictionary<(int, int), Constants.PieceType> tempDic)
    {
        
        Block block;
        Dictionary<(int, int), Piece> finalDic = new Dictionary<(int, int), Piece>();
        
        if (_pieceColor == Constants.PlayerColor.None || _pieceColor == null)
        {
            Debug.LogError("MoveChecker: _pieceColor is null or None");
            return null;
        }
        foreach (var pair in tempDic)
        {
            block = _boardController.Blocks[pair.Key];
            if (block == null)
                Debug.LogError($"MoveChecker: block is null(can't find block with {pair.Value} pieceType and {_pieceColor} color on board)");
            finalDic[pair.Key] = block.PieceInBlock;
        }

        return finalDic;
    }

    public Dictionary<(int, int), Block> GenerateFutureDic((int, int) index, Piece piece)
    {
        Dictionary<(int, int), Block> futureBlockDic = CloneBlockDic(_blocks);
        futureBlockDic[piece.Index].PieceInBlock = null;
        futureBlockDic[index].PieceInBlock = piece;
        return futureBlockDic;
    }
    
    public Dictionary<(int, int), Block> CloneBlockDic(Dictionary<(int, int), Block> originalBlocks)
    {
        Dictionary<(int, int), Block> newBlockDic = new Dictionary<(int, int), Block>();
        foreach (var block in originalBlocks)
        {
            newBlockDic.Add(block.Key, block.Value.Clone(GameManager.Instance.gameObject));
        }

        return newBlockDic;
    }

    private void AddIfPossible(Piece piece, (int, int) index, Dictionary<(int, int), Constants.PieceType> tempDic)
    {
        if (!_isValidating)
        {
            try
            {
                _isValidating = true;
                if (!GameManager.Instance.RuleChecker.IsCheck(piece.Data.pieceColor,
                        GenerateFutureDic(index, piece)))
                {
                    tempDic[index] = piece.Data.pieceType;
                    if (_blocks[index].PieceInBlock != null)
                    {
                        return;
                    }
                }
            }
            finally
            {
                _isValidating = false;
            }
        }
    }
}