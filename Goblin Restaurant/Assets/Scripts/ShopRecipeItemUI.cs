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
            priceText.text = "º¸À¯ Áß";
            buyButton.interactable = false;
        }
        else
        {
            int price = (int)(recipeData.basePrice * 1.5f);
            priceText.text = price.ToString() + " G";
            buyButton.interactable = true;
            buyButton.onClick.AddListener(OnBuyButtonClick);
        }
    }

    void OnBuyButtonClick()
    {
        controller.AttemptPurchaseRecipe(myRecipeData);
    }
}

