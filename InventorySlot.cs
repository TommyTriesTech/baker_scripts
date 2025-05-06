using System;
using UnityEngine;

[Serializable]
public class InventorySlot
{
    public event EventHandler<InventorySlotEventArgs> OnItemChanged;

    public class InventorySlotEventArgs : EventArgs
    {
        public ItemSO itemSO;
        public int quantity;
    }

    [SerializeField] private ItemSO itemSO;
    [SerializeField] private int quantity;

    public InventorySlot()
    {
        ClearSlot();
    }

    public bool CanAddItem(ItemSO newItemSO)
    {
        if (IsEmpty())
        {
            // Empty slot can always accept items
            return true;
        }

        if (itemSO == newItemSO && itemSO.isStackable)
        {
            // Check if we can add more to this stack
            return quantity < itemSO.maxStackSize;
        }

        return false;
    }

    public bool AddItem(ItemSO newItemSO, int amountToAdd = 1)
    {
        if (IsEmpty())
        {
            // Empty slot - add new item
            itemSO = newItemSO;
            quantity = amountToAdd;
            InvokeItemChangedEvent();
            return true;
        }

        if (itemSO == newItemSO && itemSO.isStackable && quantity < itemSO.maxStackSize)
        {
            // Same item and can stack more
            int spaceAvailable = itemSO.maxStackSize - quantity;
            int actualAmountToAdd = Mathf.Min(amountToAdd, spaceAvailable);

            quantity += actualAmountToAdd;
            InvokeItemChangedEvent();

            return actualAmountToAdd == amountToAdd;
        }

        return false;
    }

    public ItemSO GetItemSO()
    {
        return itemSO;
    }

    public int GetQuantity()
    {
        return quantity;
    }

    public bool IsEmpty()
    {
        return itemSO == null || quantity <= 0;
    }

    public ItemSO RemoveOne()
    {
        if (IsEmpty())
            return null;

        ItemSO removedItemSO = itemSO;
        quantity--;

        if (quantity <= 0)
        {
            ClearSlot();
        }
        else
        {
            InvokeItemChangedEvent();
        }

        return removedItemSO;
    }

    public ItemSO RemoveAmount(int amountToRemove)
    {
        if (IsEmpty() || amountToRemove <= 0)
            return null;

        ItemSO removedItemSO = itemSO;
        quantity -= amountToRemove;

        if (quantity <= 0)
        {
            ClearSlot();
        }
        else
        {
            InvokeItemChangedEvent();
        }

        return removedItemSO;
    }

    public void ClearSlot()
    {
        itemSO = null;
        quantity = 0;
        InvokeItemChangedEvent();
    }

    public InventorySlot Copy()
    {
        InventorySlot copy = new InventorySlot();
        copy.itemSO = this.itemSO;
        copy.quantity = this.quantity;
        return copy;
    }

    private void InvokeItemChangedEvent()
    {
        OnItemChanged?.Invoke(this, new InventorySlotEventArgs
        {
            itemSO = itemSO,
            quantity = quantity
        });
    }
}