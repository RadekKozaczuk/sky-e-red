using UnityEngine;
using UnityEngine.InputSystem;

class PlayerInputSystem : MonoBehaviour
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
            //Debug.Log("Movement Down");

            SceneReferenceHolder.Player.MovementStarted();
        };

        _movementAction.canceled += _ =>
        {
            _movementDown = false;
            //Debug.Log("Movement Up");
            
            SceneReferenceHolder.Player.MovementStopped();
        };

        _attackAction = gameplay.FindAction(Attack);

        // player attack actions
        _attackAction.performed += _ =>
        {
            //Debug.Log("Attack Up");
        };

        _attackAction.canceled += _ =>
        {
            //Debug.Log("Attack Down");
        };
    }

    void Update()
    {
        if (_movementDown)
        {
            SceneReferenceHolder.Player.SetMovementVector(_movementAction.ReadValue<Vector2>());
        }
    }
}