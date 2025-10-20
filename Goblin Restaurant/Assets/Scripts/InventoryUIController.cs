using UnityEngine;
using System.Collections.Generic;

public class InventoryUIController : MonoBehaviour
{
    public GameObject inventoryItemPrefab; 
    public Transform contentParent;

    public void OpenInventory()
    {
        gameObject.SetActive(true);
        UpdateInventoryList();
    }

    public void CloseInventory()
    {
        gameObject.SetActive(false);
    }

    void UpdateInventoryList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        List<IngredientData> allIngredients = GameDataManager.instance.GetAllIngredientData();

        foreach (IngredientData ingredientData in allIngredients)
        {
            InventoryManager.instance.playerIngredients.TryGetValue(ingredientData.id, out int quantity);

            GameObject itemGO = Instantiate(inventoryItemPrefab, contentParent);
            itemGO.GetComponent<InventoryItemUI>().Setup(ingredientData, quantity);
        }
    }
}