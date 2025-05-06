using System;
using UnityEngine;

public class FridgeContainer : BaseItemContainer, IInteractable
{
    [SerializeField] private bool isOpen = false;

    // Allow the fridge to be interacted with
    public event EventHandler OnFridgeStateChanged;

    protected override void Awake()
    {
        containerName = "Fridge";
        containerType = ContainerType.Fridge;
        slotCount = 6; // 6 slots in fridge

        base.Awake();
    }

    public void Interact(Player player)
    {
        ToggleOpen();
    }

    public string GetInteractText()
    {
        return isOpen ? "Close Fridge" : "Open Fridge";
    }

    public void ToggleOpen()
    {
        isOpen = !isOpen;
        OnFridgeStateChanged?.Invoke(this, EventArgs.Empty);

        // Handle UI visibility
        if (isOpen)
        {
            ContainerUIManager.Instance.ShowContainerUI(this);
        }
        else
        {
            ContainerUIManager.Instance.HideContainerUI(this);
        }
    }

    public void Open()
    {
        if (!isOpen)
        {
            isOpen = true;
            OnFridgeStateChanged?.Invoke(this, EventArgs.Empty);
            ContainerUIManager.Instance.ShowContainerUI(this);
        }
    }

    public void Close()
    {
        if (isOpen)
        {
            isOpen = false;
            OnFridgeStateChanged?.Invoke(this, EventArgs.Empty);
            ContainerUIManager.Instance.HideContainerUI(this);
        }
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    // Only allow ingredients that need refrigeration
    public override bool CanAddItem(ItemSO itemSO, int amount = 1)
    {
        // Check if it's a refrigerated ingredient
        IngredientSO ingredientSO = itemSO as IngredientSO;
        if (ingredientSO != null && ingredientSO.requiresRefrigeration)
        {
            return base.CanAddItem(itemSO, amount);
        }

        return false;
    }
}