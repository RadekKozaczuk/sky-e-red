using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameController : NetworkBehaviour
{
    public static GameController Singleton;
    
    public GameData GameData;
    public CharacterData[] CharacterData;
    public NetworkVariable<ushort> FirstPlayerCurrentHp = new();
    public NetworkVariable<ushort> SecondPlayerCurrentHp = new();

    readonly Dictionary<PlayerId, PlayerModel> _playersModels = new();

    /// <summary>
    /// Key: clientId
    /// Value: currently played character's networkObjectId and PlayerId
    /// </summary>
    readonly Dictionary<ulong, (ulong, PlayerId)> _idToPlayerId = new();

    /// <summary>
    /// Key: NetworkObjectId
    /// Value: CharacterView
    /// </summary>
    readonly Dictionary<ulong, CharacterView> _characters = new();

    /// <summary>
    /// Object may not yet be present at the time of sending the message.
    /// </summary>
    readonly List<(ulong, bool)> _pendingActivations = new();
    
    /// <summary>
    /// Used for movement vector quantization.
    /// </summary>
    const float MinusOne = -1f;

    void Awake() => Singleton = this;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += clientId =>
        {
            // only host can spawn
            if (!NetworkManager.Singleton.IsHost)
                return;

            PlayerId playerId;
            List<Character> list;
            Vector2 spawnPos;
            
            // first player
            if (clientId == NetworkManager.Singleton.LocalClient.ClientId)
            {
                playerId = PlayerId.FirstPlayer;
                list = GameData.FirstPlayerStartCharacters.ToList();
                spawnPos = GameData.FirstPlayerSpawnPosition;
            }
            else
            {
                playerId = PlayerId.SecondPlayer;
                list = GameData.SecondPlayerStartCharacters.ToList();
                spawnPos = GameData.SecondPlayerSpawnPosition;
            }
            
            // builder patten
            var player = new PlayerBuilder();
            CharacterData data;
            
            foreach (Character character in list)
            {
                data = CharacterData[(int)character];
                player.HasCharacter(character)
                      .WithHp(data.MaxHp)
                      .WithDamage(data.Damage)
                      .WithSpeed(data.Speed);
            }

            _playersModels.Add(playerId, player.Model);

            // initially always spawn the first character
            var position = new Vector3(spawnPos.x, GameData.DefaultPositionYOffset, spawnPos.y);
            Quaternion rotation = Quaternion.Euler(0, GameData.DefaultSpawnRotation, 0);
            data = CharacterData[(int)player.Model.CurrentCharacter];

            CharacterView view = SpawnCharacter(position, rotation, data);
            _idToPlayerId.Add(clientId, (view.NetworkObjectId, playerId));
        };   
    }

    void Update()
    {
        // in rare scenarios game objects may not be yet replicated on clients at the moment of receiving a CharacterSetActiveRpc call 
        for (int i = 0; i < _pendingActivations.Count; i++)
        {
            (ulong networkObjectId, bool active) = _pendingActivations[i];
            // ReSharper disable once InvertIf
            if (_characters.TryGetValue(networkObjectId, out CharacterView view))
            {
                view.gameObject.SetActive(active);
                _pendingActivations.RemoveAt(i);
                --i;
            }
        }

        // update hp
        if (NetworkManager.Singleton.IsHost)
        {
            if (_playersModels.TryGetValue(PlayerId.FirstPlayer, out PlayerModel model))
                FirstPlayerCurrentHp.Value = (ushort)model.CurrentHp;
            
            if (_playersModels.TryGetValue(PlayerId.SecondPlayer, out model))
                SecondPlayerCurrentHp.Value = (ushort)model.CurrentHp;
        }
        
        SceneReferenceHolder.Hp.SetHp(FirstPlayerCurrentHp.Value);
    }

    public void AddCharacter(ulong networkObjectId, CharacterView view) => _characters.Add(networkObjectId, view);

    public void Move(Vector2 move)
    {
        ulong id = NetworkManager.Singleton.LocalClientId;
        
        if (NetworkManager.Singleton.IsHost)
        {
            ulong netObjectId = _idToPlayerId[id].Item1;
            _characters[netObjectId].MovementVector = move;
        }
        else
        {
            float angle = move.magnitude > 0
                ? Mathf.Atan2(move.x, move.y) * Mathf.Rad2Deg
                : MinusOne;

            ushort quantified = Mathf.FloatToHalf(angle);
            MoveRpc((byte)id, quantified);
        }
    }
    
    public void Attack()
    {
        ulong id = NetworkManager.Singleton.LocalClientId;

        if (NetworkManager.Singleton.IsHost)
            _characters[_idToPlayerId[id].Item1].Attack();
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
    
    [Rpc(SendTo.NotMe)]
    public void CharacterSetActiveRpc(ulong networkObjectId, bool active) => _pendingActivations.Add((networkObjectId, active));

    [Rpc(SendTo.NotMe)]
    void InitializeVisualsRpc(ulong networkObjectId) => _characters[networkObjectId].InitializeVisuals();
    
    [Rpc(SendTo.Server)]
    // ReSharper disable once MemberCanBeMadeStatic.Local
    void MoveRpc(byte clientId, ushort move)
    {
        ulong netObjectId = _idToPlayerId[clientId].Item1;

        CharacterView character = _characters[netObjectId];
        float angle = Mathf.HalfToFloat(move);

        if (Mathf.Approximately(angle, MinusOne))
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
        ulong netObjectId = _idToPlayerId[clientId].Item1;
        _characters[netObjectId].Attack();
    }

    [Rpc(SendTo.Server)]
    // ReSharper disable once MemberCanBeMadeStatic.Local
    void ChangeCharacterRpc(byte clientId) => ChangeCharacterLogic(clientId);

    void ChangeCharacterLogic(ulong clientId)
    {
        (ulong netObjectId, PlayerId playerId) = _idToPlayerId[clientId];
        CharacterView previous = _characters[netObjectId];
        previous.DissolveMethod();
        
        PlayerModel model = _playersModels[playerId];
        model.ChangeCharacter();
            
        // initially always spawn the first character
        Transform t = previous.transform;

        CharacterData data = CharacterData[(int)model.CurrentCharacter];
        CharacterView next = SpawnCharacter(t.position, t.rotation, data);
        next.MovementVector = previous.MovementVector;
        previous.MovementVector = Vector2.zero;
        
        _characters[netObjectId] = next;
    }
    
    CharacterView SpawnCharacter(Vector3 pos, Quaternion rot, CharacterData data)
    {
        NetworkObject netObject = NetworkObjectPool.Singleton.GetNetworkObject(data.Prefab.gameObject, pos, rot);
        CharacterSetActiveRpc(netObject.NetworkObjectId, true);
        var view = netObject.GetComponent<CharacterView>();

        // spawn over the network
        if (!view.IsSpawned)
        {
            netObject.SpawnWithOwnership(NetworkManager.Singleton.LocalClient.ClientId);
            netObject.TrySetParent(SceneReferenceHolder.CharacterContainer);
        }

        view.Initialize(data);
        view.InitializeVisuals();
        
        // send info to everybody else
        InitializeVisualsRpc(netObject.NetworkObjectId);

        return view;
    }
}

