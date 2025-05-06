using System.Collections.Generic;
using UnityEngine;

// Manager for all container UIs
public class ContainerUIManager : MonoBehaviour
{
    public static ContainerUIManager Instance { get; private set; }

    [SerializeField] private ContainerUI playerInventoryUI;
    [SerializeField] private ToolbarUI toolbarUI;
    [SerializeField] private ContainerUI fridgeUI;
    [SerializeField] private ContainerUI pantryUI;

    // References to containers
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private ToolbarContainer toolbarContainer;

    // Keep track of active container UIs
    private Dictionary<IItemContainer, ContainerUI> activeContainerUIs = new Dictionary<IItemContainer, ContainerUI>();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple ContainerUIManager instances found");
            Destroy(gameObject);
            return;
        }
        Instance = this;


        // Hide UIs initially
        if (playerInventoryUI != null)
            playerInventoryUI.gameObject.SetActive(false);

        if (fridgeUI != null)
            fridgeUI.gameObject.SetActive(false);

        if (pantryUI != null)
            pantryUI.gameObject.SetActive(false);
    }

    private void Start()
    {
        // Auto-find references if not set
        if (playerInventory == null)
        {
            playerInventory = FindObjectOfType<PlayerInventory>();
            Debug.Log($"Auto-found playerInventory: {playerInventory != null}");
        }

        if (toolbarContainer == null && playerInventory != null)
        {
            toolbarContainer = playerInventory.GetToolbar();
            Debug.Log($"Auto-found toolbarContainer: {toolbarContainer != null}");
        }

        // Initialize with found references
        if (playerInventory != null)
        {
            Initialize(playerInventory, toolbarContainer);
        }
        else
        {
            Debug.LogError("Could not initialize ContainerUIManager - missing PlayerInventory reference");
        }
    }

    public void Initialize(PlayerInventory inventory, ToolbarContainer toolbar)
    {
        playerInventory = inventory;
        toolbarContainer = toolbar;

        // Initialize player inventory UI
        if (playerInventoryUI != null && playerInventory != null)
        {
            playerInventoryUI.Initialize(playerInventory);
            playerInventoryUI.gameObject.SetActive(false);
        }

        // Initialize toolbar UI
        if (toolbarUI != null && toolbarContainer != null)
        {
            toolbarUI.Initialize(toolbarContainer);
            toolbarUI.gameObject.SetActive(true); // Toolbar is always visible
        }
    }

    public void ShowContainerUI(IItemContainer container)
    {
        // Show player inventory whenever another container is opened
        if (container.ContainerType != ContainerType.PlayerInventory)
        {
            playerInventoryUI.gameObject.SetActive(true);
        }

        // Determine which UI to show based on container type
        switch (container.ContainerType)
        {
            case ContainerType.PlayerInventory:
                playerInventoryUI.gameObject.SetActive(true);
                break;

            case ContainerType.Fridge:
                if (fridgeUI != null)
                {
                    fridgeUI.Initialize(container);
                    fridgeUI.gameObject.SetActive(true);
                    activeContainerUIs[container] = fridgeUI;
                }
                break;

            case ContainerType.Pantry:
                if (pantryUI != null)
                {
                    pantryUI.Initialize(container);
                    pantryUI.gameObject.SetActive(true);
                    activeContainerUIs[container] = pantryUI;
                }
                break;
        }
    }

    public void HideContainerUI(IItemContainer container)
    {
        switch (container.ContainerType)
        {
            case ContainerType.PlayerInventory:
                playerInventoryUI.gameObject.SetActive(false);

                // Also hide any other active container UIs
                foreach (var ui in activeContainerUIs.Values)
                {
                    ui.gameObject.SetActive(false);
                }
                activeContainerUIs.Clear();
                break;

            case ContainerType.Fridge:
                if (fridgeUI != null)
                {
                    fridgeUI.gameObject.SetActive(false);
                    activeContainerUIs.Remove(container);
                }
                break;

            case ContainerType.Pantry:
                if (pantryUI != null)
                {
                    pantryUI.gameObject.SetActive(false);
                    activeContainerUIs.Remove(container);
                }
                break;
        }

        // Hide player inventory if no other containers are open
        if (activeContainerUIs.Count == 0 && playerInventoryUI != null)
        {
            playerInventoryUI.gameObject.SetActive(false);
        }
    }

    public void TogglePlayerInventory()
    {
        if (playerInventoryUI != null)
        {
            bool newState = !playerInventoryUI.gameObject.activeSelf;

            if (newState)
            {
                ShowContainerUI(playerInventory);
            }
            else
            {
                HideContainerUI(playerInventory);
            }
        }
    }
}