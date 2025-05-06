using UnityEngine;

public interface IItemHolder
{
    public Transform GetItemFollowTransform();

    public void SetItem(Item item);

    public Item GetItem();

    public void ClearItem();

    public bool HasItem();
}