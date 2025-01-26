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

    /// <summary>
    /// The Character the player is currently using.
    /// </summary>
    readonly CharacterView[] _characters = new CharacterView[Enum.GetValues(typeof(PlayerId)).Length];
    
    /// <summary>
    /// These characters will be destroyed soon.
    /// </summary>
    readonly Dictionary<PlayerId, List<CharacterView>> _deadCharacters = new();

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
            _deadCharacters.Add(playerId, new List<CharacterView>());
            
            // initially always spawn the first character
            CharacterData data = _characterData[(int)player.CurrentCharacter];
            CharacterView character = Instantiate(
                data.Prefab,
                new Vector3(0, -0.7f, 0),
                Quaternion.Euler(0, 180, 0));

            character.Initialize(playerId, data);
            
            // spawn over the network
            character.NetworkObject.SpawnWithOwnership(NetworkManager.Singleton.LocalClient.ClientId);

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

    public void OnCharacterDeath(PlayerId playerId, int characterId)
    {
        if (!NetworkManager.Singleton.IsHost)
            return;

        List<CharacterView> list = _deadCharacters[playerId];
        
        int index = list.FindIndex(c => c.Id == characterId);
        uint tickRate = NetworkManager.Singleton.NetworkTickSystem.TickRate;
        
        CharacterView view = list[index];
        //float defer = _characterData[(int)view.Character].DissolveAnimationLength * tickRate;
        //list[index].NetworkObject.DeferredDespawnTick = (int)defer;
        list[index].NetworkObject.Despawn();
        //list[index].NetworkObject.DeferDespawn((int)defer);
        
        // todo: animation should start with delay and fast forward
        
        list.RemoveAt(index);
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
        _deadCharacters[playerId].Add(previous);
        
        PlayerModel model = _playersModels[playerId];
        model.ChangeCharacter();
            
        // initially always spawn the first character
        Transform t = previous.transform;
        CharacterData data = _characterData[(int)model.CurrentCharacter];
        CharacterView nextCharacter = Instantiate(data.Prefab, t.position, t.rotation);
        nextCharacter.Initialize(playerId, data);
        nextCharacter.MovementVector = previous.MovementVector;
        previous.MovementVector = Vector2.zero;
        
        // spawn over the network
        nextCharacter.NetworkObject.SpawnWithOwnership(NetworkManager.Singleton.LocalClient.ClientId);
        _characters[(int)playerId] = nextCharacter;
    }
}

