using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameController : NetworkBehaviour
{
    public static GameController Singleton;
    
    // prefabs
    [SerializeField]
    CharacterView[] _characterPrefabs;

    // runtime
    readonly Dictionary<PlayerId, PlayerModel> _playersModels = new();

    /// <summary>
    /// Maps clientId to PlayerId.
    /// ClientId do not recycle instantaneously.
    /// </summary>
    readonly Dictionary<ulong, PlayerId> _idToPlayerId = new();
    
    /// <summary>
    /// Maps clientId to PlayerId.
    /// ClientId do not recycle instantaneously.
    /// </summary>
    readonly Dictionary<PlayerId, CharacterView> _characterViews = new();

    // configuration
    [SerializeField]
    Character[] _hostStartCharacters;
    
    [SerializeField]
    Character[] _clientStartCharacters;
    
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
            
            PlayerId playerId = clientId == 0 ? PlayerId.FirstPlayer : PlayerId.SecondPlayer;
            _idToPlayerId.Add(clientId, playerId);

            List<Character> list = playerId == PlayerId.FirstPlayer ? _hostStartCharacters.ToList() : _clientStartCharacters.ToList();
            var player = new PlayerModel(list);
            _playersModels.Add(playerId, player);
            
            // initially always spawn the first character
            CharacterView character = Instantiate(_characterPrefabs[(int)player.CurrentCharacter], new Vector3(0, -0.7f, 0), Quaternion.identity);

            // spawn over the network
            character.NetworkObject.SpawnWithOwnership(clientId);
            
            _characterViews.Add(playerId, character);
        };   
    }

    public void Move(Vector2 move)
    {
        ulong id = NetworkManager.Singleton.LocalClientId;

        if (NetworkManager.Singleton.IsHost)
        {
            PlayerId playerId = _idToPlayerId[id];
            _characterViews[playerId].Move(move);
        }
        else
            MoveRpc((byte)id, move);
    }
    
    public void Attack()
    {
        ulong id = NetworkManager.Singleton.LocalClientId;

        if (NetworkManager.Singleton.IsHost)
        {
            PlayerId playerId = _idToPlayerId[id];
            _characterViews[playerId].Attack();
        }
        else
            AttackRpc((byte)id);
    }
    
    public void ChangeCharacter()
    {
        ulong id = NetworkManager.Singleton.LocalClientId;

        if (NetworkManager.Singleton.IsHost)
        {
            PlayerId playerId = _idToPlayerId[id];

            PlayerModel model = _playersModels[playerId];

            if (model.CanChangeCharacter)
            {
                CharacterView character = _characterViews[playerId];
                character.PlayerDissolve();
                
                model.ChangeCharacter();
                
                
            }
            
            

            // start disolving this one
            //_characterViews[playerId].di
        }
        else
            ChangeCharacterRpc((byte)id);
    }
    
    [Rpc(SendTo.Server)]
    // ReSharper disable once MemberCanBeMadeStatic.Local
    void MoveRpc(byte clientId, Vector2 move)
    {
        PlayerId playerId = _idToPlayerId[clientId];
        _characterViews[playerId].Move(move);
    }
    
    [Rpc(SendTo.Server)]
    // ReSharper disable once MemberCanBeMadeStatic.Local
    void AttackRpc(byte clientId)
    {
        PlayerId playerId = _idToPlayerId[clientId];
        _characterViews[playerId].Attack();
    }

    [Rpc(SendTo.Server)]
    // ReSharper disable once MemberCanBeMadeStatic.Local
    void ChangeCharacterRpc(byte clientId)
    {
        
    }
}

