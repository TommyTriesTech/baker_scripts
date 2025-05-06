using System;
using UnityEngine;

// Add this component to your existing Player class
[RequireComponent(typeof(Player))]
public class PlayerInventoryIntegration : MonoBehaviour
{
    // References to containers
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private ToolbarContainer toolbar;

    // UI Manager reference
    [SerializeField] private ContainerUIManager containerUIManager;

    // Interaction variables
    [SerializeField] private float interactRange = 3f;
    private Transform cameraTransform;
    private Player playerReference;
    private IInteractable hoveredInteractable;

    private void Awake()
    {
        playerReference = GetComponent<Player>();
        cameraTransform = Camera.main.transform;

        if (playerInventory == null)
        {
            Debug.LogError("PlayerInventory reference missing!");
        }

        if (toolbar == null)
        {
            Debug.LogError("ToolbarContainer reference missing!");
        }
    }

    private void Start()
    {
        // Initialize UI system
        if (containerUIManager != null)
        {
            containerUIManager.Initialize(playerInventory, toolbar);
        }

        // Subscribe to input events for inventory toggle
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnPauseAction -= GameInput_OnPauseAction;
            GameInput.Instance.OnInteractAction -= GameInput_OnInteractAction;
        }
    }

    private void Update()
    {
        CheckForInteractables();
    }

    private void CheckForInteractables()
    {
        // Check what the player is looking at
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, interactRange))
        {
            // Check for interactable objects like fridges and pantries
            IInteractable interactable = hit.transform.GetComponent<IInteractable>();
            if (interactable != null)
            {
                hoveredInteractable = interactable;
                // TODO: Show interaction prompt
                // UIManager.Instance.ShowInteractPrompt(interactable.GetInteractText());
            }
            else
            {
                hoveredInteractable = null;
                // UIManager.Instance.HideInteractPrompt();
            }
        }
        else
        {
            hoveredInteractable = null;
            // UIManager.Instance.HideInteractPrompt();
        }
    }

    // Input event handlers
    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        // Handle interactable objects first (fridges, pantries, etc.)
        if (hoveredInteractable != null)
        {
            hoveredInteractable.Interact(playerReference);
            return;
        }

        // If nothing else is interacted with, check if we're looking at an item
        if (playerReference.hoveredItem != null)
        {
            HandleItemPickup(playerReference.hoveredItem);
        }
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        // Toggle player inventory UI
        containerUIManager.TogglePlayerInventory();
    }

    // Item handling
    private void HandleItemPickup(Item item)
    {
        ItemSO itemSO = item.GetItemSO();

        // Try auto-equip for tools
        bool added = playerInventory.AddItemWithAutoEquip(itemSO);

        if (added)
        {
            // Successfully added to inventory or toolbar
            Destroy(item.gameObject);
        }
        else
        {
            Debug.Log("Inventory full!");
            // Could show a UI message
        }
    }

    // Helper methods
    public PlayerInventory GetInventory()
    {
        return playerInventory;
    }

    public ToolbarContainer GetToolbar()
    {
        return toolbar;
    }

    // Method to use currently selected tool
    public void UseSelectedToolbarItem()
    {
        if (toolbar == null) return;

        InventorySlot selectedSlot = toolbar.GetSelectedSlot();
        if (selectedSlot == null || selectedSlot.IsEmpty()) return;

        ItemSO selectedItem = selectedSlot.GetItemSO();

        // Handle tool usage
        if (selectedItem is ToolSO toolItem)
        {
            // Use the tool based on its function
            toolItem.UseTool();
            Debug.Log($"Used {selectedItem.itemName}");

            // If broken, maybe remove from inventory
            if (toolItem.IsBroken())
            {
                toolbar.RemoveItem(toolbar.GetSelectedSlotIndex());
                Debug.Log($"{selectedItem.itemName} broke!");
            }
        }
    }
}