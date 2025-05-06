using System;
using UnityEngine;

public class Player : MonoBehaviour, IItemHolder
{
    // Player hover event
    public event EventHandler<OnPlayerHoverItemEventArgs> OnPlayerHoverItem;
    public class OnPlayerHoverItemEventArgs : EventArgs
    {
        public Item item;
        public bool isHovered;
    }

    // Singleton
    public static Player Instance { get; private set; }

    // References
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform itemHoldPoint; // Point where item will follow if picked up
    [SerializeField] public Item hoveredItem;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private ContainerUI inventoryUI;
    [SerializeField] private FridgeVisual fridgeVisual;

    // Movement parameters
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float fallMultiplier = 2f;
    [SerializeField] private float terminalVelocity = -20f;
    [SerializeField] private float jumpHeight = 2f;

    // State
    [SerializeField] private bool isPaused = false;
    private Transform lastHitTransform = null;
    private Vector3 velocity;
    private RaycastHit currentHit;
    private float xRotation = 0f;
    private bool hasHit;
    private Item heldItem; // Current item being held by the player

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Player already exists.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        // Keep your existing Start() code

        // Initialize container UI system
        if (ContainerUIManager.Instance != null && PlayerInventory.Instance != null)
        {
            ContainerUIManager.Instance.Initialize(
                PlayerInventory.Instance,
                PlayerInventory.Instance.GetToolbar()
            );
        }
        // Subscribe to events
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnJumpAction += GameInput_OnJumpAction;
        GameInput.Instance.OnTestAction += GameInput_OnTestAction;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnInteractAction -= GameInput_OnInteractAction;
            GameInput.Instance.OnPauseAction -= GameInput_OnPauseAction;
            GameInput.Instance.OnJumpAction -= GameInput_OnJumpAction;
            GameInput.Instance.OnTestAction -= GameInput_OnTestAction;
        }
    }

    private void Update()
    {
        if (isPaused) return;

        // Unified raycast
        hasHit = Physics.Raycast(cameraTransform.position, cameraTransform.forward, out currentHit, interactRange);

        HandleLooking();
        HandleHovering();
        HandleGravity();
        HandleMovement();

        Debug.DrawRay(cameraTransform.position, cameraTransform.forward * interactRange, Color.yellow);
    }

    #region Event Handlers
    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (isPaused) return;

        if (hoveredItem != null)
        {
            // Try to add item to inventory
            ItemSO itemSO = hoveredItem.GetItemSO();
            bool added = inventoryManager.AddItem(itemSO);

            if (added)
            {
                // Successfully added to inventory, destroy the item
                Destroy(hoveredItem.gameObject);
                hoveredItem = null;
            }
        }
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePause();
    }

    private void GameInput_OnJumpAction(object sender, EventArgs e)
    {
        if (isPaused) return;

        if (characterController.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void GameInput_OnTestAction(object sender, EventArgs e)
    {
        if (isPaused) return;

        Debug.Log("Test pressed!");
        if (fridgeVisual != null)
        {
            fridgeVisual.ToggleDoor();
        }
    }
    #endregion

    #region Movement and Input Handling
    private void HandleLooking()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -89f, 89f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void HandleMovement()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();

        Vector3 moveDir = transform.right * inputVector.x + transform.forward * inputVector.y;
        Vector3 horizontalMove = moveDir * moveSpeed;

        Vector3 fullMove = horizontalMove + Vector3.up * velocity.y;

        characterController.Move(fullMove * Time.deltaTime);
    }

    private void HandleGravity()
    {
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        else
        {
            float currentGravity = gravity;
            if (velocity.y < 0)
            {
                currentGravity *= fallMultiplier;
            }

            velocity.y += currentGravity * Time.deltaTime;
            velocity.y = Mathf.Max(velocity.y, terminalVelocity);
        }
    }
    #endregion

    #region Interaction and Item Handling
    private void HandleHovering()
    {
        if (hasHit && currentHit.transform != lastHitTransform)
        {
            lastHitTransform = currentHit.transform;

            if (currentHit.transform.TryGetComponent(out Item hitItem))
            {
                SetHoveredItem(hitItem);
            }
            else
            {
                SetHoveredItem(null);
            }
        }
        else if (!hasHit && hoveredItem != null)
        {
            lastHitTransform = null;
            SetHoveredItem(null);
        }
    }

    private void SetHoveredItem(Item newHoveredItem)
    {
        if (newHoveredItem == hoveredItem) return; // No change

        // Fire event to disable old one
        if (hoveredItem != null)
        {
            OnPlayerHoverItem?.Invoke(this, new OnPlayerHoverItemEventArgs
            {
                item = hoveredItem,
                isHovered = false
            });
        }

        // Fire event to enable new one
        hoveredItem = newHoveredItem;

        if (hoveredItem != null)
        {
            OnPlayerHoverItem?.Invoke(this, new OnPlayerHoverItemEventArgs
            {
                item = hoveredItem,
                isHovered = true
            });
        }
    }
    #endregion

    #region IItemHolder Implementation
    public Transform GetItemFollowTransform()
    {
        return itemHoldPoint;
    }

    public void SetItem(Item item)
    {
        heldItem = item;
    }

    public Item GetItem()
    {
        return heldItem;
    }

    public void ClearItem()
    {
        heldItem = null;
    }

    public bool HasItem()
    {
        return heldItem != null;
    }
    #endregion

    #region Game State
    private void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            // Show cursor and inventory
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            inventoryUI.Show();
        }
        else
        {
            // Hide cursor and inventory
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            inventoryUI.Hide();
        }
    }
    #endregion
}