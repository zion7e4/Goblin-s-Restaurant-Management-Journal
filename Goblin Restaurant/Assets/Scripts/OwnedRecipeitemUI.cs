using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public struct RarityBorder
{
    public Rarity rarity;
    public Sprite borderSprite;
}

public class OwnedRecipeItemUI : MonoBehaviour
{
    public Button selectButton;
    public Image recipeIconImage;

    [Header("등급(Rarity) 테두리")]
    public Image borderImage;
    public List<RarityBorder> rarityBorders;

    private MenuPlannerUI_Controller controller;

    private PlayerRecipe myPlayerRecipeInstance;

    public void Setup(RecipeData recipeData, MenuPlannerUI_Controller uiController)
    {
        controller = uiController;
        myPlayerRecipeInstance = null;
        if (recipeIconImage != null)
        {
            recipeIconImage.sprite = recipeData.icon;
        }
        Rarity currentRarity = recipeData.rarity;

        bool isOwned = RecipeManager.instance.playerRecipes.TryGetValue(recipeData.id, out myPlayerRecipeInstance);

        bool isSelectableVisual;
        Color targetColor;

        if (isOwned)
        {
            selectButton.interactable = true;

            bool alreadyAdded = MenuPlanner.instance.dailyMenu.Any(r => r != null && r.data.id == recipeData.id);

            bool canCook = InventoryManager.instance.CanCook(myPlayerRecipeInstance);

            isSelectableVisual = canCook && !alreadyAdded;

            targetColor = isSelectableVisual ? Color.white : Color.gray;
        }
        else
        {
            selectButton.interactable = false;

            targetColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        }

        if (recipeIconImage != null) recipeIconImage.color = targetColor;

        if (borderImage != null && rarityBorders != null)
        {
            Sprite targetSprite = rarityBorders.FirstOrDefault(b => b.rarity == currentRarity).borderSprite;
            if (targetSprite != null)
            {
                borderImage.sprite = targetSprite;
                borderImage.color = targetColor;
            }
        }

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnSelectButtonClick);
    }

    void OnSelectButtonClick()
    {
        if (controller != null && myPlayerRecipeInstance != null)
        {
            controller.OpenQuantityPopup(myPlayerRecipeInstance);
        }
    }
}