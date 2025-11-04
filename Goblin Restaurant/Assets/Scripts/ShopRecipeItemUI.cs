using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopRecipeItemUI : MonoBehaviour
{
    public Image recipeIcon;
    public TextMeshProUGUI recipeNameText;
    public TextMeshProUGUI priceText;
    public Button buyButton;

    private RecipeData myRecipeData;
    private ShopUIController controller;

    public void Setup(RecipeData recipeData, bool isPurchased, ShopUIController shopController)
    {
        myRecipeData = recipeData;
        controller = shopController;

        recipeIcon.sprite = recipeData.icon;
        recipeNameText.text = recipeData.recipeName;

        if (isPurchased)
        {
            priceText.text = "보유 중";
            buyButton.interactable = false;
        }
        else
        {
            int price = (int)(recipeData.basePrice * 1.5f);
            priceText.text = price.ToString() + " G";
            buyButton.interactable = true;
            buyButton.onClick.AddListener(OnBuyButtonClick);
        }

        string tooltip = "필요 재료:\n";
        foreach (var req in GameDataManager.instance.GetRecipeDataById(recipeData.id).requiredIngredients)
        {
            var ingredientData = GameDataManager.instance.GetIngredientDataById(req.ingredientID);
            tooltip += $"- {ingredientData.ingredientName} x{req.amount}\n";
        }


        // 툴팁 트리거에 내용 전달
        GetComponentInChildren<TooltipTrigger>().SetTooltipText(tooltip);
    }

    void OnBuyButtonClick()
    {
        controller.AttemptPurchaseRecipe(myRecipeData);
    }
}

