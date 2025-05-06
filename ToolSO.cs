using UnityEngine;

[CreateAssetMenu(fileName = "New Tool", menuName = "Inventory/Cooking/Tool")]
public class ToolSO : ItemSO
{
    [Header("Tool Properties")]
    public bool hasDurability = true;
    public int maxDurability = 100;
    public int currentDurability = 100;
    public enum ToolFunction
    {
        Mixing,
        Cutting,
        Baking,
        Whipping,
        Measuring
    }
    public ToolFunction function = ToolFunction.Mixing;

    // Default values for tools
    public ToolSO()
    {
        isStackable = false;
        maxStackSize = 1;
    }

    // Tool-specific methods
    public float GetDurabilityPercentage()
    {
        if (!hasDurability || maxDurability <= 0)
            return 1f;

        return (float)currentDurability / maxDurability;
    }

    public void UseTool()
    {
        if (hasDurability)
        {
            currentDurability = Mathf.Max(0, currentDurability - 1);
        }
    }

    public bool IsBroken()
    {
        return hasDurability && currentDurability <= 0;
    }
}