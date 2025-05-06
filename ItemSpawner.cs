using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private ItemSO[] availableItems;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnRadius = 1.5f;
    [SerializeField] private float spawnHeight = 0.5f;

    // Debug Controls
    [SerializeField] private KeyCode spawnRandomKey = KeyCode.F;

    private void Update()
    {
        // Debug spawn items
        if (Input.GetKeyDown(spawnRandomKey))
        {
            SpawnRandomItem();
        }
    }

    public void SpawnRandomItem()
    {
        if (availableItems == null || availableItems.Length == 0)
        {
            Debug.LogWarning("No items configured in ItemSpawner");
            return;
        }

        // Select random item
        ItemSO randomItemSO = availableItems[Random.Range(0, availableItems.Length)];
        SpawnItem(randomItemSO);
    }

    public void SpawnItem(ItemSO itemSO)
    {
        if (itemSO == null || itemSO.prefab == null)
        {
            Debug.LogError("Invalid item or prefab");
            return;
        }

        // Determine random position within spawn radius
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = spawnPoint.position + new Vector3(randomCircle.x, spawnHeight, randomCircle.y);

        // Instantiate the item
        Transform itemTransform = Instantiate(itemSO.prefab, spawnPosition, Quaternion.identity);

        // Make sure it has an Item component
        Item item = itemTransform.GetComponent<Item>();
        if (item == null)
        {
            Debug.LogError($"Prefab {itemSO.itemName} doesn't have an Item component");
            Destroy(itemTransform.gameObject);
            return;
        }

        // Set its ItemSO
        item.itemSO = itemSO;
    }
}