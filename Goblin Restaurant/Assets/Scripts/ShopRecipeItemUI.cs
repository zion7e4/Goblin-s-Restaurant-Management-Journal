using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopRecipeItemUI : MonoBehaviour
{
    public Image recipeIcon;
    public Image coinIcon;
    public TextMeshProUGUI recipeNameText;
    public TextMeshProUGUI priceText;
    public Button buyButton;

    private RecipeData myRecipeData;
    private ShopUIController controller;

    public void Setup(RecipePoolEntry poolEntry, ShopUIController shopController)
    {
        controller = shopController;
        myRecipeData = GameDataManager.instance.GetRecipeDataById(poolEntry.rcp_id);

        if (myRecipeData == null)
        {
            Debug.LogError($"ShopRecipeItemUI: ID {poolEntry.rcp_id}에 해당하는 RecipeData를 찾을 수 없습니다.");
            gameObject.SetActive(false);
            return;
        }

        recipeIcon.sprite = myRecipeData.icon;
        recipeIcon.preserveAspect = true;
        recipeNameText.text = myRecipeData.recipeName;

        bool isPurchased = RecipeManager.instance.playerRecipes.ContainsKey(myRecipeData.id);

        int currentFameLevel = FameManager.instance.CurrentFameLevel;
        int requiredLevel = poolEntry.required_lv;
        bool fameMet = currentFameLevel >= requiredLevel;

        if (isPurchased)
        {
            priceText.text = "보유 중";
            coinIcon.gameObject.SetActive(false);
            buyButton.interactable = false;
        }
        else if (!fameMet)
        {
            priceText.text = $"명성도 레벨 {requiredLevel} 필요";
            buyButton.interactable = false;
            recipeIcon.color = Color.gray;
            recipeNameText.color = Color.gray;
            coinIcon.gameObject.SetActive(false);
        }
        else
        {
            int price = (int)(myRecipeData.basePrice * 1.5f);
            coinIcon.gameObject.SetActive(true);
            priceText.text = price.ToString();
            buyButton.interactable = true;
            buyButton.onClick.AddListener(OnBuyButtonClick);
        }

        string tooltip = "필요 재료:\n";
        foreach (var req in myRecipeData.requiredIngredients)
        {
            var ingredientData = GameDataManager.instance.GetIngredientDataById(req.ingredientID);
            tooltip += $"- {ingredientData.ingredientName} x{req.amount}\n";
        }
        GetComponentInChildren<TooltipTrigger>().SetTooltipText(tooltip);
    }

    void OnBuyButtonClick()
    {
        controller.AttemptPurchaseRecipe(myRecipeData);
    }
}