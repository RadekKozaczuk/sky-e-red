using UnityEngine;

public class SceneReferenceHolder : MonoBehaviour
{
    public static GhostScript Ghost;
    
    [SerializeField]
    GhostScript _audioContainer;
    
    void Awake()
    {
        Ghost = _audioContainer;
    }
}
