using System;
using UnityEngine;

public class PantryContainer : BaseItemContainer, IInteractable
{
    [SerializeField] private bool isOpen = false;

    public event EventHandler OnPantryStateChanged;

    protected override void Awake()
    {
        containerName = "Pantry";
        containerType = ContainerType.Pantry;
        slotCount = 12; // 12 slots in pantry

        base.Awake();
    }

    public void Interact(Player player)
    {
        ToggleOpen();
    }

    public string GetInteractText()
    {
        return isOpen ? "Close Pantry" : "Open Pantry";
    }

    public void ToggleOpen()
    {
        isOpen = !isOpen;
        OnPantryStateChanged?.Invoke(this, EventArgs.Empty);

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
            OnPantryStateChanged?.Invoke(this, EventArgs.Empty);
            ContainerUIManager.Instance.ShowContainerUI(this);
        }
    }

    public void Close()
    {
        if (isOpen)
        {
            isOpen = false;
            OnPantryStateChanged?.Invoke(this, EventArgs.Empty);
            ContainerUIManager.Instance.HideContainerUI(this);
        }
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    // Only allow dry ingredients or ones that don't need refrigeration
    public override bool CanAddItem(ItemSO itemSO, int amount = 1)
    {
        // Check if it's a non-refrigerated ingredient
        IngredientSO ingredientSO = itemSO as IngredientSO;
        if (ingredientSO != null && !ingredientSO.requiresRefrigeration)
        {
            return base.CanAddItem(itemSO, amount);
        }

        return false;
    }
}