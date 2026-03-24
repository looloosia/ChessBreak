
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class PlayerState : BaseState
{
    // 멀티 플레이 관련 변수
    private bool _isMultiplayer;
    private MultiPlayManager _multiplayManager;
    private string _multiplayRoomId;
    
    private Piece _firstClickedPiece;
    private (int, int) _firstClickedIndex;
    private Piece _secondClickedPiece;
    private (int, int) _secondClickedIndex;
    private bool _isTurnable = false;
    Dictionary<(int, int), Piece> _moveableBlocks = new Dictionary<(int, int), Piece>();

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
        _ruleChecker = GameManager.Instance.RuleChecker;
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
        
        // 이동 가능한 곳을 클릭했을 때
        if (_ruleChecker.IsMoveable(index, _firstClickedPiece, false))
        {
            // firstClicked Block의 piece 제거하기
            gameLogic.BoardController.Blocks[_firstClickedIndex].Clear(false);
            gameLogic.BoardController.Blocks[_secondClickedIndex].RemovePiece();
            ProcessMove(gameLogic, index, _firstClickedPiece);
        }
        else
        {
            Debug.Log("불가능한 수입니다.");
        }
        gameLogic.BoardController.ClearBoard(true);
        _firstClickedPiece = null;
        _secondClickedPiece = null;
        _secondClickedIndex = (-1, -1);
        _moveableBlocks.Clear();
        // TODO: 멀티플레이 관련
        // 멀티 플레이인 경우, 상대방에게도 이동 정보 전송
        // if (_isMultiplayer)
        // {
        //     _multiplayManager.SendPlayerMove(_multiplayRoomId, index);
        // }
    }

    public override void OnExit(GameLogic gameLogic)
    {
        // gameLogic.blockController.onBlockClicked = null;
    }
    /// <summary>
    /// 아무 블록을 클릭했을 때 실행됨
    /// </summary>
    /// <param name="piece"></param>
    /// <param name="block"></param>
    /// <param name="blockIndex">클릭한 블록의 인덱스</param>
    void OnBlockClicked(Piece piece, Block block, (int, int) blockIndex)
    {
        string strPiece = piece == null ? "null" : piece.ToString();
        
        List<(int, int)> willRemove = new List<(int, int)>();
        
        // 이게 첫번째 클릭일 때
        if (_firstClickedPiece == null)
        {
            Debug.Log("이게 첫 번째 클릭임");
            _isTurnable = false;
            
            // 첫번째 클릭을 빈칸을 했을 때 그냥 반환
            if (piece == null)
                return;
            
            // 현재 턴 색상이 아닌 기물 클릭했을 때 그냥 반환
            if (piece.Data.pieceColor != _playerType)
                return;
            
            _firstClickedPiece = piece;
            _firstClickedIndex = blockIndex;
        }
        // 이게 두번째 클릭일 때
        else if (_secondClickedPiece == null && _firstClickedPiece != piece)
        {
            Debug.Log("<color=red>이게 두번째 클릭임</color>");
            // 기물을 먹을 수 있으면
            if (piece != null)
            {
                if (_ruleChecker.IsCapturable(blockIndex, _firstClickedPiece))
                {
                    _secondClickedPiece = piece;
                    _secondClickedIndex = blockIndex;
                    
                }
                // 기물을 먹을 수 없으면
                else
                {
                    _firstClickedPiece = piece;
                    _firstClickedIndex = blockIndex;
                    willRemove.Add(blockIndex);
                    _gameLogic.BoardController.ClearBoard(true);
                }
            }
            _isTurnable = true;
        }
        // 첫번째로 기물 클릭하고 두번째로 빈칸 클릭했을 때
        else if (_secondClickedPiece == null)
        {
            _secondClickedPiece = piece;
            _secondClickedIndex = blockIndex;
            
            _isTurnable = true;
        }
        // 한 기물을 두 번 클릭했을 때
        else if (_firstClickedPiece == piece)
        {
            Debug.Log("한 기물을 두 번 클릭함");
            _secondClickedPiece = null;
            _firstClickedPiece = piece;

            _isTurnable = false;
        }
        else
        {
            Debug.Log("firstClickedPiece, secondClickedPiece 모두 null이 아님");
        }
        
        if (_firstClickedPiece == null)
        {
            Debug.LogError("firstClickedPiece == null");
            return;
        }
        
        Debug.Log($"OnBlockClicked에서 {_firstClickedPiece} 피스 확인함");
        bool isVisualizing = !_isTurnable;
        Debug.Log($"OnBlockClicked에서 isVisualizing is  {isVisualizing}");
        _moveableBlocks =
            ClonePieceDic(
                GameManager.Instance.MoveChecker.MoveableBlocks(_firstClickedPiece, _playerType, _firstClickedIndex, isVisualizing));
            
        foreach (var moveable in _moveableBlocks.Keys)
        {
            foreach (var remove in willRemove)
            {
                if (remove == moveable)
                {
                    _moveableBlocks.Remove(moveable);
                    willRemove.Remove(remove);
                }
            }
        }
        if (_isTurnable)
        {
            HandleMove(_gameLogic, blockIndex);
        }
        else
        {
            Debug.Log("visualize 해야 함");
            _gameLogic.VisualizeMoveables(piece, blockIndex, _moveableBlocks.Keys.ToList());
        }
    }
    
    public Dictionary<(int, int), Piece> ClonePieceDic(Dictionary<(int, int), Piece> originalPieceDic)
    {
        Dictionary<(int, int), Piece> newPieceDic = new Dictionary<(int, int), Piece>();
        foreach (var pair in originalPieceDic)
        {
            if (pair.Value == null)
            {
                newPieceDic.Add(pair.Key, null);
                continue;
            }
            newPieceDic.Add(pair.Key, pair.Value.Clone(GameManager.Instance.gameObject));
        }

        return newPieceDic;
    }
}
