using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameController : NetworkBehaviour
{
    public static GameController Singleton;
    
    [SerializeField]
    CharacterView[] _characterPrefabs;

    readonly Dictionary<ulong, CharacterView> _characters = new();
    
    void Awake()
    {
        Singleton = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += clientId =>
        {
            //_joinedPlayers++;
            // ignore self connection
            //if (clientId == 0)
            //    return;

            // only host can spawn
            if (!NetworkManager.Singleton.IsHost)
                return;
            
            // initially always spawn the first character
            CharacterView player = Instantiate(_characterPrefabs[0], new Vector3(0, -0.7f, 0), Quaternion.identity,
                                               SceneReferenceHolder.CharacterContainer);

            //PresentationData.NetworkPlayers[(int)PlayerId.Player2] = player;
            //SetPlayerOutfit(player);

            // spawn over the network
            player.NetworkObject.SpawnWithOwnership(clientId);
            
            _characters.Add((byte)clientId, player);
            
            Debug.Log($"Spawned player with id: {clientId}");
        };   
    }

    public void Move(Vector2 move)
    {
        ulong id = NetworkManager.Singleton.LocalClientId;
        
        if (NetworkManager.Singleton.IsHost)
            _characters[id].SetMovementVector(move);
        else
            MoveRpc((byte)id, move);
    }
    
    public void Attack()
    {
        if (NetworkManager.Singleton.IsHost)
            ;
        else
            AttackRpc((byte)NetworkManager.Singleton.LocalClientId);
    }
    
    public void ChangeCharacter(byte clientId)
    {
        if (NetworkManager.Singleton.IsHost)
            ;
        else
            AttackRpc((byte)NetworkManager.Singleton.LocalClientId);
    }
    
    [Rpc(SendTo.Server)]
    void MoveRpc(byte clientId, Vector2 move)
    {
        _characters[clientId].SetMovementVector(move);
        
        Debug.Log($"MoveRpc Body executed, clientId: {clientId}");
    }
    
    // todo: does not work
    [Rpc(SendTo.Server)]
    void AttackRpc(byte clientId)
    {
        Debug.Log($"AttackRPC Body executed, clientId: {clientId}");
    }

    [Rpc(SendTo.Server)]
    void ChangeCharacterRpc(byte clientId)
    {
        
    }
}

