using System;
using System.Collections.Generic;
using UnityEngine;

// Base container implementation that can be inherited by specific container types
public abstract class BaseItemContainer : MonoBehaviour, IItemContainer
{
    [SerializeField] protected string containerName;
    [SerializeField] protected int slotCount;
    [SerializeField] protected ContainerType containerType;

    protected List<InventorySlot> slots = new List<InventorySlot>();

    public event EventHandler OnContainerChanged;

    public string ContainerName => containerName;
    public int SlotCount => slotCount;
    public ContainerType ContainerType => containerType;

    protected virtual void Awake()
    {
        // Initialize slots
        InitializeSlots();
    }

    protected virtual void InitializeSlots()
    {
        slots.Clear();
        for (int i = 0; i < slotCount; i++)
        {
            InventorySlot slot = new InventorySlot();
            slot.OnItemChanged += Slot_OnItemChanged;
            slots.Add(slot);
        }
    }

    // Event handler for when a slot changes
    protected virtual void Slot_OnItemChanged(object sender, InventorySlot.InventorySlotEventArgs e)
    {
        // Notify listeners that container has changed
        OnContainerChanged?.Invoke(this, EventArgs.Empty);
    }

    // Default implementations of interface methods
    public virtual List<InventorySlot> GetAllSlots() => slots;

    public virtual InventorySlot GetSlot(int index)
    {
        if (index < 0 || index >= slots.Count)
            return null;
        return slots[index];
    }

    public virtual bool CanAddItem(ItemSO itemSO, int amount = 1)
    {
        if (itemSO == null) return false;

        // First check if we can stack with existing items
        if (itemSO.isStackable)
        {
            int remainingAmount = amount;

            foreach (var slot in slots)
            {
                if (slot.GetItemSO() == itemSO && slot.CanAddItem(itemSO))
                {
                    int availableSpace = itemSO.maxStackSize - slot.GetQuantity();
                    remainingAmount -= availableSpace;

                    if (remainingAmount <= 0)
                        return true;
                }
            }
        }

        // Then check for empty slots
        int emptySlots = 0;
        foreach (var slot in slots)
        {
            if (slot.IsEmpty())
                emptySlots++;
        }

        // Calculate required slots based on stack size
        int requiredSlots = itemSO.isStackable
            ? Mathf.CeilToInt((float)amount / itemSO.maxStackSize)
            : amount;

        return emptySlots >= requiredSlots;
    }

    public virtual bool AddItem(ItemSO itemSO, int amount = 1)
    {
        if (itemSO == null) return false;

        int remainingAmount = amount;

        // First try to stack with existing items
        if (itemSO.isStackable)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (remainingAmount <= 0) break;

                InventorySlot slot = slots[i];
                if (slot.GetItemSO() == itemSO && slot.CanAddItem(itemSO))
                {
                    bool addedAll = slot.AddItem(itemSO, remainingAmount);
                    if (addedAll)
                        return true;
                    else
                    {
                        int added = itemSO.maxStackSize - (slot.GetQuantity() - remainingAmount);
                        remainingAmount -= added;
                    }
                }
            }
        }

        // If we have items left to add, find empty slots
        if (remainingAmount > 0)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (remainingAmount <= 0) break;

                InventorySlot slot = slots[i];
                if (slot.IsEmpty())
                {
                    // If not stackable, only add 1 per slot
                    int amountToAdd = itemSO.isStackable
                        ? Mathf.Min(remainingAmount, itemSO.maxStackSize)
                        : 1;

                    slot.AddItem(itemSO, amountToAdd);
                    remainingAmount -= amountToAdd;
                }
            }
        }

        return remainingAmount <= 0;
    }

    public virtual ItemSO RemoveItem(int slotIndex, int amount = 1)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count)
            return null;

        if (amount <= 1)
            return slots[slotIndex].RemoveOne();
        else
            return slots[slotIndex].RemoveAmount(amount);
    }

    public virtual bool SwapItems(int fromSlotIndex, int toSlotIndex)
    {
        if (fromSlotIndex < 0 || fromSlotIndex >= slots.Count ||
            toSlotIndex < 0 || toSlotIndex >= slots.Count)
            return false;

        InventorySlot fromSlot = slots[fromSlotIndex];
        InventorySlot toSlot = slots[toSlotIndex];

        // Make copies
        InventorySlot fromCopy = fromSlot.Copy();
        InventorySlot toCopy = toSlot.Copy();

        // Try to combine if same item and stackable
        if (!fromCopy.IsEmpty() && !toCopy.IsEmpty() &&
            fromCopy.GetItemSO() == toCopy.GetItemSO() &&
            fromCopy.GetItemSO().isStackable)
        {
            int transferAmount = Mathf.Min(
                fromCopy.GetQuantity(),
                toCopy.GetItemSO().maxStackSize - toCopy.GetQuantity()
            );

            if (transferAmount > 0)
            {
                toSlot.AddItem(fromCopy.GetItemSO(), transferAmount);
                fromSlot.RemoveAmount(transferAmount);
                return true;
            }
        }

        // Otherwise just swap
        fromSlot.ClearSlot();
        toSlot.ClearSlot();

        if (!fromCopy.IsEmpty())
            toSlot.AddItem(fromCopy.GetItemSO(), fromCopy.GetQuantity());

        if (!toCopy.IsEmpty())
            fromSlot.AddItem(toCopy.GetItemSO(), toCopy.GetQuantity());

        return true;
    }

    public virtual bool TransferItemTo(IItemContainer targetContainer, int fromSlotIndex, int toSlotIndex)
    {
        if (fromSlotIndex < 0 || fromSlotIndex >= slots.Count)
            return false;

        InventorySlot fromSlot = GetSlot(fromSlotIndex);
        InventorySlot toSlot = targetContainer.GetSlot(toSlotIndex);

        if (fromSlot == null || toSlot == null || fromSlot.IsEmpty())
            return false;

        ItemSO itemToTransfer = fromSlot.GetItemSO();
        int amountToTransfer = fromSlot.GetQuantity();

        // Special case for stacking with same item
        if (!toSlot.IsEmpty() && toSlot.GetItemSO() == itemToTransfer && itemToTransfer.isStackable)
        {
            int spaceAvailable = itemToTransfer.maxStackSize - toSlot.GetQuantity();
            int actualAmountToTransfer = Mathf.Min(amountToTransfer, spaceAvailable);

            if (actualAmountToTransfer > 0)
            {
                toSlot.AddItem(itemToTransfer, actualAmountToTransfer);
                fromSlot.RemoveAmount(actualAmountToTransfer);
                return true;
            }
            else if (spaceAvailable <= 0)
            {
                // Stack is full, do a normal swap
                return SwapWithExternalContainer(targetContainer, fromSlotIndex, toSlotIndex);
            }
        }
        else if (toSlot.IsEmpty())
        {
            // Target slot is empty, just transfer
            toSlot.AddItem(itemToTransfer, amountToTransfer);
            fromSlot.ClearSlot();
            return true;
        }
        else
        {
            // Different items, do a swap
            return SwapWithExternalContainer(targetContainer, fromSlotIndex, toSlotIndex);
        }

        return false;
    }

    // Helper for external container swaps
    protected virtual bool SwapWithExternalContainer(IItemContainer otherContainer, int ourSlotIndex, int theirSlotIndex)
    {
        InventorySlot ourSlot = GetSlot(ourSlotIndex);
        InventorySlot theirSlot = otherContainer.GetSlot(theirSlotIndex);

        if (ourSlot == null || theirSlot == null)
            return false;

        // Make copies
        InventorySlot ourCopy = ourSlot.Copy();
        InventorySlot theirCopy = theirSlot.Copy();

        // Clear both slots
        ourSlot.ClearSlot();
        theirSlot.ClearSlot();

        // Fill with opposite contents
        if (!ourCopy.IsEmpty())
            theirSlot.AddItem(ourCopy.GetItemSO(), ourCopy.GetQuantity());

        if (!theirCopy.IsEmpty())
            ourSlot.AddItem(theirCopy.GetItemSO(), theirCopy.GetQuantity());

        return true;
    }
}