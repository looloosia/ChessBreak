using UnityEngine;
using UnityEngine.UI;

public class Piece : MonoBehaviour
{
    [SerializeField] private PieceData _data;

    void Setup()
    {
        Canvas canvas = GetComponentInChildren<Canvas>();
        Image image = canvas.transform.GetChild(0).GetComponent<Image>();
        image.sprite = _data.icon;
    }
    
}
