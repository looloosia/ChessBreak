using UnityEngine;

public abstract class BaseState
{
    protected Constants.PlayerColor _playerType;
    public Constants.PlayerColor PlayerType => _playerType;
    protected GameLogic _gameLogic;
    public abstract void OnEnter(GameLogic gameLogic);                      // 상태 진입 시 호출
    public abstract void HandleMove(GameLogic gameLogic, (int,int) index);        // 플레이어 이동 처리
    public abstract void OnExit(GameLogic gameLogic);                       // 상태 종료 시 호출
    public abstract void HandleNextTurn(GameLogic gameLogic);               // 다음 턴 처리

    // 이동이 확정되어야만 실행됨
    public void ProcessMove(GameLogic gameLogic, (int,int) index, Piece piece)
    {
        // secondClickedBlock으로 firstClickedPiece 옮기기
        if (gameLogic.PlacePiece(index, piece))
        {
            // 게임 승패 확인
            var gameResult = gameLogic.CheckGameResult(piece.Data.pieceColor);
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
