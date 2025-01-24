using UnityEngine;

public class SceneReferenceHolder : MonoBehaviour
{
    public static HpView Hp;
    public static Transform CharacterContainer;

    [SerializeField]
    HpView _hp;

    [SerializeField]
    Transform _characterContainer;

    void Awake()
    {
        Hp = _hp;
        CharacterContainer = _characterContainer;
    }
}