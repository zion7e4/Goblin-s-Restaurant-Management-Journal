using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

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

    void Awake()
    {
        recipeTabButton.onClick.AddListener(SwitchToRecipeTab);
        ingredientTabButton.onClick.AddListener(SwitchToIngredientTab);
    }

    void OnEnable()
    {
        SwitchToRecipeTab();
    }

    public void SwitchToRecipeTab()
    {
        recipeShopPanel.SetActive(true);
        ingredientShopPanel.SetActive(false);
        PopulateRecipeShop();
    }

    public void SwitchToIngredientTab()
    {
        recipeShopPanel.SetActive(false);
        ingredientShopPanel.SetActive(true);
        PopulateIngredientShop();
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

        var allIngredients = GameDataManager.instance.GetAllIngredientData();
        foreach (var ingredientData in allIngredients)
        {
            GameObject itemGO = Instantiate(ingredientItemPrefab, ingredientContentParent);
            itemGO.GetComponent<ShopIngredientItemUI>().Setup(ingredientData, this);
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
        }
        else
        {
            Debug.Log("골드가 부족하여 재료를 구매할 수 없습니다.");
        }
    }
}

