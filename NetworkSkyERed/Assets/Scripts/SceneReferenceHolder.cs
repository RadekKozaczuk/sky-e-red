using UnityEngine;

public class SceneReferenceHolder : MonoBehaviour
{
    public static PlayerView Player;
    public static HpView Hp;

    [SerializeField]
    PlayerView _player;

    [SerializeField]
    HpView _hp;

    void Awake()
    {
        Player = _player;
        Hp = _hp;
    }
}