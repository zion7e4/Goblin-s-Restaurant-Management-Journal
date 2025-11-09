using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System.Text;

public class ShopUIController : MonoBehaviour
{
    public GameObject recipeShopPanel;
    public GameObject ingredientShopPanel;
    public Button recipeTabButton;
    public Button ingredientTabButton;

    public GameObject recipeItemPrefab;
    public Transform recipeContentParent;

    public GameObject ingredientItemPrefab;
    public Transform ingredientContentParent;

    [Header("Ingredient Bulk Purchase")]
    public Button bulkPurchaseButton;
    public TextMeshProUGUI bulkTotalCostText;

    private List<ShopIngredientItemUI> spawnedIngredientItems = new List<ShopIngredientItemUI>();

    void Awake()
    {
        recipeTabButton.onClick.AddListener(SwitchToRecipeTab);
        ingredientTabButton.onClick.AddListener(SwitchToIngredientTab);

        if (bulkPurchaseButton != null)
        {
            bulkPurchaseButton.onClick.AddListener(OnBulkPurchaseClick);
        }
    }

    void OnEnable()
    {
        SwitchToRecipeTab();
    }

    void OnDisable()
    {
        if (TooltipSystem.instance != null)
        {
            TooltipSystem.instance.Hide();
        }
        spawnedIngredientItems.Clear();
    }

    public void SwitchToRecipeTab()
    {
        recipeShopPanel.SetActive(true);
        ingredientShopPanel.SetActive(false);
        PopulateRecipeShop();

        if (bulkPurchaseButton != null) bulkPurchaseButton.gameObject.SetActive(false);
        if (bulkTotalCostText != null) bulkTotalCostText.gameObject.SetActive(false);
    }

    public void SwitchToIngredientTab()
    {
        recipeShopPanel.SetActive(false);
        ingredientShopPanel.SetActive(true);
        PopulateIngredientShop();

        if (bulkPurchaseButton != null) bulkPurchaseButton.gameObject.SetActive(true);
        if (bulkTotalCostText != null) bulkTotalCostText.gameObject.SetActive(true);

        UpdateBulkTotalCost();
    }

    void PopulateRecipeShop()
    {
        foreach (Transform child in recipeContentParent) Destroy(child.gameObject);

        var allRecipes = GameDataManager.instance.GetAllRecipeData();


        foreach (var recipeData in allRecipes)
        {
            bool isAlreadyOwned = RecipeManager.instance.playerRecipes.ContainsKey(recipeData.id);

            GameObject itemGO = Instantiate(recipeItemPrefab, recipeContentParent);
            itemGO.GetComponent<ShopRecipeItemUI>().Setup(recipeData, isAlreadyOwned, this);
        }
    }

    void PopulateIngredientShop()
    {
        foreach (Transform child in ingredientContentParent) Destroy(child.gameObject);
        spawnedIngredientItems.Clear();

        var allIngredients = GameDataManager.instance.GetAllIngredientData();
        foreach (var ingredientData in allIngredients)
        {
            GameObject itemGO = Instantiate(ingredientItemPrefab, ingredientContentParent);
            ShopIngredientItemUI itemUI = itemGO.GetComponent<ShopIngredientItemUI>();
            itemUI.Setup(ingredientData, this);

            spawnedIngredientItems.Add(itemUI);
        }
    }

    public void AttemptPurchaseRecipe(RecipeData recipeData)
    {
        int price = (int)(recipeData.basePrice * 1.5f);
        if (GameManager.instance.totalGoldAmount >= price)
        {
            GameManager.instance.SpendGold(price);
            RecipeManager.instance.UnlockRecipe(recipeData.id);
            Debug.Log($"'{recipeData.recipeName}' 레시피 구매 성공!");
            PopulateRecipeShop();

            NotificationController.instance.ShowNotification($"-{price} G\n (레시피 구매)");
        }
        else
        {
            Debug.Log("골드가 부족하여 레시피를 구매할 수 없습니다.");
        }
    }
    public void AttemptPurchaseIngredient(IngredientData ingredientData, int quantity)
    {
        if (quantity <= 0) return;
        int totalPrice = ingredientData.buyPrice * quantity;
        if (GameManager.instance.totalGoldAmount >= totalPrice)
        {
            GameManager.instance.SpendGold(totalPrice);
            InventoryManager.instance.AddIngredient(ingredientData.id, quantity);
            Debug.Log($"'{ingredientData.ingredientName}' {quantity}개 구매 성공!");

            NotificationController.instance.ShowNotification($"-{totalPrice} G\n ({ingredientData.ingredientName} {quantity}개 구매)"); //
        }
        else
        {
            Debug.Log("골드가 부족하여 재료를 구매할 수 없습니다.");
            NotificationController.instance.ShowNotification("골드가 부족합니다!"); //
        }
    }
    public void OnBulkPurchaseClick()
    {
        int totalCost = 0;
        Dictionary<IngredientData, int> itemsToBuy = new Dictionary<IngredientData, int>();

        foreach (var itemUI in spawnedIngredientItems)
        {
            int quantity = itemUI.GetCurrentQuantity();
            if (quantity > 0)
            {
                IngredientData data = itemUI.GetIngredientData();
                itemsToBuy.Add(data, quantity);
                totalCost += data.buyPrice * quantity;
            }
        }

        if (totalCost == 0)
        {
            Debug.Log("일괄 구매할 아이템이 없습니다 (모든 수량이 0).");
            return;
        }

        if (GameManager.instance.totalGoldAmount >= totalCost)
        {
            GameManager.instance.SpendGold(totalCost);

            StringBuilder sb = new StringBuilder("일괄 구매 완료:\n");

            foreach (var item in itemsToBuy)
            {
                InventoryManager.instance.AddIngredient(item.Key.id, item.Value);
                sb.AppendLine($"- {item.Key.ingredientName} {item.Value}개");
            }

            sb.Append($"\n총 지출: -{totalCost} G");
            NotificationController.instance.ShowNotification(sb.ToString());

            PopulateIngredientShop();
        }
        else
        {
            Debug.Log("골드가 부족하여 일괄 구매를 할 수 없습니다.");
            NotificationController.instance.ShowNotification("골드가 부족합니다!");
        }
    }

    public void UpdateBulkTotalCost()
    {
        int totalCost = 0;

        foreach (var itemUI in spawnedIngredientItems)
        {
            totalCost += itemUI.GetCurrentTotalCost();
        }

        if (bulkTotalCostText != null)
        {
            bulkTotalCostText.text = $"총합계: {totalCost} G";
        }

        if (bulkPurchaseButton != null)
        {
            bulkPurchaseButton.interactable = totalCost > 0;
        }
    }
}