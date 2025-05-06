using UnityEngine;

[CreateAssetMenu(fileName = "New Ingredient", menuName = "Inventory/Cooking/Ingredient")]
public class IngredientSO : ItemSO
{
    [Header("Ingredient Properties")]
    public bool requiresRefrigeration = false;
    public float nutritionalValue = 1.0f;

    [Header("Cooking Properties")]
    public bool canBeCooked = true;
    public float cookingTime = 1.0f;
}