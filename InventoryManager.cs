using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    // Singleton for easy access
    public static InventoryManager Instance { get; private set; }

    // Event fired whenever inventory contents change
    public event EventHandler OnInventoryChanged;

    [SerializeField] private int inventorySize = 27; // 3x9 grid like Minecraft

    // List of inventory slots
    private List<InventorySlot> inventorySlots;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null)
        {
            Debug.LogError("Multiple instances of InventoryManager found");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Initialize inventory with empty slots
        inventorySlots = new List<InventorySlot>();
        for (int i = 0; i < inventorySize; i++)
        {
            InventorySlot slot = new InventorySlot();
            slot.OnItemChanged += Slot_OnItemChanged;
            inventorySlots.Add(slot);
        }
    }

    // Event handler for when a slot's contents change
    private void Slot_OnItemChanged(object sender, InventorySlot.InventorySlotEventArgs e)
    {
        OnInventoryChanged?.Invoke(this, EventArgs.Empty);
    }

    // Add an item to the inventory
    public bool AddItem(ItemSO itemSO, int amount = 1)
    {
        if (itemSO == null) return false;

        int remainingAmount = amount;

        // First try to stack with existing items of the same type
        if (itemSO.isStackable)
        {
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                if (remainingAmount <= 0) break;

                InventorySlot slot = inventorySlots[i];
                if (slot.GetItemSO() == itemSO && slot.CanAddItem(itemSO))
                {
                    bool addedAll = slot.AddItem(itemSO, remainingAmount);
                    if (addedAll)
                    {
                        // All items were added
                        return true;
                    }
                    else
                    {
                        // Only some were added, calculate how many
                        int spaceAvailable = itemSO.maxStackSize - slot.GetQuantity() + remainingAmount;
                        remainingAmount -= spaceAvailable;
                    }
                }
            }
        }

        // If we still have items to add, find empty slots
        if (remainingAmount > 0)
        {
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                if (remainingAmount <= 0) break;

                InventorySlot slot = inventorySlots[i];
                if (slot.IsEmpty())
                {
                    // Handle stacks larger than max stack size
                    int amountToAdd = Mathf.Min(remainingAmount, itemSO.maxStackSize);
                    slot.AddItem(itemSO, amountToAdd);
                    remainingAmount -= amountToAdd;
                }
            }
        }

        // Return true if we added all items
        return remainingAmount <= 0;
    }

    // Remove an item from a specific slot
    public ItemSO RemoveItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySlots.Count)
            return null;

        return inventorySlots[slotIndex].RemoveOne();
    }

    // Swap or combine items between two slots
    public bool SwapItems(int fromSlotIndex, int toSlotIndex)
    {
        if (fromSlotIndex < 0 || fromSlotIndex >= inventorySlots.Count ||
            toSlotIndex < 0 || toSlotIndex >= inventorySlots.Count)
            return false;

        // Make copies of both slots
        InventorySlot fromSlotCopy = inventorySlots[fromSlotIndex].Copy();
        InventorySlot toSlotCopy = inventorySlots[toSlotIndex].Copy();

        // Try to combine stacks if same item
        if (!fromSlotCopy.IsEmpty() && !toSlotCopy.IsEmpty() &&
            fromSlotCopy.GetItemSO() == toSlotCopy.GetItemSO() &&
            fromSlotCopy.GetItemSO().isStackable)
        {
            // Try to add fromSlot items to toSlot
            int transferAmount = Mathf.Min(
                fromSlotCopy.GetQuantity(),
                toSlotCopy.GetItemSO().maxStackSize - toSlotCopy.GetQuantity()
            );

            if (transferAmount > 0)
            {
                // Transfer some or all items
                inventorySlots[toSlotIndex].AddItem(fromSlotCopy.GetItemSO(), transferAmount);
                inventorySlots[fromSlotIndex].RemoveAmount(transferAmount);
                return true;
            }
        }

        // If combination wasn't possible or didn't work, do a straight swap
        inventorySlots[fromSlotIndex].ClearSlot();
        inventorySlots[toSlotIndex].ClearSlot();

        if (!fromSlotCopy.IsEmpty())
        {
            inventorySlots[toSlotIndex].AddItem(fromSlotCopy.GetItemSO(), fromSlotCopy.GetQuantity());
        }

        if (!toSlotCopy.IsEmpty())
        {
            inventorySlots[fromSlotIndex].AddItem(toSlotCopy.GetItemSO(), toSlotCopy.GetQuantity());
        }

        return true;
    }

    // Get all inventory slots
    public List<InventorySlot> GetInventorySlots()
    {
        return inventorySlots;
    }

    // Get a specific inventory slot
    public InventorySlot GetSlot(int index)
    {
        if (index < 0 || index >= inventorySlots.Count)
            return null;

        return inventorySlots[index];
    }
}