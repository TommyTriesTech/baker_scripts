using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public class SlotClickedEventArgs : EventArgs
    {
        public int slotIndex;
        public InventorySlot slot;
    }

    public class SlotDragEventArgs : EventArgs
    {
        public int slotIndex;
        public InventorySlot slot;
        public InventorySlotUI slotUI;
        public Vector2 position;
    }

    public event EventHandler<SlotClickedEventArgs> OnSlotClicked;
    public event EventHandler<SlotDragEventArgs> OnSlotBeginDrag;
    public event EventHandler<SlotDragEventArgs> OnSlotDrag;
    public event EventHandler<SlotDragEventArgs> OnSlotEndDrag;

    [SerializeField] private Image itemIcon;
    [SerializeField] private Image slotBackground;
    [SerializeField] private TextMeshProUGUI quantityText;

    private InventorySlot inventorySlot;
    private int slotIndex;

    public void Initialize(InventorySlot slot, int index)
    {
        inventorySlot = slot;
        slotIndex = index;

        // Subscribe to slot change events
        inventorySlot.OnItemChanged += InventorySlot_OnItemChanged;

        RefreshUI();
    }

    public void RefreshUI()
    {
        // Add null checks to prevent errors if components are missing
        if (inventorySlot == null) return;

        if (inventorySlot.IsEmpty())
        {
            // Empty slot
            if (itemIcon != null) itemIcon.enabled = false;
            if (quantityText != null) quantityText.enabled = false;
        }
        else
        {
            // Slot with item
            ItemSO itemSO = inventorySlot.GetItemSO();

            if (itemIcon != null)
            {
                itemIcon.enabled = true;
                if (itemSO != null && itemSO.sprite != null)
                {
                    itemIcon.sprite = itemSO.sprite;
                }
                else
                {
                    Debug.LogWarning($"Missing sprite for item in slot {slotIndex}");
                }
            }

            // Show quantity for stacks > 1
            int quantity = inventorySlot.GetQuantity();
            if (quantityText != null)
            {
                quantityText.enabled = quantity > 1;
                quantityText.text = quantity.ToString();
            }
        }
    }

    private void InventorySlot_OnItemChanged(object sender, InventorySlot.InventorySlotEventArgs e)
    {
        RefreshUI();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnSlotClicked?.Invoke(this, new SlotClickedEventArgs
        {
            slotIndex = slotIndex,
            slot = inventorySlot
        });
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (inventorySlot.IsEmpty()) return;

        OnSlotBeginDrag?.Invoke(this, new SlotDragEventArgs
        {
            slotIndex = slotIndex,
            slot = inventorySlot,
            slotUI = this,
            position = eventData.position
        });
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (inventorySlot.IsEmpty()) return;

        OnSlotDrag?.Invoke(this, new SlotDragEventArgs
        {
            slotIndex = slotIndex,
            slot = inventorySlot,
            slotUI = this,
            position = eventData.position
        });
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnSlotEndDrag?.Invoke(this, new SlotDragEventArgs
        {
            slotIndex = slotIndex,
            slot = inventorySlot,
            slotUI = this,
            position = eventData.position
        });
    }

    public int GetSlotIndex()
    {
        return slotIndex;
    }

    public InventorySlot GetInventorySlot()
    {
        return inventorySlot;
    }

}