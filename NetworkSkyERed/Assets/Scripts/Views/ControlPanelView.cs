using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ControlPanelView : MonoBehaviour
{
    [SerializeField]
    Button _host;
    
    [SerializeField]
    Button _join;

    void Awake()
    {
        _host.onClick.AddListener(StartHost);
        _join.onClick.AddListener(Join);
    }

    static void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    static void Join()
    {
        NetworkManager.Singleton.StartClient();
    }
}
