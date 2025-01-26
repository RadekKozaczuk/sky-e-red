using UnityEngine;
using UnityEngine.InputSystem;

class PlayerInputController : MonoBehaviour
{
    const string PlayerActionMap = "Player";
    const string Move = "Move";
    const string Attack = "Attack";
    const string ChangeCharacter = "ChangeCharacter";

    InputAction _movementAction;
    InputAction _attackAction;
    InputAction _changeCharacter;

    [SerializeField]
    InputActionAsset _inputActionAsset;

    /// <summary>
    /// True if movement action is pressed down.
    /// </summary>
    static bool _movementDown;

    Vector2 _lastMovement;

    void Awake()
    {
        InputActionMap gameplay = _inputActionAsset.FindActionMap(PlayerActionMap);

        // movement actions
        _movementAction = gameplay.FindAction(Move);
        _movementAction.performed += _ =>
        {
            // to prevent calling method too many times
            if (_movementDown)
                return;

            _movementDown = true;
        };

        _movementAction.canceled += _ =>
        {
            _movementDown = false;
            GameController.Singleton.Move(Vector2.zero);
        };

        // attack actions
        _attackAction = gameplay.FindAction(Attack);
        _attackAction.performed += _ => GameController.Singleton.Attack();
        
        // change character actions
        _changeCharacter = gameplay.FindAction(ChangeCharacter);
        _changeCharacter.performed += _ => GameController.Singleton.ChangeCharacter();
    }

    void Update()
    {
        if (!_movementDown)
            return;

        var move = _movementAction.ReadValue<Vector2>();
            
        // send information only when it's different
        if (move != _lastMovement)
            GameController.Singleton.Move(move);

        _lastMovement = move;
    }
}