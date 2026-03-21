using System;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    [SerializeField] private PieceData[] whitePieces;
    [SerializeField] private PieceData[] blackPieces;
    [SerializeField] private Vector3 _blockMinPos;
    [SerializeField] private Vector3 _blockScale;
    private float _blockXDistance;
    private float _blockZDistance;
    public Block.OnBlockClicked onBlockClicked;
    private Dictionary<(int, int), Block> _blocks = new Dictionary<(int, int), Block>();
    public Dictionary<(int, int), Block> Blocks => _blocks;

    public void InitBoard()
    {
        _blockXDistance = _blockScale.x;
        _blockZDistance = _blockScale.z;
        
        for (int z = 0; z < Constants.BOARD_SIZE; z++)
        {
            for (int x = 0; x < Constants.BOARD_SIZE; x++)
            {
                int localRow = z;
                int localCol = x;
                Vector3 spawnPos = new Vector3(_blockMinPos.x - localCol * _blockScale.x, _blockMinPos.y, _blockMinPos.z + localRow * _blockScale.z);
                GameObject newBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
                newBlock.transform.parent = this.transform;
                newBlock.GetComponent<BoxCollider>().isTrigger = true;
                Block blockScript = newBlock.AddComponent<Block>();
                blockScript.Row = localRow;
                blockScript.Col = localCol;
                newBlock.transform.localScale = _blockScale;
                newBlock.transform.localPosition = spawnPos;
                blockScript.InitBlock((localRow,localCol), (piece, block, pos) =>
                {
                    onBlockClicked?.Invoke(piece, block, (pos.Item1, pos.Item2));
                });
                _blocks[(localRow, localCol)] = blockScript;
                ArrangePiece(blockScript, localRow, localCol);
                newBlock.GetComponent<Renderer>().enabled = false;
                newBlock.name = $"Block_{localRow}_{localCol}";
            }
        }
    }

    void ArrangePiece(Block blockScript, int localRow, int localCol)
    {
        PieceData[] pieces;
        // 첫번째줄에 있을 경우 : 기물들 생성
        if (localCol == 0 || localCol == Constants.BOARD_SIZE - 1)
        {
            pieces = localCol == 0 ? whitePieces : blackPieces;
            switch (localRow)
            {
                case 0:
                    blockScript.MakePiece(pieces[0]);
                    break;
                case 1:
                    blockScript.MakePiece(pieces[1]);
                    break;
                case 2:
                    blockScript.MakePiece(pieces[2]);
                    break;
                case 3:
                    blockScript.MakePiece(pieces[3]);
                    break;
                case 4:
                    blockScript.MakePiece(pieces[4]);
                    break;
                case 5:
                    blockScript.MakePiece(pieces[2]);
                    break;
                case 6:
                    blockScript.MakePiece(pieces[1]);
                    break;
                case 7:
                    blockScript.MakePiece(pieces[0]);
                    break;
            }
        }
        
        // 두번째줄에 있을 경우 : 폰 생성
        if (localCol == 1 || localCol == Constants.BOARD_SIZE - 2)
        {
            pieces = localCol == 1 ? whitePieces : blackPieces;
            blockScript.MakePiece(pieces[5]);
        }
    }

    public void ClearBoard(bool onlyMoveables)
    {
        foreach (Block block in _blocks.Values)
        {
            block.Clear(onlyMoveables);
            if (!onlyMoveables)
                Destroy(block.PieceInBlock.gameObject);
        }
    }

    public void LoadStage(StageData stage)
    {
        ClearBoard(false);
        // TODO: piece Layout 커스텀으롤 만들게 되면 불러오기
        
        // 보드 초기화
        InitBoard();
    }

    public Block FindBlockWithTypeColor(Constants.PieceType pieceType, Constants.PlayerColor pieceColor)
    {
        foreach (Block block in _blocks.Values)
        {
            if (block.PieceInBlock == null)
            {
                continue;
            }

            if (block.PieceInBlock.Data.pieceType == pieceType && block.PieceInBlock.Data.pieceColor == pieceColor)
            {
                return block;
            }
        }

        return null;
    }
}
