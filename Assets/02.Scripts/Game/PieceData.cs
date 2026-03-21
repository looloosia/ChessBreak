using UnityEngine;

[CreateAssetMenu(fileName = "ChessPiece", menuName = "Scriptable Objects/ChessPiece")]
public class PieceData : ScriptableObject
{
    [Header("기본 정보")]
    public Constants.PieceType pieceType;
    public Constants.PlayerColor pieceColor;
    public int pieceValue;

    [Header("비주얼 에셋")] 
    public GameObject prefab;
    public Sprite icon;
}
