using System.Collections.Generic;
using UnityEngine;

public class RuleChecker
{
    private BoardController _boardController;
    public RuleChecker(BoardController boardController)
    {
        _boardController = boardController;
    }
    
    // 체크메이트, 스테일메이트, 합의에 의한 무승부, 시간승, 기물부족/시간부족 등등에 의한 무승부, 기권승

    // attackColor가 체크를 하고 있을 경우, reciever의 가능한 모든 move들이 recieverAllMoves일 때
    // 체크 여부(bool)와 이동제한 기물 목록을 반환
    public bool IsCheck(Constants.PlayerColor attackerColor, Dictionary<(int,int), Block> futureBlockDic = null)
    {
        Debug.Log($"IsCheck + {attackerColor}, {futureBlockDic}");
        Dictionary<(int, int), Piece> attackerAllMoves = new Dictionary<(int, int), Piece>();
        attackerAllMoves = GetAllMoveables(attackerColor, true);
        var recieverAllMoves = GetAllMoveables(OppositePlayerColor(attackerColor), true);
        Dictionary<(int, int), Piece> allBlockedMoves = new Dictionary<(int, int), Piece>();
        Dictionary<(int, int), Block> blockDic = _boardController.Blocks;
        if (futureBlockDic != null)
        {
            blockDic = futureBlockDic;
        }
        
        // attacker가 이동할 수 있는 곳이 아무 곳도 없으면 false 반환(갇힌 경우)
        if (attackerAllMoves == null)
            return false;
        
        // attacker의 allMoveables 중 체크가 있는지 확인 후 있으면 true 반환
        foreach (var movePair in attackerAllMoves)
        {
            Debug.Log($"attackerAllMoves 중 {movePair.Value} 확인");
            Piece pieceInBlock = blockDic[movePair.Key].PieceInBlock;
            
            
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
    public bool IsMate(Constants.PlayerColor color)
    {
        Debug.Log("<color=red>IsMate에서 GetAllMoveables 실행됨1</color>");
        // Dictionary<(int, int), Piece> opponentAllMoves = GetAllMoveables(OppositePlayerColor(color), false);
        Debug.Log("<color=red>IsCheck에서 GetAllMoveables 실행됨2</color>");
        // Dictionary<(int, int), Piece> myAllMoveables = GetAllMoveables(color, false);

        Debug.Log("IsMate");
        if (IsCheck(color))
            return true;
        return false;
    }
    
    // 모든 기물의 moveables를 반환함 => 추후 같은 블록에 겹치는 것 처리 문제 해결하기
    public Dictionary<(int, int), Piece> GetAllMoveables(Constants.PlayerColor playerColor, bool isVisualizing, Dictionary<(int, int), Block> baseVirtualBlockDic = null)
    {
        Dictionary<(int, int), Piece> allMoveables = new Dictionary<(int, int), Piece>();
        Dictionary<(int, int), Block> blocks = _boardController.Blocks;
        
        // temp
        string tempStr = "";

        if (baseVirtualBlockDic != null)
        {
            Debug.Log("virtualdic 사용함");
            blocks = baseVirtualBlockDic;
        }
            
        if (blocks == null || blocks.Count == 0)
            Debug.Log("blocks가 비어있음");
        foreach (var blockPair in blocks)
        {
            if (blockPair.Value == null)
            {
                Debug.Log("blockPair.Value is null");
            }
            Piece pieceInBlock = blockPair.Value.PieceInBlock;
            if (pieceInBlock == null)
                continue;
            if (pieceInBlock.Data.pieceColor == playerColor)
            {
                foreach (var moveable in GameManager.Instance.MoveChecker.MoveableBlocks(pieceInBlock, playerColor,
                             pieceInBlock.Index, isVisualizing))
                {
                    
                    // Debug.Log($"{pieceInBlock} 피스의 블록 GetAllMove");
                    tempStr += $", {pieceInBlock} pieceInBlock의 {moveable.Key} 인덱스 {moveable.Value}";
                    
                    // 반환할 리스트에 해당 기물의 moveables 추가
                    if (!allMoveables.ContainsKey(moveable.Key))
                        allMoveables[moveable.Key] = moveable.Value;
                }
                
            }
        }
        Debug.Log(tempStr);
        return allMoveables;
    }
    
    // index에 있는 기물이 piece로 먹을 수 있는 기물인지
    public bool IsCapturable((int, int) index, Piece piece)
    {
        Piece goalPiece = _boardController.Blocks[index].PieceInBlock;
        if (piece.Data.pieceColor != goalPiece.Data.pieceColor)
        {
            // pawn일 때 (직선캡쳐방지)
            if (piece.Data.pieceType == Constants.PieceType.Pawn)
            {
                // 만약 row가 같으면
                if (goalPiece.Index.Item1 == piece.Index.Item1)
                    return false;
            }
                
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public bool IsMoveable((int, int) index, Piece piece, bool isVisualizing)
    {
        Debug.Log($"<color=red>IsMoveable에서 {piece} 피스 체크시작</color>");
        Dictionary<(int, int), Piece> moveableBlocks =
            GameManager.Instance.MoveChecker.MoveableBlocks(piece, piece.Data.pieceColor, piece.Index, isVisualizing);
        // temp
        string debugStr = $"<color=red>클릭된 인덱스는 {index}, IsMoveable에서 MoveableBlocks로 확인한 이동가능 곳: </color>";
        foreach (var block in moveableBlocks)
        {
            debugStr += $"{block.Key} 인덱스의 {block.Value} 피스, ";
        }
        Debug.Log(debugStr);
        // 이동 가능한 곳이면
        if (moveableBlocks.ContainsKey(index))
        {
            Debug.Log("<color=red>이동 가능한 곳임</color>");
            // 해당 블록에 piece가 있으면
            if (_boardController.Blocks[index].PieceInBlock != null)
            {
                Debug.Log("<color=red>해당 블록에 piece가 있음</color>");
                // 먹을 수 있는 piece이면
                if (IsCapturable(index, piece))
                {
                    Debug.Log("<color=red>해당 피스를 먹을 수 있음</color>");
                    // _boardController.Blocks[index].Clear(false);
                    // Debug.Log($"{piece}가 먹음!");
                    return true;
                }
            }
            // 해당 블록이 비어있으면
            else
            {
                Debug.Log("<color=red>해당 블록이 비어 있어서 이동 가능함.</color>");
                return true;
            }
        }

        Debug.Log("<color=red>MoveableBlocks로 계산된 이동 가능한 곳이 아님</color>");
        return false;
    }
    
    public Constants.PlayerColor OppositePlayerColor(Constants.PlayerColor playerColor)
    {
        if (playerColor == Constants.PlayerColor.None)
        {
            return  Constants.PlayerColor.None;
        }
        return playerColor ==  Constants.PlayerColor.White ? Constants.PlayerColor.Black : Constants.PlayerColor.White;
    }
}