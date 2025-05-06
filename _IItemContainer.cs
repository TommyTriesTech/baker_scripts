using System;
using System.Collections.Generic;
using UnityEngine;

// Define container types
public enum ContainerType
{
    PlayerInventory,
    Toolbar,
    Fridge,
    Pantry,
    GenericContainer
}

// Interface for all item containers
public interface IItemContainer
{
    // Core properties
    string ContainerName { get; }
    int SlotCount { get; }
    ContainerType ContainerType { get; }

    // Events
    event EventHandler OnContainerChanged;

    // Core methods
    List<InventorySlot> GetAllSlots();
    InventorySlot GetSlot(int index);
    bool CanAddItem(ItemSO itemSO, int amount = 1);
    bool AddItem(ItemSO itemSO, int amount = 1);
    ItemSO RemoveItem(int slotIndex, int amount = 1);
    bool SwapItems(int fromSlotIndex, int toSlotIndex);

    // Cross-container functionality
    bool TransferItemTo(IItemContainer targetContainer, int fromSlotIndex, int toSlotIndex);
}