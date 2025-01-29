using Unity.Netcode;
using UnityEngine;

public class CharacterView : NetworkBehaviour
{
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
    float _dissolveValue;
    bool _dissolveFlag;
    float _speed;

    // Cache hash values
    static readonly int _idleState = Animator.StringToHash("Base Layer.idle");
    static readonly int _moveState = Animator.StringToHash("Base Layer.move");
    static readonly int _surprisedState = Animator.StringToHash("Base Layer.surprised");
    static readonly int _attackState = Animator.StringToHash("Base Layer.attack_shift");
    static readonly int _attackTag = Animator.StringToHash("Attack");
    static readonly int _dissolveTag = Animator.StringToHash("Dissolve");

    static readonly int _dissolve = Shader.PropertyToID("_Dissolve");
    const string DissolveAnimationName = "ghost_dissolve";

    // dissolve
    //[SerializeField]
    //SkinnedMeshRenderer _skinnedMeshRenderer;

    // order of execution
    // when dynamically spawned: Awake -> OnNetworkSpawn -> Start
    // when in-Scene placed:     Awake -> Start -> OnNetworkSpawn

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

                GameController.Singleton.CharacterSetActiveRpc(NetworkObject.NetworkObjectId, false);
            }
        }
    }
    bool _dissolveStateChange;
    
    public void Initialize(CharacterData data)
    {
        _character = data.Character;
        _speed = data.Speed;
    }

    public void InitializeVisuals()
    {
        _dissolveValue = 1;
        //_skinnedMeshRenderer.material.SetFloat(_dissolve, _dissolveValue);
        //_skinnedMeshRenderer.shadowCastingMode = ShadowCastingMode.On;
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
            //_skinnedMeshRenderer.material.SetFloat(_dissolve, _dissolveValue);
        }

        if (_move.magnitude > 0)
        {
            transform.position += new Vector3(_move.x, 0f, _move.y) * (Time.deltaTime * _speed);
            transform.rotation = _rotation;
        }
        
        DissolveStatusUpdate();
    }
    
    void DissolveStatusUpdate()
    {
        AnimatorClipInfo[] clips = _animator.GetCurrentAnimatorClipInfo(0);

        if (clips.Length == 0)
            return;

        AnimatorClipInfo clip = clips[0];
        DissolveStateChange = clip.clip.name.Equals(DissolveAnimationName);
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
        //_skinnedMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
    }

    // play the animation of Attack
    public void Attack() => _animator.CrossFade(_attackState, 0.1f, 0, 0);

    void Damage()
    {
        _animator.CrossFade(_surprisedState, 0.1f, 0, 0);
    }
}