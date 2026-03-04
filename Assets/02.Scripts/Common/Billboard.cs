using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform mainCameraTransform;
    
    void Start()
    {
        if (Camera.main != null)
            mainCameraTransform = Camera.main.transform;
    }
    
    void LateUpdate()
    {
        if (mainCameraTransform == null) return;
        
        transform.rotation = mainCameraTransform.rotation;
        
    }
}
