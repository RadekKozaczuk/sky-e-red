using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameController : NetworkBehaviour
{
    public static GameController Singleton;

    // runtime
    readonly Dictionary<PlayerId, PlayerModel> _playersModels = new();

    /// <summary>
    /// Maps clientId to PlayerId.
    /// ClientId do not recycle instantaneously.
    /// </summary>
    readonly Dictionary<ulong, PlayerId> _idToPlayerId = new();

    readonly CharacterView[] _characters = new CharacterView[Enum.GetValues(typeof(PlayerId)).Length];
    
    public readonly Dictionary<ulong, CharacterView> Characters = new();

    [SerializeField]
    GameData _gameData;

    [SerializeField]
    CharacterData[] _characterData;
    
    void Awake()
    {
        Singleton = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += clientId =>
        {
            // only host can spawn
            if (!NetworkManager.Singleton.IsHost)
                return;
            
            PlayerId playerId = clientId == NetworkManager.Singleton.LocalClient.ClientId
                ? PlayerId.FirstPlayer
                : PlayerId.SecondPlayer;

            _idToPlayerId.Add(clientId, playerId);

            List<Character> list = playerId == PlayerId.FirstPlayer 
                ? _gameData.FirstPlayerStartCharacters.ToList() 
                : _gameData.SecondPlayerStartCharacters.ToList();

            var player = new PlayerModel(list);
            _playersModels.Add(playerId, player);
            
            // initially always spawn the first character
            CharacterData data = _characterData[(int)player.CurrentCharacter];
            CharacterView character = SpawnCharacter(playerId, new Vector3(0, -0.7f, 0), Quaternion.Euler(0, 180f, 0), data);
            _characters[(int)playerId] = character;
        };   
    }

    public void Move(Vector2 move)
    {
        ulong id = NetworkManager.Singleton.LocalClientId;
        
        if (NetworkManager.Singleton.IsHost)
        {
            PlayerId playerId = _idToPlayerId[id];
            _characters[(int)playerId].MovementVector = move;
        }
        else
        {
            float angle = move.magnitude > 0 
                ? Mathf.Atan2(move.x, move.y) * Mathf.Rad2Deg
                : -1f;

            ushort quantified = Mathf.FloatToHalf(angle);

            MoveRpc((byte)id, quantified);
        }
    }
    
    public void Attack()
    {
        ulong id = NetworkManager.Singleton.LocalClientId;

        if (NetworkManager.Singleton.IsHost)
        {
            PlayerId playerId = _idToPlayerId[id];
            _characters[(int)playerId].Attack();
        }
        else
            AttackRpc((byte)id);
    }
    
    public void ChangeCharacter()
    {
        ulong id = NetworkManager.Singleton.LocalClientId;

        if (NetworkManager.Singleton.IsHost)
            ChangeCharacterLogic(id);
        else
            ChangeCharacterRpc((byte)id);
    }

    public void OnCharacterDeath(ulong networkObjectId)
    {
        if (!NetworkManager.Singleton.IsHost)
            return;

        CharacterView view = Characters[networkObjectId];
        CharacterData data = _characterData[(int)view.Character];
        NetworkObjectPool.Singleton.ReturnNetworkObject(view.NetworkObject, data.Prefab.gameObject);
    }
    
    [Rpc(SendTo.Server)]
    // ReSharper disable once MemberCanBeMadeStatic.Local
    void MoveRpc(byte clientId, ushort move)
    {
        PlayerId playerId = _idToPlayerId[clientId];

        CharacterView character = _characters[(int)playerId];
        float angle = Mathf.HalfToFloat(move);

        if (Mathf.Approximately(angle, -1f))
        {
            character.MovementVector = Vector2.zero;
        }
        else
        {
            float radians = angle * Mathf.Deg2Rad;
            var moveVector = new Vector2(Mathf.Sin(radians), Mathf.Cos(radians));
            character.MovementVector = moveVector;
        }
    }
    
    [Rpc(SendTo.Server)]
    // ReSharper disable once MemberCanBeMadeStatic.Local
    void AttackRpc(byte clientId)
    {
        PlayerId playerId = _idToPlayerId[clientId];
        _characters[(int)playerId].Attack();
    }

    [Rpc(SendTo.Server)]
    // ReSharper disable once MemberCanBeMadeStatic.Local
    void ChangeCharacterRpc(byte clientId)
    {
        ChangeCharacterLogic(clientId);
    }

    void ChangeCharacterLogic(ulong id)
    {
        PlayerId playerId = _idToPlayerId[id];
        CharacterView previous = _characters[(int)playerId];
        previous.Dissolve();
        
        PlayerModel model = _playersModels[playerId];
        model.ChangeCharacter();
            
        // initially always spawn the first character
        Transform t = previous.transform;
        CharacterData data = _characterData[(int)model.CurrentCharacter];
        
        CharacterView character = SpawnCharacter(playerId, t.position, t.rotation, data);
        character.MovementVector = previous.MovementVector;
        previous.MovementVector = Vector2.zero;
        
        _characters[(int)playerId] = character;
    }
    
    CharacterView SpawnCharacter(PlayerId playerId, Vector3 pos, Quaternion rot, CharacterData data)
    {
        NetworkObject netObject = NetworkObjectPool.Singleton.GetNetworkObject(data.Prefab.gameObject, pos, rot);
        var character = netObject.GetComponent<CharacterView>();

        // spawn over the network
        if (!character.IsSpawned)
            netObject.SpawnWithOwnership(NetworkManager.Singleton.LocalClient.ClientId);
        
        character.Initialize(playerId, data);
        character.InitializeVisuals();
        character.NetworkObject.TrySetParent(SceneReferenceHolder.CharacterContainer);
        
        // send info to everybody else
        InitializeRpc(netObject.NetworkObjectId);

        return character;
    }
    
    [Rpc(SendTo.NotMe)]
    void InitializeRpc(ulong networkObjectId)
    {
        Debug.Log($"Initialize RPC executed on clientId: {NetworkManager.Singleton.LocalClient.ClientId}, networkObjectId: {networkObjectId}");

        CharacterView character = Characters[networkObjectId];
        character.InitializeVisuals();
    }
}

