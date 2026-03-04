using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    [SerializeField] private Vector3 _blockMinPos;
    [SerializeField] private Vector3 _blockScale;
    private float _blockXDistance;
    private float _blockZDistance;
    public Block.OnBlockClicked onBlockClicked;

    void Start()
    {
        _blockXDistance = _blockScale.x;
        _blockZDistance = _blockScale.z;
        
        for (int z = 0; z < Constants.BOARD_SIZE; z++)
        {
            for (int x = 0; x < Constants.BOARD_SIZE; x++)
            {
                int localX = x;
                int localZ = z;
                Vector3 spawnPos = new Vector3(_blockMinPos.x - localX * _blockScale.x, _blockMinPos.y, _blockMinPos.z + localZ * _blockScale.z);
                GameObject newBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
                newBlock.transform.parent = this.transform;
                newBlock.GetComponent<BoxCollider>().isTrigger = true;
                Block blockScript = newBlock.AddComponent<Block>();
                blockScript.Row = localX;
                blockScript.Col = localZ;
                blockScript.InitBlock((localX,localZ), (piece, pos) =>
                {
                    onBlockClicked?.Invoke(piece, (pos.Item1, pos.Item2));
                });
                newBlock.GetComponent<Renderer>().enabled = false;
                newBlock.transform.localScale = _blockScale;
                newBlock.transform.localPosition = spawnPos;
                newBlock.name = $"Block_{localX}_{localZ}";
            }
        }
    }

   
}
