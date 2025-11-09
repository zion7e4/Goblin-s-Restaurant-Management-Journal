using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectableRecipeItemUI : MonoBehaviour
{
    public Button selectButton;
    public TextMeshProUGUI recipeNameText;

    private PlayerRecipe myRecipe;
    private MenuPlannerUI_Controller controller;

    public void Setup(PlayerRecipe recipe, bool canSelect, MenuPlannerUI_Controller uiController)
    {
        myRecipe = recipe;
        controller = uiController;
        recipeNameText.text = myRecipe.data.recipeName;

        selectButton.interactable = canSelect;

        if (!canSelect)
        {
            recipeNameText.color = Color.gray;
        }

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnSelectButtonClick);
    }

    void OnSelectButtonClick()
    {
        if (controller != null)
        {
            controller.OpenQuantityPopup(myRecipe);
        }
    }
}