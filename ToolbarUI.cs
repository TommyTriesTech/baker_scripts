using UnityEngine;
using UnityEngine.UI;

// UI component for displaying the toolbar
public class ToolbarUI : ContainerUI
{
    [SerializeField] private RectTransform selectionHighlight;

    private ToolbarContainer toolbarContainer;

    public override void Initialize(IItemContainer itemContainer)
    {
        base.Initialize(itemContainer);

        toolbarContainer = itemContainer as ToolbarContainer;
        if (toolbarContainer == null)
        {
            Debug.LogError("ToolbarUI must be initialized with a ToolbarContainer");
            return;
        }

        // Subscribe to selected slot changed event
        toolbarContainer.OnSelectedSlotChanged += ToolbarContainer_OnSelectedSlotChanged;

        // Set initial highlight position
        UpdateSelectionHighlight(toolbarContainer.GetSelectedSlotIndex());
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (toolbarContainer != null)
        {
            toolbarContainer.OnSelectedSlotChanged -= ToolbarContainer_OnSelectedSlotChanged;
        }
    }

    private void ToolbarContainer_OnSelectedSlotChanged(object sender, ToolbarContainer.SelectedSlotChangedEventArgs e)
    {
        UpdateSelectionHighlight(e.slotIndex);
    }

    protected override void SlotUI_OnSlotClicked(object sender, InventorySlotUI.SlotClickedEventArgs e)
    {
        // When a toolbar slot is clicked, select it
        toolbarContainer.SelectSlot(e.slotIndex);
    }

    private void UpdateSelectionHighlight(int selectedIndex)
    {
        if (selectionHighlight == null || slotUIs.Count == 0 ||
            selectedIndex < 0 || selectedIndex >= slotUIs.Count)
            return;

        // Position the highlight over the selected slot
        RectTransform selectedSlotRect = slotUIs[selectedIndex].GetComponent<RectTransform>();
        selectionHighlight.position = selectedSlotRect.position;
        selectionHighlight.sizeDelta = selectedSlotRect.sizeDelta + new Vector2(10, 10); // Slightly larger
    }
}