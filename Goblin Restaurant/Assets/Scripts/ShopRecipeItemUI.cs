using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopRecipeItemUI : MonoBehaviour
{
    [Header("UI References")]
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

        if (recipeIcon != null)
        {
            recipeIcon.sprite = myRecipeData.icon;
            recipeIcon.preserveAspect = true;
        }
        if (recipeNameText != null) recipeNameText.text = myRecipeData.recipeName;

        bool isOwned = RecipeManager.instance.playerRecipes.ContainsKey(myRecipeData.id);
        
        int currentFameLevel = 0;
        if (FameManager.instance != null) 
            currentFameLevel = FameManager.instance.CurrentFameLevel;

        int requiredLevel = poolEntry.required_lv; 
        bool fameMet = currentFameLevel >= requiredLevel;

        if (isOwned)
        {
            if (priceText != null) priceText.text = "보유 중";
            if (buyButton != null) buyButton.interactable = false;
            if (coinIcon != null) coinIcon.gameObject.SetActive(false); 
        }
        else if (!fameMet)
        {
            if (priceText != null) priceText.text = $"명성도 레벨{requiredLevel} 해금";
            if (buyButton != null) buyButton.interactable = false;
            
            if (recipeIcon != null) recipeIcon.color = Color.gray;
            if (coinIcon != null) coinIcon.gameObject.SetActive(false);
        }
        else
        {
            int price = (int)(myRecipeData.basePrice * 1.5f);
            if (priceText != null) priceText.text = $"{price} G";
            
            if (buyButton != null)
            {
                buyButton.interactable = true;
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(OnBuyButtonClick);
            }
            
            if (recipeIcon != null) recipeIcon.color = Color.white;
            if (coinIcon != null) coinIcon.gameObject.SetActive(true);
        }

        SetTooltip();
    }

    void SetTooltip()
    {
        string tooltip = "필요 재료:\n";
        foreach (var req in myRecipeData.requiredIngredients)
        {
            if (GameDataManager.instance != null)
            {
                var ingredientData = GameDataManager.instance.GetIngredientDataById(req.ingredientID);
                if (ingredientData != null)
                {
                    tooltip += $"- {ingredientData.ingredientName} x{req.amount}\n";
                }
            }
        }

        var tooltipTrigger = GetComponentInChildren<TooltipTrigger>();
        if (tooltipTrigger != null)
        {
            tooltipTrigger.SetTooltipText(tooltip);
        }
    }

    void OnBuyButtonClick()
    {
        if (controller != null)
        {
            controller.AttemptPurchaseRecipe(myRecipeData);
        }
    }
}