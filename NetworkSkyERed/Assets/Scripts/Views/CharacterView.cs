using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class CharacterView : NetworkBehaviour
{
    public PlayerId PlayerId { get; private set; }

    int Hp
    {
        get => _hp;
        set
        {
            _hp = value;
            SceneReferenceHolder.Hp.SetHp(_hp);
        }
    }
    int _hp;
    
    public Vector2 MovementVector
    {
        get => _move;
        set
        {
            // was not moving
            if (_move.magnitude == 0)
            {
                // movement started
                if (value.magnitude > 0)
                {
                    float angle = Mathf.Atan2(value.x, value.y) * Mathf.Rad2Deg;
                    _rotation = Quaternion.Euler(0, angle, 0);
                    
                    _animator.CrossFade(_moveState, 0.1f, 0, 0);
                }
            }
            // was moving
            else
            {
                // movement stopped
                if (value.magnitude == 0)
                    _animator.CrossFade(_idleState, 0.1f, 0, 0);
                else
                {
                    float angle = Mathf.Atan2(value.x, value.y) * Mathf.Rad2Deg;
                    _rotation = Quaternion.Euler(0, angle, 0);
                }
            }
            
            _move = value;
        }
    } 
    Vector2 _move = Vector2.zero;

    [SerializeField]
    Animator _animator;

    Character _character;
    Quaternion _rotation = Quaternion.identity;
    static int _idCounter;

    // Cache hash values
    static readonly int _idleState = Animator.StringToHash("Base Layer.idle");
    static readonly int _moveState = Animator.StringToHash("Base Layer.move");
    static readonly int _surprisedState = Animator.StringToHash("Base Layer.surprised");
    static readonly int _attackState = Animator.StringToHash("Base Layer.attack_shift");
    static readonly int _dissolveState = Animator.StringToHash("Base Layer.dissolve");
    static readonly int _attackTag = Animator.StringToHash("Attack");
    static readonly int _dissolveTag = Animator.StringToHash("Dissolve");

    static readonly int _dissolve = Shader.PropertyToID("_Dissolve");
    static readonly string _dissolve_name = "ghost_dissolve";

    // dissolve
    [SerializeField]
    SkinnedMeshRenderer _skinnedMeshRenderer;

    float _dissolveValue;
    bool _dissolveFlag;

    // order of execution
    // when dynamically spawned: Awake -> OnNetworkSpawn -> Start
    // when in-Scene placed:     Awake -> Start -> OnNetworkSpawn

    float _speed;
    
    bool DissolveStateChange
    {
        get => _dissolveStateChange;
        set
        {
            if (value == _dissolveStateChange)
                return;

            _dissolveStateChange = value;

            if (value)
                return;

            if (NetworkManager.Singleton.IsHost)
            {
                GameObject prefab = GameController.Singleton.CharacterData[(int)_character].Prefab.gameObject; 
                NetworkObjectPool.Singleton.ReturnNetworkObject(NetworkObject, prefab);
            }
        }
    }
    bool _dissolveStateChange;
    
    public void Initialize(PlayerId playerId, CharacterData data)
    {
        PlayerId = playerId;
        _character = data.Character;
        Hp = data.MaxHp;
        _speed = data.Speed;
    }

    public void InitializeVisuals()
    {
        _dissolveValue = 1;
        _skinnedMeshRenderer.material.SetFloat(_dissolve, _dissolveValue);
        _skinnedMeshRenderer.shadowCastingMode = ShadowCastingMode.On;
        _dissolveFlag = false;
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        GameController.Singleton.AddCharacter(NetworkObjectId, this);
    }

    void Update()
    {
        if (_dissolveFlag)
        {
            _dissolveValue -= Time.deltaTime;
            _skinnedMeshRenderer.material.SetFloat(_dissolve, _dissolveValue);
        }

        if (_move.magnitude > 0)
        {
            transform.position += new Vector3(_move.x, 0f, _move.y) * (Time.deltaTime * _speed);
            transform.rotation = _rotation;
        }
        
        Status();
    }
    
    void Status()
    {
        AnimatorClipInfo[] clips = _animator.GetCurrentAnimatorClipInfo(0);

        if (clips.Length == 0)
            return;

        AnimatorClipInfo clip = clips[0];
        DissolveStateChange = clip.clip.name.Equals(_dissolve_name);
    }

    /// <summary>
    /// Starts dissolving animation.
    /// Disables shadows.
    /// </summary>
    public void DissolveMethod()
    {
        _animator.SetBool(_dissolveTag, true);
    }

    public void OnDissolveStart()
    {
        _dissolveFlag = true;
        _skinnedMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
    }

    // play the animation of Attack
    public void Attack() => _animator.CrossFade(_attackState, 0.1f, 0, 0);

    void Damage()
    {
        _animator.CrossFade(_surprisedState, 0.1f, 0, 0);
        Hp--;
    }

    /*void Respawn()
    {
        // player HP
        Hp = MaxHp;

        transform.position = new Vector3(0, -0.7f, 0);              // player position
        transform.rotation = Quaternion.Euler(new Vector3(0, -180, 0)); // player facing

        // reset Dissolve
        _dissolveValue = 1;
        _skinnedMeshRenderer.material.SetFloat(_dissolve, _dissolveValue);

        // reset animation
        _animator.CrossFade(_idleState, 0.1f, 0, 0);
    }*/
}