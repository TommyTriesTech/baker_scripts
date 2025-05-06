using System;
using UnityEngine;

public class ToolbarContainer : BaseItemContainer
{
    [SerializeField] private int selectedSlotIndex = 0;

    public event EventHandler<SelectedSlotChangedEventArgs> OnSelectedSlotChanged;

    public class SelectedSlotChangedEventArgs : EventArgs
    {
        public int slotIndex;
        public InventorySlot slot;
    }

    protected override void Awake()
    {
        containerName = "Toolbar";
        containerType = ContainerType.Toolbar;
        slotCount = 6; // 6 slots toolbar

        base.Awake();
    }

    private void Start()
    {
        // Notify about initial selected slot
        RaiseSelectedSlotChanged();
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= slotCount)
            return;

        selectedSlotIndex = index;
        RaiseSelectedSlotChanged();
    }

    public void SelectNextSlot()
    {
        selectedSlotIndex = (selectedSlotIndex + 1) % slotCount;
        RaiseSelectedSlotChanged();
    }

    public void SelectPreviousSlot()
    {
        selectedSlotIndex = (selectedSlotIndex - 1 + slotCount) % slotCount;
        RaiseSelectedSlotChanged();
    }

    public int GetSelectedSlotIndex()
    {
        return selectedSlotIndex;
    }

    public InventorySlot GetSelectedSlot()
    {
        return GetSlot(selectedSlotIndex);
    }

    private void RaiseSelectedSlotChanged()
    {
        OnSelectedSlotChanged?.Invoke(this, new SelectedSlotChangedEventArgs
        {
            slotIndex = selectedSlotIndex,
            slot = GetSlot(selectedSlotIndex)
        });
    }

    // Handle keyboard number inputs to select toolbar slots
    private void Update()
    {
        // Check number keys 1-6 (or however many slots you have)
        for (int i = 0; i < Mathf.Min(SlotCount, 9); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
            {
                SelectSlot(i);
                break;
            }
        }

        // Scroll wheel to cycle through slots
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if (scrollDelta > 0.1f)
        {
            SelectPreviousSlot();
        }
        else if (scrollDelta < -0.1f)
        {
            SelectNextSlot();
        }
    }
}