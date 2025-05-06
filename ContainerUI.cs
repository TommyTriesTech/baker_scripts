using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Base UI for any container
public class ContainerUI : MonoBehaviour
{
    [SerializeField] protected Transform slotsParent;
    [SerializeField] protected InventorySlotUI slotUIPrefab;
    [SerializeField] protected RectTransform draggedItemIcon;
    [SerializeField] protected TextMeshProUGUI containerTitleText;

    protected List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();
    protected IItemContainer container;

    protected InventorySlotUI currentDraggedSlot;
    protected Vector2 originalDragPosition;
    protected bool isDragging = false;

    public virtual void Initialize(IItemContainer itemContainer)
    {
        // Store reference to container
        container = itemContainer;

        // Set container title
        if (containerTitleText != null)
        {
            containerTitleText.text = container.ContainerName;
        }

        // Subscribe to container events
        if (container != null)
        {
            container.OnContainerChanged += Container_OnContainerChanged;
        }
        else
        {
            Debug.LogError($"Container is null in {gameObject.name}");
            return;
        }

        // Use existing slot UIs - don't clear or create new ones
        slotUIs.Clear();
        InventorySlotUI[] existingSlots = slotsParent.GetComponentsInChildren<InventorySlotUI>();

        Debug.Log($"Found {existingSlots.Length} existing slot UIs in {gameObject.name}");

        // Get container slots
        List<InventorySlot> containerSlots = container.GetAllSlots();
        Debug.Log($"Container has {containerSlots.Count} slots");

        // Connect the minimum number of slots (to avoid index errors)
        int slotCount = Mathf.Min(existingSlots.Length, containerSlots.Count);

        // Connect each UI slot to a container slot
        for (int i = 0; i < slotCount; i++)
        {
            // Skip null slots
            if (existingSlots[i] == null)
            {
                Debug.LogWarning($"Slot UI at index {i} is null");
                continue;
            }

            try
            {
                // Initialize slot and subscribe to events
                existingSlots[i].Initialize(containerSlots[i], i);

                // Hook up event handlers
                existingSlots[i].OnSlotClicked += SlotUI_OnSlotClicked;
                existingSlots[i].OnSlotBeginDrag += SlotUI_OnSlotBeginDrag;
                existingSlots[i].OnSlotDrag += SlotUI_OnSlotDrag;
                existingSlots[i].OnSlotEndDrag += SlotUI_OnSlotEndDrag;

                // Add to our tracking list
                slotUIs.Add(existingSlots[i]);

                Debug.Log($"Connected slot {i}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error initializing slot {i}: {e.Message}");
            }
        }

        // Initially hide dragged item icon
        if (draggedItemIcon != null)
        {
            draggedItemIcon.gameObject.SetActive(false);
        }

        // Refresh all slots
        RefreshAllSlots();
    }

    protected virtual void OnDestroy()
    {
        // Unsubscribe from events
        if (container != null)
        {
            container.OnContainerChanged -= Container_OnContainerChanged;
        }
    }

    protected virtual void ClearSlotUIs()
    {
        // Clear existing slot UIs
        foreach (Transform child in slotsParent)
        {
            Destroy(child.gameObject);
        }
        slotUIs.Clear();
    }

    protected virtual void Container_OnContainerChanged(object sender, EventArgs e)
    {
        RefreshAllSlots();
    }

    protected virtual void RefreshAllSlots()
    {
        for (int i = 0; i < slotUIs.Count; i++)
        {
            if (slotUIs[i] != null)
            {
                slotUIs[i].RefreshUI();
            }
            else
            {
                Debug.LogWarning($"Slot UI at index {i} is null when refreshing");
                // Remove null slots from our list
                slotUIs.RemoveAt(i);
                i--; // Adjust index after removal
            }
        }
    }

    protected virtual void SlotUI_OnSlotClicked(object sender, InventorySlotUI.SlotClickedEventArgs e)
    {
        // Handle click actions (e.g., item use, stack splitting, etc.)
        Debug.Log($"Slot {e.slotIndex} clicked in {container.ContainerName}");
    }

    protected virtual void SlotUI_OnSlotBeginDrag(object sender, InventorySlotUI.SlotDragEventArgs e)
    {
        if (e.slot.IsEmpty()) return;

        currentDraggedSlot = e.slotUI;
        originalDragPosition = e.position;

        // Set up dragged item visuals
        if (draggedItemIcon != null)
        {
            draggedItemIcon.gameObject.SetActive(true);
            draggedItemIcon.position = e.position;
            draggedItemIcon.GetComponent<Image>().sprite = e.slot.GetItemSO().sprite;
        }

        isDragging = true;
    }

    protected virtual void SlotUI_OnSlotDrag(object sender, InventorySlotUI.SlotDragEventArgs e)
    {
        if (!isDragging) return;

        // Update dragged item position
        if (draggedItemIcon != null)
        {
            draggedItemIcon.position = e.position;
        }
    }

    protected virtual void SlotUI_OnSlotEndDrag(object sender, InventorySlotUI.SlotDragEventArgs e)
    {
        if (!isDragging) return;

        // Hide dragged item
        if (draggedItemIcon != null)
        {
            draggedItemIcon.gameObject.SetActive(false);
        }

        isDragging = false;

        // Find the slot under the cursor
        InventorySlotUI targetSlot = FindSlotUnderPosition(e.position);

        if (targetSlot != null && targetSlot != currentDraggedSlot)
        {
            // Is it in the same container?
            if (targetSlot.transform.parent == currentDraggedSlot.transform.parent)
            {
                // Swap items within this container
                container.SwapItems(currentDraggedSlot.GetSlotIndex(), targetSlot.GetSlotIndex());
            }
            else
            {
                // Find which container the target slot belongs to
                foreach (var ui in UnityEngine.Object.FindObjectsByType<ContainerUI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                {
                    if (ui != this && ui.IsSlotInContainer(targetSlot))
                    {
                        // Cross-container transfer
                        container.TransferItemTo(
                            ui.GetContainer(),
                            currentDraggedSlot.GetSlotIndex(),
                            targetSlot.GetSlotIndex());

                        break;
                    }
                }
            }
        }

        currentDraggedSlot = null;
    }

    protected virtual InventorySlotUI FindSlotUnderPosition(Vector2 position)
    {
        // First check slots in this container
        foreach (InventorySlotUI slotUI in slotUIs)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                slotUI.GetComponent<RectTransform>(), position))
            {
                return slotUI;
            }
        }

        // Then check slots in other visible containers
        foreach (var ui in UnityEngine.Object.FindObjectsByType<ContainerUI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (ui != this && ui.gameObject.activeSelf)
            {
                InventorySlotUI slot = ui.FindSlotUnderPositionInThis(position);
                if (slot != null)
                {
                    return slot;
                }
            }
        }

        return null;
    }

    public InventorySlotUI FindSlotUnderPositionInThis(Vector2 position)
    {
        foreach (InventorySlotUI slotUI in slotUIs)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                slotUI.GetComponent<RectTransform>(), position))
            {
                return slotUI;
            }
        }

        return null;
    }

    public bool IsSlotInContainer(InventorySlotUI slot)
    {
        return slotUIs.Contains(slot);
    }

    public IItemContainer GetContainer()
    {
        return container;
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);
        RefreshAllSlots();
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);

        // Reset any dragging operation
        if (isDragging)
        {
            if (draggedItemIcon != null)
                draggedItemIcon.gameObject.SetActive(false);
            isDragging = false;
            currentDraggedSlot = null;
        }
    }
}