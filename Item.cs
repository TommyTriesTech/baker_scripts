using System;
using UnityEngine;

public class Item : MonoBehaviour
{
    public static event Action<Item, IItemHolder> OnItemInteract;

    public ItemSO itemSO;

    private IItemHolder itemHolder;

    public ItemSO GetItemSO()
    {
        return itemSO;
    }

    public void Interact(IItemHolder itemHolder)
    {
        SetItemParent(itemHolder);
        OnItemInteract?.Invoke(this, itemHolder);
        Destroy(gameObject);
    }

    public void SetItemParent(IItemHolder itemHolder)
    {
        this.itemHolder = itemHolder;

        if (itemHolder.HasItem())
        {
            Debug.LogError("Item Holder already has an item");
        }

        itemHolder.SetItem(this);
    }

    public IItemHolder GetItemHolderParent()
    {
        return itemHolder;
    }

    public void DestroySelf()
    {
        itemHolder.ClearItem();
        Destroy(gameObject);
    }
}
