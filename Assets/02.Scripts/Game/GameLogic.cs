using System;
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
    
    // 게임의 플레이어 타입
    private Constants.PlayerType _gamePlayerType;
    public Constants.PlayerType GamePlayerType
    {
        get { return _gamePlayerType; }
    }
    
    // 게임의 종류
    private Constants.GameType _gameType;
    public Constants.GameType GameType => _gameType;
    
    // 게임 플레이어의 승패 여부
    Constants.GameResult _gameResult;
    public Constants.GameResult GameResult
    {
        get { return _gameResult; }
    }

    // 보드의 상태
    private Piece[,] _board;
    public Piece[,] Board => _board;
    
    private BoardController _boardController;
    public BoardController BoardController => _boardController;
    #endregion
    
    public GameLogic(Constants.GameType gameType, BoardController boardController)
    {
        _gameType = gameType;
        _boardController = boardController;
        _board = new Piece[Constants.BOARD_SIZE, Constants.BOARD_SIZE];
        InitStates();
    }

    public void SetState(BaseState newState)
    {
        _currentState?.OnExit(this);
        _currentState = newState;
        _currentState.OnEnter(this);
    }

    public bool PlacePiece((int, int) index, Piece piece)
    {
        //piece를 해당 index에 추가.
        if (_board[index.Item1, index.Item2] != null)
            return false;
        _boardController.Blocks[index].SetPiece(piece);
        _board[index.Item1, index.Item2] = piece;
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

    public void Dispose()
    {
        
    }
}
