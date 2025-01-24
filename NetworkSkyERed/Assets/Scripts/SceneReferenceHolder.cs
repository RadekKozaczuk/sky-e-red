using UnityEngine;

public class SceneReferenceHolder : MonoBehaviour
{
    public static HpView Hp;

    [SerializeField]
    HpView _hp;

    void Awake()
    {
        Hp = _hp;
    }
}