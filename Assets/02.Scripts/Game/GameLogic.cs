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
    private event Action<Dictionary<(int, int), Piece>> OnCheck;

    private event Action OnCheckmate;

    #endregion
    
    public GameLogic(Constants.GameType gameType, BoardController boardController)
    {
        _gameType = gameType;
        _boardController = boardController;
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

        Dictionary<(int, int), Piece> myAllMoveables = GetAllMoveables(_currentState.PlayerType);
        
        // 해당 칸으로 옮기면 체크일 때 _onCheck Invoke
        Debug.Log("_currentState type : " + _currentState.PlayerType);
        if (IsCheck(_currentState.PlayerType))
        {
            OnCheck?.Invoke(myAllMoveables);
        }
        return true;
    }

    // index에 있는 기물이 piece로 먹을 수 있는 기물인지
    public bool IsCapturable((int, int) index, Piece piece)
    {
        Piece goalPiece = BoardController.Blocks[index].PieceInBlock;
        if (piece.Data.pieceColor != goalPiece.Data.pieceColor)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    // 모든 기물의 moveables를 반환함 => 추후 같은 블록에 겹치는 것 처리 문제 해결하기
    public Dictionary<(int, int), Piece> GetAllMoveables(Constants.PlayerColor playerColor)
    {
        Dictionary<(int, int), Piece> allMoveables = new Dictionary<(int, int), Piece>();
        foreach (var blockPair in BoardController.Blocks)
        {
            Piece pieceInBlock = blockPair.Value.PieceInBlock;
            if (pieceInBlock == null)
                continue;
            
            // playerColor 색의 기물일 경우에
            if (pieceInBlock.Data.pieceColor == playerColor)
            {
                foreach (var moveable in GameManager.Instance.MoveChecker.MoveableBlocks(pieceInBlock.Data.pieceType, _currentState.PlayerType,
                             pieceInBlock.Index))
                {
                    // 반환할 리스트에 해당 기물의 moveables 추가
                    if (!allMoveables.ContainsKey(moveable.Key))
                        allMoveables[moveable.Key] = moveable.Value;
                }
            }
        }
        return allMoveables;
    }

    public bool IsMoveable((int, int) index, Piece piece)
    {
        Dictionary<(int, int), Piece> moveableBlocks =
            GameManager.Instance.MoveChecker.MoveableBlocks(piece.Data.pieceType, piece.Data.pieceColor, piece.Index);
        
        // 이동 가능한 곳이면
        if (moveableBlocks.ContainsKey(index))
        {
            // 해당 블록에 piece가 있으면
            if (BoardController.Blocks[index].PieceInBlock != null)
            {
                // 먹을 수 있는 piece이면
                if (IsCapturable(index, piece))
                {
                    // _boardController.Blocks[index].Clear(false);
                    // Debug.Log($"{piece}가 먹음!");
                    return true;
                }
            }
            // 해당 블록이 비어있으면
            else
            {
                return true;
            }
        }

        return false;
    }
    
    // attackColor가 체크를 하고 있을 경우 attackColor의 모든 기물이동가능성 반환
    private bool IsCheck(Constants.PlayerColor attackerColor, Dictionary<(int,int), Piece> recieverAllMoves = null)
    {
        Debug.Log("<color=yellow>IsCheck() 실행됨</color>");
        Dictionary<(int, int), Piece> attackerAllMoves = new Dictionary<(int, int), Piece>();
        attackerAllMoves = GetAllMoveables(attackerColor);
        
        // attacker가 이동할 수 있는 곳이 아무 곳도 없으면 false 반환(갇힌 경우)
        if (attackerAllMoves == null)
            return false;
        
        // attacker의 allMoveables 중 체크가 있는지 확인 후 있으면 true 반환
        foreach (var movePair in attackerAllMoves)
        {
            Piece pieceInBlock = BoardController.Blocks[movePair.Key].PieceInBlock;
            
            // movePair로 갈 수 있는 reciever의 기물이 있을 경우
            if (recieverAllMoves != null)
            {
                Piece moveablePiece = null;
                if (recieverAllMoves.ContainsKey(movePair.Key))
                {
                    moveablePiece = recieverAllMoves[movePair.Key];
                }
                if (moveablePiece != null)
                {
                    pieceInBlock = moveablePiece;
                }
            }
            
            // moveable한 칸에 기물이 있을 경우
            if (pieceInBlock != null)
            {
                PieceData data = pieceInBlock.Data;
                // 해당 기물이 attacker 색의 반대 색깔 킹이면
                if (data.pieceType == Constants.PieceType.King && data.pieceColor != attackerColor)
                {
                    return true;
                }
            }
        }

        return false;
    }

    // 체크된 경우에만 실행됨. color가 메이트 했는지 판정
    private bool IsMate(Constants.PlayerColor color)
    {
        Dictionary<(int, int), Piece> opponentAllMoves = GetAllMoveables(OppositePlayerColor(color));
        Dictionary<(int, int), Piece> myAllMoveables = GetAllMoveables(color);

        if (IsCheck(color, opponentAllMoves))
            return true;
        return false;
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
        if (IsMate(_currentState.PlayerType))
        {
            OnCheckmate?.Invoke();
        }
    }

    void Checkmate()
    {
        Debug.Log("<color=red>Checkmate</color>");
        //EndGame() 하기
    }

    public Constants.PlayerColor OppositePlayerColor(Constants.PlayerColor playerColor)
    {
        if (playerColor == Constants.PlayerColor.None)
        {
            return  Constants.PlayerColor.None;
        }
        return playerColor ==  Constants.PlayerColor.White ? Constants.PlayerColor.Black : Constants.PlayerColor.White;
    }

    public void Dispose()
    {
        OnCheck -= Check;
        OnCheckmate -= Checkmate;
    }
    
}
