using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField]
    CharacterController _characterController;

    Vector3 _moveDirection = Vector3.zero;

    // Cache hash values
    static readonly int _idleState = Animator.StringToHash("Base Layer.idle");
    static readonly int _moveState = Animator.StringToHash("Base Layer.move");
    static readonly int _surprisedState = Animator.StringToHash("Base Layer.surprised");
    static readonly int _attackState = Animator.StringToHash("Base Layer.attack_shift");
    static readonly int _dissolveState = Animator.StringToHash("Base Layer.dissolve");
    static readonly int _attackTag = Animator.StringToHash("Attack");

    // dissolve
    [SerializeField]
    SkinnedMeshRenderer[] MeshR;

    float _dissolveValue = 1;
    bool _dissolveFlg;
    const int MaxHp = 3;
    Text _hpText;

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
    static readonly int _dissolve = Shader.PropertyToID("_Dissolve");

    void Start() => Hp = MaxHp;

    void Update()
    {
        Status();
        Gravity();

        if (Input.GetKeyDown(KeyCode.Space))
            Respawn();

        // this character status
        if (!_playerStatus.ContainsValue(true))
        {
            if (Input.GetKeyDown(KeyCode.F))
                Attack();
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
            else if (statusName == AttackId)
            {
                if (Input.GetKeyDown(KeyCode.F))
                    Attack();
            }
            else if (statusName == Surprised)
            {
                // nothing method
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
        foreach (SkinnedMeshRenderer mesh in MeshR)
            mesh.material.SetFloat(_dissolve, _dissolveValue);

        if (_dissolveValue <= 0)
            _characterController.enabled = false;
    }

    // play a animation of Attack
    public void Attack() => _animator.CrossFade(_attackState, 0.1f, 0, 0);

    //---------------------------------------------------------------------
    // gravity for fall of this character
    //---------------------------------------------------------------------
    void Gravity()
    {
        if (_characterController.enabled)
        {
            if (CheckGrounded())
                if (_moveDirection.y < -0.1f)
                    _moveDirection.y = -0.1f;
            _moveDirection.y -= 0.1f;
            _characterController.Move(_moveDirection * Time.deltaTime);
        }
    }

    //---------------------------------------------------------------------
    // whether it is grounded
    //---------------------------------------------------------------------
    bool CheckGrounded()
    {
        if (_characterController.isGrounded && _characterController.enabled)
            return true;

        var ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        float range = 0.2f;
        return Physics.Raycast(ray, range);
    }

    public void MovementStarted()
    {
        _flag = true;
        Debug.Log("MovementStarted");
        _animator.CrossFade(_moveState, 0.1f, 0, 0);
    }
    
    public void SetMovementVector(Vector2 move)
    {
        _moveDirection = new Vector3(move.x, 0f, move.y);
        if (_characterController.enabled)
            _characterController.Move(_moveDirection * (Time.deltaTime * Speed));
        
        float angle = Mathf.Atan2(move.x, move.y) * Mathf.Rad2Deg;

        if (_flag)
        {
            Debug.Log($"Move: {_moveDirection}, angle: {angle}");
            _flag = false;
        }
        transform.rotation = Quaternion.Euler(0, angle, 0);

        _moveDirection.x = 0;
        _moveDirection.z = 0;
        
        //Debug.Log("Movement: " + move);
    }

    bool _flag = true;
    
    public void MovementStopped()
    {
        _flag = true;
        Debug.Log("MovementStopped");
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

        _characterController.enabled = false;
        transform.position = Vector3.zero;                   // player position
        transform.rotation = Quaternion.Euler(new Vector3(0, -180, 0)); // player facing
        _characterController.enabled = true;

        // reset Dissolve
        _dissolveValue = 1;
        foreach (SkinnedMeshRenderer mesh in MeshR)
            mesh.material.SetFloat(_dissolve, _dissolveValue);

        // reset animation
        _animator.CrossFade(_idleState, 0.1f, 0, 0);
    }
}