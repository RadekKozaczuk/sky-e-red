using Unity.Netcode;
using UnityEngine;

public class SceneReferenceHolder : MonoBehaviour
{
    public static HpView Hp;
    public static NetworkObject CharacterContainer;

    [SerializeField]
    HpView _hp;
    
    [SerializeField]
    NetworkObject _characterContainer;

    void Awake()
    {
        Hp = _hp;
        CharacterContainer = _characterContainer;
    }
}