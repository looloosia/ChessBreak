using UnityEngine;

public abstract class BaseState
{
    protected GameLogic _gameLogic;
    public abstract void OnEnter(GameLogic gameLogic);                      // 상태 진입 시 호출
    public abstract void HandleMove(GameLogic gameLogic, (int,int) index);        // 플레이어 이동 처리
    public abstract void OnExit(GameLogic gameLogic);                       // 상태 종료 시 호출
    public abstract void HandleNextTurn(GameLogic gameLogic);               // 다음 턴 처리

    public void ProcessMove(GameLogic gameLogic, (int,int) index, Constants.PlayerColor playerType)
    {
        // 특정 위치에 마커 표시
        if (gameLogic.PlacePiece(index, playerType))
        {
            // 게임 승패 확인
            var gameResult = gameLogic.CheckGameResult(playerType);
            if (gameResult == Constants.GameResult.None)
            {
                // 턴 전환
                HandleNextTurn(gameLogic);
                Debug.Log("턴 전환");
            }
            else
            {
                // 게임오버 처리
                gameLogic.EndGame(gameResult);
                Debug.Log("게임오버");
            }
        }
    }
}
