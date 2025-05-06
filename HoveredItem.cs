using UnityEngine;
using UnityEngine.UI;

public class HoveredItem : MonoBehaviour
{
    [SerializeField] private Item item;

    private Outline[] outlines;

    private void Awake()
    {
        // Get all Outline components in children of this GameObject (MilkVisual)
        outlines = GetComponentsInChildren<Outline>();
        DisableOutlines();
    }

    private void Start()
    {
        Player.Instance.OnPlayerHoverItem += Player_OnPlayerHoverItem;
    }

    private void Player_OnPlayerHoverItem(object sender, Player.OnPlayerHoverItemEventArgs e)
    {
        if (e.item == item)
        {
            if (e.isHovered)
                EnableOutlines();
            else
                DisableOutlines();
        }
    }


    private void DisableOutlines()
    {
        foreach (Outline outline in outlines)
        {
            outline.enabled = false;
        }
    }

    private void EnableOutlines()
    {
        foreach (Outline outline in outlines)
        {
            outline.enabled = true;
        }
    }

}
