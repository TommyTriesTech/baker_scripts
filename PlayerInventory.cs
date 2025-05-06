using UnityEngine;

public class PlayerInventory : BaseItemContainer
{
    public static PlayerInventory Instance { get; private set; }

    [Header("Debug")]
    [SerializeField]
    private ItemSO[] testItems;

    [SerializeField] private ToolbarContainer toolbarReference;

    protected override void Awake()
    {
        // Singleton check
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple PlayerInventory instances found");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        containerName = "Player Inventory";
        containerType = ContainerType.PlayerInventory;

        base.Awake();
    }

    // Access the player's toolbar
    public ToolbarContainer GetToolbar()
    {
        return toolbarReference;
    }

    // Special method to auto-equip items to toolbar when appropriate
    public bool AddItemWithAutoEquip(ItemSO itemSO, int amount = 1)
    {
        // For non-stackable tools, try to add to toolbar first if there's space
        if (!itemSO.isStackable && toolbarReference != null)
        {
            if (toolbarReference.CanAddItem(itemSO))
            {
                return toolbarReference.AddItem(itemSO);
            }
        }

        // Otherwise add to normal inventory
        return AddItem(itemSO, amount);
    }
}