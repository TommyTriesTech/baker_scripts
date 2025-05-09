using UnityEngine;

public interface IInteractable
{
    void Interact(Player player);
    string GetInteractText();
}

