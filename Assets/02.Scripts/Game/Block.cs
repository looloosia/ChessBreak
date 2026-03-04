using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Block : MonoBehaviour
{
    // 자식 chessPiece
    private Piece _pieceInBlock;
    public delegate void OnBlockClicked(Piece piece, (int row, int col) index);
    private OnBlockClicked _onBlockClicked;
    
    private int _row;
    public int Row
    {
        get => _row;
        set => _row = value;
    }

    private int _col;

    public int Col
    {
        get => _col;
        set => _col = value;
    }

    // 클릭되면 chesspiece, 좌표 반환
    void OnMouseUpAsButton()
    {
        // if (EventSystem.current.IsPointerOverGameObject())
        // {
        //     return;
        // }
        _onBlockClicked?.Invoke(_pieceInBlock, (_row, _col));
        
    }

    void OnMouseDown()
    {
        // piece의 opacity나 색상 변경
    }
    
    // 블록 초기화
    public void InitBlock((int,int) blockIndex, OnBlockClicked onBlockClicked)
    {
        _row = blockIndex.Item1;
        _col = blockIndex.Item2;
        SetPiece(null);

        // 클릭 콜백 설정
        _onBlockClicked = onBlockClicked;
    }

    public void SetPiece(Piece piece)
    {
        if (piece == null)
        {
            _pieceInBlock = null;
            return;
        }
        // 블록에 기물할당
        piece.transform.SetParent(transform);
        piece.transform.localPosition = Vector3.zero;
    }
}
