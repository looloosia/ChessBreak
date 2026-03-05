using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Block : MonoBehaviour
{
    private GameObject _tempObj;
    private GameObject _moveableObj;
    public GameObject MoveableObj => _moveableObj;
    private Vector3 _moveableObjScale;
    // 자식 chessPiece
    private Piece _pieceInBlock;
    private GameObject _moveablePrefab;

    public Piece PieceInBlock
    {
        get => _pieceInBlock;
        set => _pieceInBlock = value;
    }
    public delegate void OnBlockClicked(Piece piece, Block block, (int row, int col) index);
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
        Debug.Log($"<color=yellow>_pieceInBlock is {_pieceInBlock}</color>");
        _onBlockClicked?.Invoke(_pieceInBlock, this, (_row, _col));
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
        _moveablePrefab = GameManager.Instance.MoveablePrefab;
        _moveableObjScale = GameManager.Instance.MoveableScale;
        SetPiece(null);

        // 클릭 콜백 설정
        _onBlockClicked = onBlockClicked;
    }

    public void SetPiece(Piece piece)
    {
        Debug.Log("<color=yellow>SetPiece()</color>");
        if (piece == null)
        {
            Debug.Log("Piece in block is null이어서 블록 기물할당 못함");
            _pieceInBlock = null;
            return;
        }
        piece.Index = (_row, _col);
        // 블록에 기물할당
        piece.transform.position = transform.position;
        _pieceInBlock = piece;
        if (_tempObj == null)
        {
            _tempObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _tempObj.GetComponent<Renderer>().material.color = Color.red;
            _tempObj.layer = 2;
        }
        _tempObj.transform.position = transform.position;
    }

    public void MakePiece(PieceData data)
    {
        if (_pieceInBlock == null)
        {
            GameObject pieceObj;
            if (_tempObj == null)
            {
                _tempObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _tempObj.transform.position = transform.position;
                _tempObj.GetComponent<Renderer>().material.color = Color.red;
                _tempObj.layer = 2;
            }
            else
            {
                Debug.Log("tempobj가 null이 아님");
            }
            pieceObj = Instantiate(data.prefab);
            pieceObj.transform.position = transform.position;
            pieceObj.transform.localScale = Vector3.one;
            _pieceInBlock = pieceObj.GetComponent<Piece>();
            _pieceInBlock.Index = (_row, _col);
        }
    }

    public void SetMovebale()
    {
        _moveableObj = Instantiate(_moveablePrefab);
        _moveableObj.transform.position = transform.position;
        _moveableObj.transform.localScale = _moveableObjScale;
        // TODO: 먹을 수 있는 기물도 표시하기
    }

    public void Clear(bool onlyMovable)
    {
        // Debug.Log($"<color=red>Clear: onlyMoveable: {onlyMovable} <- 잡을 때는 false여야 함</color>");
        if (_moveableObj != null)
        {
            Destroy(_moveableObj);
        }
        if (onlyMovable)
        {
            return;
        }
        Debug.Log("Block: Clear()에서 onlyMoveables가 flase여서 _pieceInBlock도 제거함");
        _pieceInBlock = null;
        if (_tempObj != null)
        {
            Destroy(_tempObj);
        }
    }

    public void RemovePiece()
    {
        if (_pieceInBlock != null)
        {
            Destroy(_pieceInBlock.gameObject);
            _pieceInBlock = null;
        }
    }
}
