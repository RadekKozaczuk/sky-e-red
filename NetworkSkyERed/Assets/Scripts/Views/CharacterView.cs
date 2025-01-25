using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class CharacterView : NetworkBehaviour
{
    public int Id { get; private set; }
    public Character Character { get; private set; }
    public PlayerId PlayerId { get; private set; }

    static int _idCounter;
    
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

    [SerializeField]
    Animator _animator;

    Vector3 _move = Vector3.zero;

    // Cache hash values
    static readonly int _idleState = Animator.StringToHash("Base Layer.idle");
    static readonly int _moveState = Animator.StringToHash("Base Layer.move");
    static readonly int _surprisedState = Animator.StringToHash("Base Layer.surprised");
    static readonly int _attackState = Animator.StringToHash("Base Layer.attack_shift");
    static readonly int _dissolveState = Animator.StringToHash("Base Layer.dissolve");
    static readonly int _attackTag = Animator.StringToHash("Attack");
    
    static readonly int _dissolve = Shader.PropertyToID("_Dissolve");

    // dissolve
    [SerializeField]
    SkinnedMeshRenderer _skinnedMeshRenderer;

    float _dissolveValue = 1;
    bool _dissolveFlg;
    
    /*const int Dissolve = 1;
    const int AttackId = 2;
    const int Surprised = 3;
    readonly Dictionary<int, bool> _playerStatus = new() {{Dissolve, false}, {AttackId, false}, {Surprised, false}};*/
    
    static readonly int _dissolve1 = Animator.StringToHash("Dissolve");

    // order of execution
    // when dynamically spawned: Awake -> OnNetworkSpawn -> Start
    // when in-Scene placed:     Awake -> Start -> OnNetworkSpawn

    float _speed;
    
    public void Initialize(PlayerId playerId, CharacterData data)
    {
        Character = data.Character;
        PlayerId = playerId;
        Hp = data.MaxHp;
        _speed = data.Speed;
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Id = _idCounter++;
    }

    void Update()
    {
        if (_dissolveFlg)
        {
            _dissolveValue -= Time.deltaTime;
            _skinnedMeshRenderer.material.SetFloat(_dissolve, _dissolveValue);
        }
    }

    /// <summary>
    /// Starts dissolving animation.
    /// Disables shadows.
    /// </summary>
    public void Dissolve()
    {
        _animator.SetBool(_dissolve1, true);
    }

    public void OnDissolveStart()
    {
        _dissolveFlg = true;
        _skinnedMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
    }

    // play the animation of Attack
    public void Attack() => _animator.CrossFade(_attackState, 0.1f, 0, 0);

    /// <summary>
    /// Last change of the movement vector.
    /// Meaning that if you want the character to stop you have to send <see cref="Vector2.zero"/>.
    /// </summary>
    public void Move(Vector2 move)
    {
        bool movementStopped = false;
        
        // was not moving
        if (_move.magnitude == 0)
        {
            // movement started
            if (move.magnitude > 0)
            {
                _animator.CrossFade(_moveState, 0.1f, 0, 0);
            }
        }
        // was moving
        else
        {
            // movement stopped
            if (move.magnitude == 0)
            {
                _animator.CrossFade(_idleState, 0.1f, 0, 0);
                movementStopped = true;
            }
        }
        
        _move = new Vector3(move.x, 0f, move.y);
        transform.position += _move * (Time.deltaTime * _speed);

        if (movementStopped)
            return;

        float angle = Mathf.Atan2(move.x, move.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, angle, 0);
    }

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