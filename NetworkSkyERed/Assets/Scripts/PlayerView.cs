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

    void Start()
    {
        Hp = MaxHp;
    }

    void Update()
    {
        Status();
        Gravity();

        if (Input.GetKeyDown(KeyCode.Space))
            Respawn();

        // this character status
        if(!_playerStatus.ContainsValue( true ))
        {
            Move();
            PlayerAttack();
            Damage();
        }
        else if(_playerStatus.ContainsValue( true ))
        {
            int statusName = 0;
            foreach(KeyValuePair<int, bool> i in _playerStatus)
            {
                if (i.Value)
                {
                    statusName = i.Key;
                    break;
                }
            }

            if(statusName == Dissolve)
            {
                PlayerDissolve();
            }
            else if(statusName == Attack)
            {
                PlayerAttack();
            }
            else if(statusName == Surprised)
            {
                // nothing method
            }
        }

        // Dissolve
        if(_hp <= 0 && !_dissolveFlg)
        {
            _animator.CrossFade(_dissolveState, 0.1f, 0, 0);
            _dissolveFlg = true;
        }
        // processing at respawn
        else if(_hp == MaxHp && _dissolveFlg)
        {
            _dissolveFlg = false;
        }
    }

    //---------------------------------------------------------------------
    // character status
    //---------------------------------------------------------------------
    const int Dissolve = 1;
    const int Attack = 2;
    const int Surprised = 3;
    readonly Dictionary<int, bool> _playerStatus = new()
    {
        {Dissolve, false },
        {Attack, false },
        {Surprised, false },
    };
    static readonly int _dissolve = Shader.PropertyToID("_Dissolve");

    //------------------------------
    void Status ()
    {
        // during dissolve
        if(_dissolveFlg && _hp <= 0)
        {
            _playerStatus[Dissolve] = true;
        }
        else if(!_dissolveFlg)
        {
            _playerStatus[Dissolve] = false;
        }
        
        // during attacking
        if(_animator.GetCurrentAnimatorStateInfo(0).tagHash == _attackTag)
        {
            _playerStatus[Attack] = true;
        }
        else if(_animator.GetCurrentAnimatorStateInfo(0).tagHash != _attackTag)
        {
            _playerStatus[Attack] = false;
        }
        
        // during damaging
        if(_animator.GetCurrentAnimatorStateInfo(0).fullPathHash == _surprisedState)
        {
            _playerStatus[Surprised] = true;
        }
        else if(_animator.GetCurrentAnimatorStateInfo(0).fullPathHash != _surprisedState)
        {
            _playerStatus[Surprised] = false;
        }
    }

    // dissolve shading
    void PlayerDissolve ()
    {
        _dissolveValue -= Time.deltaTime;
        foreach (SkinnedMeshRenderer mesh in MeshR)
            mesh.material.SetFloat(_dissolve, _dissolveValue);

        if(_dissolveValue <= 0)
        {
            _characterController.enabled = false;
        }
    }
    
    // play a animation of Attack
    void PlayerAttack ()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            _animator.CrossFade(_attackState,0.1f,0,0);
        }
    }
    
    //---------------------------------------------------------------------
    // gravity for fall of this character
    //---------------------------------------------------------------------
    void Gravity ()
    {
        if(_characterController.enabled)
        {
            if(CheckGrounded())
            {
                if(_moveDirection.y < -0.1f)
                {
                    _moveDirection.y = -0.1f;
                }
            }
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
        {
            return true;
        }
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        float range = 0.2f;
        return Physics.Raycast(ray, range);
    }
    
    //---------------------------------------------------------------------
    // for slime moving
    //---------------------------------------------------------------------
    void Move ()
    {
        // velocity
        if(_animator.GetCurrentAnimatorStateInfo(0).fullPathHash == _moveState)
        {
            if (Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow) && !Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
                MOVE_Velocity(new Vector3(0, 0, -Speed), new Vector3(0, 180, 0));
            else if (Input.GetKey(KeyCode.DownArrow) && !Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
                MOVE_Velocity(new Vector3(0, 0, Speed), new Vector3(0, 0, 0));
            else if (Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow) && !Input.GetKey(KeyCode.RightArrow))
                MOVE_Velocity(new Vector3(Speed, 0, 0), new Vector3(0, 90, 0));
            else if (Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow) && !Input.GetKey(KeyCode.LeftArrow))
                MOVE_Velocity(new Vector3(-Speed, 0, 0), new Vector3(0, 270, 0));
        }
        
        // key down
        if (Input.GetKeyDown(KeyCode.UpArrow) 
            || Input.GetKeyDown(KeyCode.DownArrow)
            || Input.GetKeyDown(KeyCode.LeftArrow)
            || Input.GetKeyDown(KeyCode.RightArrow))
            _animator.CrossFade(_moveState, 0.1f, 0, 0);
        
        KeyUp();
    }

    //---------------------------------------------------------------------
    // value for moving
    //---------------------------------------------------------------------
    void MOVE_Velocity (Vector3 velocity, Vector3 rot)
    {
        _moveDirection = new Vector3 (velocity.x, _moveDirection.y, velocity.z);
        if(_characterController.enabled)
        {
            _characterController.Move(_moveDirection * Time.deltaTime);
        }
        _moveDirection.x = 0;
        _moveDirection.z = 0;
        transform.rotation = Quaternion.Euler(rot);
    }
    
    //---------------------------------------------------------------------
    // whether arrow key is key up
    //---------------------------------------------------------------------
    void KeyUp ()
    {
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            if(!Input.GetKey(KeyCode.DownArrow) && !Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
            {
                _animator.CrossFade(_idleState, 0.1f, 0, 0);
            }
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            if(!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
            {
                _animator.CrossFade(_idleState, 0.1f, 0, 0);
            }
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            if(!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow) && !Input.GetKey(KeyCode.RightArrow))
            {
                _animator.CrossFade(_idleState, 0.1f, 0, 0);
            }
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            if(!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow) && !Input.GetKey(KeyCode.LeftArrow))
            {
                _animator.CrossFade(_idleState, 0.1f, 0, 0);
            }
        }
    }
    
    //---------------------------------------------------------------------
    // damage
    //---------------------------------------------------------------------
    void Damage ()
    {
        // Damaged by outside field.
        if(Input.GetKeyUp(KeyCode.S))
        {
            _animator.CrossFade(_surprisedState, 0.1f, 0, 0);
            Hp--;
        }
    }
    
    //---------------------------------------------------------------------
    // respawn
    //---------------------------------------------------------------------
    void Respawn ()
    {
        // player HP
        _hp = MaxHp;
        
        _characterController.enabled = false;
        transform.position = Vector3.zero; // player position
        transform.rotation = Quaternion.Euler(Vector3.zero); // player facing
        _characterController.enabled = true;
        
        // reset Dissolve
        _dissolveValue = 1;
        foreach (SkinnedMeshRenderer mesh in MeshR)
            mesh.material.SetFloat(_dissolve, _dissolveValue);

        // reset animation
        _animator.CrossFade(_idleState, 0.1f, 0, 0);
    }
}
