using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    // Singleton pattern
    public static GameInput Instance { get; private set; }

    // Events
    public event EventHandler OnInteractAction;
    public event EventHandler OnJumpAction;
    public event EventHandler OnTestAction;
    public event EventHandler OnPauseAction;

    // Input values
    private Vector2 movementInput;

    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null)
        {
            Debug.LogWarning("Found more than one GameInput instance");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Initialize input system
        playerInputActions = new PlayerInputActions();

        // Subscribe to input events
        playerInputActions.Player.Enable();
        playerInputActions.Player.Move.performed += Move_performed;
        playerInputActions.Player.Move.canceled += Move_canceled;
        playerInputActions.Player.Jump.performed += Jump_performed;
        playerInputActions.Player.Interact.performed += Interact_performed;
        playerInputActions.Player.Test.performed += Test_performed;
        playerInputActions.Player.Pause.performed += Pause_performed;
    }

    private void OnDestroy()
    {
        // Clean up subscriptions
        playerInputActions.Player.Move.performed -= Move_performed;
        playerInputActions.Player.Move.canceled -= Move_canceled;
        playerInputActions.Player.Jump.performed -= Jump_performed;
        playerInputActions.Player.Interact.performed -= Interact_performed;
        playerInputActions.Player.Test.performed -= Test_performed;
        playerInputActions.Player.Pause.performed -= Pause_performed;

        playerInputActions.Dispose();
    }

    // Input callbacks
    private void Move_performed(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>().normalized;
    }

    private void Move_canceled(InputAction.CallbackContext context)
    {
        movementInput = Vector2.zero;
    }

    private void Jump_performed(InputAction.CallbackContext context)
    {
        OnJumpAction?.Invoke(this, EventArgs.Empty);
    }

    private void Interact_performed(InputAction.CallbackContext context)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    private void Test_performed(InputAction.CallbackContext context)
    {
        OnTestAction?.Invoke(this, EventArgs.Empty);
    }

    private void Pause_performed(InputAction.CallbackContext context)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    // Public accessor for movement values
    public Vector2 GetMovementVectorNormalized()
    {
        return movementInput;
    }
}