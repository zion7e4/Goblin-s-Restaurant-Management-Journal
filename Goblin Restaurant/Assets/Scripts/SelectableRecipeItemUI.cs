using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectableRecipeItemUI : MonoBehaviour
{
    public Button selectButton;
    public TextMeshProUGUI recipeNameText;
    private PlayerRecipe myRecipe;
    private MenuPlannerUI_Controller controller;

    public void Setup(PlayerRecipe recipe, MenuPlannerUI_Controller uiController)
    {
        myRecipe = recipe;
        recipeNameText.text = myRecipe.data.recipeName;

        controller = uiController;

        selectButton.onClick.AddListener(OnSelectButtonClick);
    }

    void OnSelectButtonClick()
    {
        if (controller != null)
        {
            controller.OnRecipeSelectedFromPopup(myRecipe);
        }
    }
}