using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class GameLogic : IDisposable
{
    #region Fields & Properties
    
    // 게임의 플레이어 state
    private BaseState _gamePlayer;
    public BaseState GamePlayer
    {
        get { return _gamePlayer; }
    }

    private BaseState playerAState;
    private BaseState playerBState;
    public BaseState PlayerAState => playerAState;
    public BaseState PlayerBState => playerBState;

    // 현재 턴의 플레이어 state
    private BaseState _currentState;
    public BaseState CurrentState => _currentState;
    
    // 멀티 플레이를 처리하는 매니저
    private MultiPlayManager _multiplayManager;
    private string _multiplayRoomId;
    
    // 게임의 플레이어 타입(인간, AI)
    private Constants.PlayerType _gamePlayerType;
    public Constants.PlayerType GamePlayerType
    {
        get { return _gamePlayerType; }
    }
    
    // 게임의 종류(싱글, 로컬듀얼, 멀티)
    private Constants.GameType _gameType;
    public Constants.GameType GameType => _gameType;
    
    // 게임 플레이어의 승패 여부
    Constants.GameResult _gameResult;
    public Constants.GameResult GameResult
    {
        get { return _gameResult; }
    }
    
    private BoardController _boardController;
    public BoardController BoardController => _boardController;
    
    private RuleChecker _ruleChecker;
    private event Action<Dictionary<(int, int), Piece>> OnCheck;

    private event Action OnCheckmate;

    #endregion
    
    public GameLogic(Constants.GameType gameType, BoardController boardController, RuleChecker ruleChecker)
    {
        _gameType = gameType;
        _boardController = boardController;
        _ruleChecker = ruleChecker;
        InitStates();
        OnCheck += Check;
        OnCheckmate += Checkmate;
    }
    

    public void SetState(BaseState newState)
    {
        _currentState?.OnExit(this);
        _currentState = newState;
        _currentState.OnEnter(this);
    }

    // index로 piece 이동 (이동이 확정되어야만 실행됨)
    public bool PlacePiece((int, int) index, Piece piece)
    {
        if (piece == null)
            Debug.LogError("piece == null");
        _boardController.Blocks[index].Clear(false);
        _boardController.Blocks[index].SetPiece(piece);

        Debug.Log("<color=red>PlacePiece에서 GetAllMoveables 실행</color>");
        Dictionary<(int, int), Piece> myAllMoveables = _ruleChecker.GetAllMoveables(_currentState.PlayerType, false);
        
        // 해당 칸으로 옮기면 체크일 때 _onCheck Invoke
        Debug.Log("_currentState type : " + _currentState.PlayerType);
        
        // TODO: 여기서 futuredic 안 넣는 게 맞는지 확인
        if (_ruleChecker.IsCheck(_currentState.PlayerType))
        {
            OnCheck?.Invoke(myAllMoveables);
        }
        return true;
    }
    
    void InitStates()
    {
        switch (_gameType)
        {
            case Constants.GameType.SinglePlay:
                // 싱글 플레이어 모드 초기화 작업
                playerAState = new PlayerState(true);
                // playerBState = new AIState(false);

                // 초기 상태 설정 (예: 플레이어 A부터 시작)
                SetState(playerAState);
                break;
            case Constants.GameType.LocalDualPlay:
                // 듀얼 플레이어 모드 초기화 작업
                playerAState = new PlayerState(true);
                playerBState = new PlayerState(false);

                // 초기 상태 설정 (예: 플레이어 A부터 시작)
                SetState(playerAState);
                break;
            #region MultiPlay
            // case Constants.GameType.MultiPlay:
            //     // 멀티 플레이어 모드 초기화 작업
            //     _multiplayManager = new MultiplayManager((state, roomId) =>
            //     {
            //         _multiplayRoomId = roomId;
            //
            //         switch (state)
            //         {
            //             case MultiplayManagerState.CreateRoom:
            //                 // TODO: "상대방을 기다리고 있습니다." 팝업 표시
            //
            //                 Debug.Log("방 생성됨, 방 ID: " + _multiplayRoomId);
            //
            //                 break;
            //             case MultiplayManagerState.JoinRoom:
            //
            //                 Debug.Log("방 참가됨, 방 ID: " + _multiplayRoomId);
            //
            //                 playerAState = new MultiplayerState(true, _multiplayManager);
            //                 playerBState = new PlayerState(false, _multiplayManager, _multiplayRoomId);
            //                 SetState(playerAState);
            //                 break;
            //             case MultiplayManagerState.StartGame:
            //
            //                 Debug.Log("게임 시작됨, 방 ID: " + _multiplayRoomId);
            //
            //                 playerAState = new PlayerState(true, _multiplayManager, _multiplayRoomId);
            //                 playerBState = new MultiplayerState(false, _multiplayManager);
            //                 SetState(playerAState);
            //                 break;
            //             case MultiplayManagerState.ExitRoom:
            //                 // TODO: "본인이 방을 나갔습니다." 팝업 표시
            //
            //                 Debug.Log("본인이 방을 나감, 방 ID: " + _multiplayRoomId);
            //
            //                 break;
            //             case MultiplayManagerState.EndGame:
            //                 // TODO: "상대방이 접속을 끊었습니다." 팝업 표시
            //
            //                 Debug.Log("상대방이 접속 끊음, 방 ID: " + _multiplayRoomId);
            //
            //                 break;
            //         }
            //     }); 
                // break;
            #endregion
        }
    }
    
    // 턴 변경
    public void ChangeGameState()
    {
        if (_currentState == playerAState)
        {
            SetState(playerBState);
        }
        else
        {
            SetState(playerAState);
        }
    }
    
    // 게임 결과 확인
    public Constants.GameResult CheckGameResult(Constants.PlayerColor playerColor)
    {
        // 게임결과에 따라 반환
        return Constants.GameResult.None;
    }
    
    // 게임오버 처리
    public void EndGame(Constants.GameResult gameResult)
    {
        string resultStr = "";
        switch (gameResult)
        {
            case Constants.GameResult.Win:
                resultStr = "Player1 승리!";
                break;
            case Constants.GameResult.Lose:
                resultStr = "Player2 승리!";
                break;
            case Constants.GameResult.Draw:
                resultStr = "무승부";
                break;
        }

        UIManager.Instance.OpenConfirmPanel(resultStr/*, () =>
        {
            GameManager.Instance.ChangeToMainScene();
        }*/);
    }
    
    public void VisualizeMoveables(Piece piece, (int, int) blockIndex, List<(int, int)> moveableBlocks)
    {
        BoardController boardController = BoardController;
        
        foreach (var moveableBlock in moveableBlocks)
        {
            Block block = boardController.Blocks[moveableBlock];
            block.SetMovebale();
        }
    }

    void Check(Dictionary<(int, int), Piece> checkersAllMoveables)
    {
        Debug.Log("<color=red>Check</color>");
        if (_ruleChecker.IsMate(_currentState.PlayerType))
        {
            OnCheckmate?.Invoke();
        }
    }

    void Checkmate()
    {
        Debug.Log("<color=red>Checkmate</color>");
        //EndGame() 하기
    }

    

    public void Dispose()
    {
        OnCheck -= Check;
        OnCheckmate -= Checkmate;
    }
    
}
