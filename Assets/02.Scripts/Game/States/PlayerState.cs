
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PlayerState : BaseState
{
    private Constants.PlayerColor _playerType;

    // 멀티 플레이 관련 변수
    private bool _isMultiplayer;
    private MultiPlayManager _multiplayManager;
    private string _multiplayRoomId;
    
    private Piece _firstClickedPiece;
    private Piece _secondClickedPiece;
    private (int, int) _secondClickedIndex;
    private bool _isTurnable = false;

    public PlayerState(bool isFirstPlayer)
    {
        _playerType = isFirstPlayer ? Constants.PlayerColor.White : Constants.PlayerColor.Black;
        _isMultiplayer = false;
    }

    public PlayerState(bool isFirstPlayer, MultiPlayManager multiplayManager, string roomId)
    {
        _playerType = isFirstPlayer ? Constants.PlayerColor.White : Constants.PlayerColor.Black;
        _isMultiplayer = true;
        _multiplayManager = multiplayManager;
        _multiplayRoomId = roomId;
    }

    // 턴 변경
    public override void HandleNextTurn(GameLogic gameLogic)
    {
        gameLogic.ChangeGameState();
    }

    public override void OnEnter(GameLogic gameLogic)
    {
        Debug.Log("OnEnter");
        _gameLogic = gameLogic;
        // 상태 진입 시 로직 구현
        gameLogic.BoardController.onBlockClicked = OnBlockClicked;

        if (_isMultiplayer)
        {
            // TODO: UnityThread 필요
            // UnityThread.executeInUpdate(() =>
            // {
            //     // OX UI 업데이트
            //     GameManager.Instance.SetGameTurn(_playerType);
            // });
        }
        else
        {
            // OX UI 업데이트
            UIManager.Instance.SetGameTurn(_playerType);
        }
    }

    public override void HandleMove(GameLogic gameLogic, (int, int) index)
    {
        if (_firstClickedPiece == null)
        {
            Debug.LogError("PlayerState: _firstClickedPiece == null");
            return;
        }
        List<(int, int)> moveableBlocks =
            GameManager.Instance.MoveChecker.MoveableBlocks(_firstClickedPiece.Data.pieceType, _firstClickedPiece.Index.Item1, _firstClickedPiece.Index.Item2);
        // temp
        Debug.Log(_secondClickedIndex.ToString());
        foreach (var block in  moveableBlocks)
            Debug.Log($"<color=yellow>{block.ToString()}</color>");
        
        if (moveableBlocks.Contains(_secondClickedIndex))
        {
            // firstClicked Block의 piece 제거하기
            _gameLogic.BoardController.Blocks[_secondClickedIndex].Clear();
            ProcessMove(gameLogic, index, _firstClickedPiece);
        }
        else
        {
            Debug.Log("불가능한 수입니다.");
        }
        _firstClickedPiece = null;
        _secondClickedPiece = null;
        _secondClickedIndex = (-1, -1);
        // TODO: 멀티플레이 관련
        // 멀티 플레이인 경우, 상대방에게도 이동 정보 전송
        // if (_isMultiplayer)
        // {
        //     _multiplayManager.SendPlayerMove(_multiplayRoomId, index);
        // }
    }

    public override void OnExit(GameLogic gameLogic)
    {
        Debug.Log("OnExit");
        // gameLogic.blockController.onBlockClicked = null;
    }
    
    void OnBlockClicked(Piece piece, (int, int) blockIndex)
    {
        string strPiece = piece == null ? "null" : piece.ToString();
        Debug.Log($"OnBlockClicked. piece in the block is {strPiece}");
        if (_firstClickedPiece == null)
        {
            _firstClickedPiece = piece;
            _isTurnable = false;
        }
        else if (_secondClickedPiece == null)
        {
            Debug.Log("second piece is being filled");
            _secondClickedPiece = piece;
            _secondClickedIndex = blockIndex;
            _isTurnable = true;
        }
        else
        {
            Debug.Log("firstClickedPiece, secondClickedPiece 모두 null이 아님");
        }
        Debug.Log($"fistPiece: {_firstClickedPiece}, secondPiece: {_secondClickedPiece}, isTurnable: {_isTurnable}");
        if (_isTurnable)
            HandleMove(_gameLogic, blockIndex);
    }
}
