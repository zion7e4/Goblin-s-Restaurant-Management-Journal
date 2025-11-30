// Assets/Scripts/Data/GeneratedShopItem.cs (덮어쓰기)
using UnityEngine;

public enum ShopItemType { Ingredient, Recipe }

public class GeneratedShopItem
{
    public ShopItemType ItemType;
    public string ItemID; // 재료(ING10) 또는 레시피(1002) ID
    public int BasePrice { get; private set; }
    public int CurrentPrice;
    public int CurrentStock;
    public int InitialStock; // (UI 표시용)

    // 편의를 위한 참조
    public IngredientData ingredientData { get; private set; }
    public RecipeData recipeData { get; private set; }
    public bool isSoldOut { get { return CurrentStock <= 0; } }

    // 특수 재료용 생성자
    public GeneratedShopItem(IngredientData data, int basePrice, int currentPrice, int stock)
    {
        this.ItemType = ShopItemType.Ingredient;
        this.ItemID = data.id;
        this.ingredientData = data;
        this.recipeData = null;

        this.BasePrice = basePrice;
        this.CurrentPrice = currentPrice;
        this.CurrentStock = stock;
        this.InitialStock = stock;
    }

    // 레시피용 생성자
    public GeneratedShopItem(RecipeData data, int basePrice, int currentPrice)
    {
        this.ItemType = ShopItemType.Recipe;
        this.ItemID = data.id.ToString();
        this.ingredientData = null;
        this.recipeData = data;

        this.BasePrice = basePrice;
        this.CurrentPrice = currentPrice;
        this.CurrentStock = 1;
        this.InitialStock = 1;
    }
}