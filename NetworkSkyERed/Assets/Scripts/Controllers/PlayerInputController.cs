using UnityEngine;
using UnityEngine.InputSystem;

class PlayerInputController : MonoBehaviour
{
    const string PlayerActionMap = "Player";
    const string Move = "Move";
    const string Attack = "Attack";

    InputAction _movementAction;
    InputAction _attackAction;

    [SerializeField]
    InputActionAsset _inputActionAsset;

    /// <summary>
    /// True if movement action is pressed down.
    /// </summary>
    static bool _movementDown;

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

            //SceneReferenceHolder.Character.MovementStarted();
        };

        _movementAction.canceled += _ =>
        {
            _movementDown = false;
            //SceneReferenceHolder.Character.MovementStopped();
        };

        _attackAction = gameplay.FindAction(Attack);

        // player attack actions
        _attackAction.performed += _ =>
        {
            GameController.Singleton.Attack();
        };
    }

    void Update()
    {
        if (_movementDown)
        {
            var move = _movementAction.ReadValue<Vector2>();
            GameController.Singleton.Move(move);
        }
    }
}