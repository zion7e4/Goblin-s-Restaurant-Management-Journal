using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopIngredientItemUI : MonoBehaviour
{
    public Image ingredientIcon;
    public TextMeshProUGUI ingredientNameText;
    public TextMeshProUGUI priceText;
    public Button buyButton;

    public Button plusButton;
    public Button minusButton;
    public TMP_InputField quantityInput;
    public TextMeshProUGUI buyButtonText;

    public Button plus10Button;
    public Button minus10Button;

    private int currentQuantity = 0;

    private IngredientData myIngredientData;
    private ShopUIController controller;

    public void Setup(IngredientData ingredientData, ShopUIController shopController)
    {
        myIngredientData = ingredientData;
        controller = shopController;

        ingredientIcon.sprite = ingredientData.icon;
        ingredientNameText.text = ingredientData.ingredientName;
        priceText.text = ingredientData.buyPrice.ToString() + " G";

        buyButton.onClick.AddListener(OnInstantBuyClick);

        plusButton.onClick.AddListener(() => ChangeQuantity(1));
        minusButton.onClick.AddListener(() => ChangeQuantity(-1));

        if (plus10Button != null)
        {
            plus10Button.onClick.AddListener(() => ChangeQuantity(10));
        }
        if (minus10Button != null)
        {
            minus10Button.onClick.AddListener(() => ChangeQuantity(-10));
        }

        quantityInput.onValueChanged.AddListener(OnInputValueChanged);

        UpdateUI();

        string tooltip = "사용되는 레시피:\n";
        foreach (var recipe in GameDataManager.instance.GetAllRecipeData())
        {
            foreach (var req in recipe.requiredIngredients)
            {
                if (req.ingredientID == ingredientData.id)
                {
                    tooltip += $"- {recipe.recipeName}\n";
                    break;
                }
            }
        }
        GetComponentInChildren<TooltipTrigger>().SetTooltipText(tooltip);
    }

    void ChangeQuantity(int amount)
    {
        currentQuantity += amount;
        if (currentQuantity < 0) currentQuantity = 0;
        UpdateUI();
    }

    void OnInputValueChanged(string value)
    {
        if (int.TryParse(value, out int num))
        {
            if (num < 0) num = 0;
            currentQuantity = num;
        }
        UpdateUI();
    }

    void UpdateUI()
    {
        quantityInput.text = currentQuantity.ToString();

        int totalCost = myIngredientData.buyPrice * currentQuantity;

        if (buyButtonText != null)
        {
            buyButtonText.text = $"구매 ({totalCost} G)";
        }

        if (controller != null)
        {
            controller.UpdateBulkTotalCost();
        }
    }

    void OnInstantBuyClick()
    {
        if (currentQuantity <= 0)
        {
            Debug.Log("구매할 수량이 0입니다.");
            return;
        }

        controller.AttemptPurchaseIngredient(myIngredientData, currentQuantity);

        currentQuantity = 0;

        UpdateUI();
    }

    public int GetCurrentQuantity()
    {
        return currentQuantity;
    }

    public IngredientData GetIngredientData()
    {
        return myIngredientData;
    }

    public int GetCurrentTotalCost()
    {
        return myIngredientData.buyPrice * currentQuantity;
    }
}