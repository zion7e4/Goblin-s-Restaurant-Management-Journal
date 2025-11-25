using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections.Generic;

public class QuantityPopupController : MonoBehaviour
{
    [Header("UI References")]
    public Image recipeIcon;
    public TextMeshProUGUI recipeNameText;

    [Header("Ingredients List")]
    public GameObject ingredientUIPrefab;
    public Transform ingredientContentParent;

    [Header("Quantity Control")]
    public TextMeshProUGUI quantityText;
    public Button plusButton;
    public Button minusButton;
    public Button maxButton;

    [Header("Action Buttons")]
    public Button confirmButton;
    public Button closeButton;

    private PlayerRecipe myRecipe;
    private MenuPlannerUI_Controller controller;
    private int currentQuantity;
    private int maxQuantity;

    private List<MenuIngredientUI> spawnedIngredientUIs = new List<MenuIngredientUI>();

    void Awake()
    {
        if (plusButton) plusButton.onClick.AddListener(() => ChangeQuantity(1));
        if (minusButton) minusButton.onClick.AddListener(() => ChangeQuantity(-1));
        if (maxButton) maxButton.onClick.AddListener(SetMaxQuantity);
        if (confirmButton) confirmButton.onClick.AddListener(OnConfirmClick);
        if (closeButton) closeButton.onClick.AddListener(OnCloseClick);
    }

    public void Show(PlayerRecipe recipe, MenuPlannerUI_Controller uiController)
    {
        myRecipe = recipe;
        controller = uiController;

        recipeIcon.sprite = recipe.data.icon;
        recipeIcon.preserveAspect = true;
        recipeNameText.text = recipe.data.recipeName;

        maxQuantity = InventoryManager.instance.GetMaxCookableAmount(recipe);

        currentQuantity = 1;

        foreach (Transform child in ingredientContentParent)
        {
            Destroy(child.gameObject);
        }
        spawnedIngredientUIs.Clear();

        foreach (var req in myRecipe.data.requiredIngredients)
        {
            GameObject itemGO = Instantiate(ingredientUIPrefab, ingredientContentParent);
            MenuIngredientUI itemUI = itemGO.GetComponent<MenuIngredientUI>();
            spawnedIngredientUIs.Add(itemUI);
        }

        UpdateUI();
    }

    void ChangeQuantity(int amount)
    {
        currentQuantity += amount;

        int max = (maxQuantity > 0) ? maxQuantity : 1;

        currentQuantity = Mathf.Clamp(currentQuantity, 1, max);

        UpdateUI();
    }

    void SetMaxQuantity()
    {
        currentQuantity = (maxQuantity > 0) ? maxQuantity : 1;
        UpdateUI();
    }

    void UpdateUI()
    {
        quantityText.text = currentQuantity.ToString("D2");

        for (int i = 0; i < spawnedIngredientUIs.Count; i++)
        {
            IngredientRequirement req = myRecipe.data.requiredIngredients[i];
            IngredientData ingredient = GameDataManager.instance.GetIngredientDataById(req.ingredientID);
            InventoryManager.instance.playerIngredients.TryGetValue(req.ingredientID, out int owned);

            int required = req.amount * currentQuantity;

            spawnedIngredientUIs[i].UpdateData(ingredient, owned, required);
        }

        bool canAdjustQuantity = (maxQuantity > 0);

        plusButton.interactable = canAdjustQuantity && (currentQuantity < maxQuantity);
        minusButton.interactable = canAdjustQuantity && (currentQuantity > 1);
        maxButton.interactable = canAdjustQuantity;

        confirmButton.interactable = canAdjustQuantity;
    }

    void OnConfirmClick()
    {
        if (currentQuantity > 0)
        {
            controller.OnConfirmQuantity(myRecipe, currentQuantity);
        }
    }

    void OnCloseClick()
    {
        controller.CloseQuantityPopup();
    }
}