using UnityEngine;
using UnityEngine.UI;

public class Piece : MonoBehaviour
{
    [SerializeField] private PieceData _data;

    public PieceData Data
    {
        get { return _data; }
        set { _data = value; }
    }
    private (int, int) index;
    public (int, int) Index
    {
        get { return index; }
        set { index = value; }
    }

    public Piece Clone(GameObject baseObj)
    {
        Piece newPiece = baseObj.AddComponent<Piece>();

        newPiece.Data = _data;
        newPiece.Index = index;

        return newPiece;
    }
    void Setup()
    {
        Canvas canvas = GetComponentInChildren<Canvas>();
        Image image = canvas.transform.GetChild(0).GetComponent<Image>();
        image.sprite = _data.icon;
    }
    
}
