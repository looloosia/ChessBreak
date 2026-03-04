using UnityEngine;
using UnityEngine.UI;

public class Piece : MonoBehaviour
{
    [SerializeField] private PieceData _data;
    public PieceData Data => _data;
    private (int, int) index;

    public (int, int) Index
    {
        get { return index; }
        set { index = value; }
    }
    void Setup()
    {
        Canvas canvas = GetComponentInChildren<Canvas>();
        Image image = canvas.transform.GetChild(0).GetComponent<Image>();
        image.sprite = _data.icon;
    }
    
}
