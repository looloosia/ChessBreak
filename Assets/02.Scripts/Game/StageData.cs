using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct PieceSpawnInfo
{
    public PieceData pieceData;
    public int row;
    public int col;
}
[CreateAssetMenu(fileName = "StageData", menuName = "Scriptable Objects/StageData")]
public class StageData : ScriptableObject
{
    public int stageNumber;
    public string stageTitle;
    public List<PieceSpawnInfo> pieceLayout;

    // [Header("승리 조건")] 
    //examples
    // public int maxMoves;
    // public Constants.PieceType targetPiece;
}
