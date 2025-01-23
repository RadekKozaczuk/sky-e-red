using System.Collections.Generic;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
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

    Vector3 _moveDirection = Vector3.zero;

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
    const int MaxHp = 3;

    // moving speed
    [SerializeField]
    float Speed = 4;

    //---------------------------------------------------------------------
    // character status
    //---------------------------------------------------------------------
    const int Dissolve = 1;
    const int AttackId = 2;
    const int Surprised = 3;
    readonly Dictionary<int, bool> _playerStatus = new() {{Dissolve, false}, {AttackId, false}, {Surprised, false}};

    void Start() => Hp = MaxHp;

    void Update()
    {
        Status();

        if (Input.GetKeyDown(KeyCode.Space))
            Respawn();

        // this character status
        if (!_playerStatus.ContainsValue(true))
        {
            Damage();
        }
        else if (_playerStatus.ContainsValue(true))
        {
            int statusName = 0;
            foreach (KeyValuePair<int, bool> i in _playerStatus)
                if (i.Value)
                {
                    statusName = i.Key;
                    break;
                }

            if (statusName == Dissolve)
            {
                PlayerDissolve();
            }
        }

        // Dissolve
        if (_hp <= 0 && !_dissolveFlg)
        {
            _animator.CrossFade(_dissolveState, 0.1f, 0, 0);
            _dissolveFlg = true;
        }
        // processing at respawn
        else if (_hp == MaxHp && _dissolveFlg)
        {
            _dissolveFlg = false;
        }
    }

    //------------------------------
    void Status()
    {
        // during dissolve
        if (_dissolveFlg && _hp <= 0)
            _playerStatus[Dissolve] = true;
        else if (!_dissolveFlg)
            _playerStatus[Dissolve] = false;

        // during attacking
        if (_animator.GetCurrentAnimatorStateInfo(0).tagHash == _attackTag)
            _playerStatus[AttackId] = true;
        else if (_animator.GetCurrentAnimatorStateInfo(0).tagHash != _attackTag)
            _playerStatus[AttackId] = false;

        // during damaging
        if (_animator.GetCurrentAnimatorStateInfo(0).fullPathHash == _surprisedState)
            _playerStatus[Surprised] = true;
        else if (_animator.GetCurrentAnimatorStateInfo(0).fullPathHash != _surprisedState)
            _playerStatus[Surprised] = false;
    }

    // dissolve shading
    void PlayerDissolve()
    {
        _dissolveValue -= Time.deltaTime;
        _skinnedMeshRenderer.material.SetFloat(_dissolve, _dissolveValue);
    }

    // play the animation of Attack
    public void Attack() => _animator.CrossFade(_attackState, 0.1f, 0, 0);

    public void MovementStarted()
    {
        _animator.CrossFade(_moveState, 0.1f, 0, 0);
    }
    
    public void SetMovementVector(Vector2 move)
    {
        _moveDirection = new Vector3(move.x, 0f, move.y);

        transform.position += _moveDirection * (Time.deltaTime * Speed);
        
        float angle = Mathf.Atan2(move.x, move.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, angle, 0);

        _moveDirection.x = 0;
        _moveDirection.z = 0;
    }
    
    public void MovementStopped()
    {
        _animator.CrossFade(_idleState, 0.1f, 0, 0);
    }

    //---------------------------------------------------------------------
    // damage
    //---------------------------------------------------------------------
    void Damage()
    {
        // Damaged by outside field.
        if (Input.GetKeyUp(KeyCode.G))
        {
            _animator.CrossFade(_surprisedState, 0.1f, 0, 0);
            Hp--;
        }
    }

    //---------------------------------------------------------------------
    // respawn
    //---------------------------------------------------------------------
    void Respawn()
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
    }
}