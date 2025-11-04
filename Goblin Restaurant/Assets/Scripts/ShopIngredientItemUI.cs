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

    private int currentQuantity = 1;
    private IngredientData myIngredientData;
    private ShopUIController controller;

    public void Setup(IngredientData ingredientData, ShopUIController shopController)
    {
        myIngredientData = ingredientData;
        controller = shopController;

        ingredientIcon.sprite = ingredientData.icon;
        ingredientNameText.text = ingredientData.ingredientName;
        priceText.text = ingredientData.buyPrice.ToString() + " G";

        buyButton.onClick.AddListener(OnBuyButtonClick);
        plusButton.onClick.AddListener(() => ChangeQuantity(1));
        minusButton.onClick.AddListener(() => ChangeQuantity(-1));
        quantityInput.onValueChanged.AddListener(OnInputValueChanged);

        UpdateUI();
    }

    void ChangeQuantity(int amount)
    {
        currentQuantity += amount;
        if (currentQuantity < 1) currentQuantity = 1;
        UpdateUI();
    }

    void OnInputValueChanged(string value)
    {
        if (int.TryParse(value, out int num))
        {
            if (num < 1) num = 1;
            currentQuantity = num;
        }
    }

    void UpdateUI()
    {
        quantityInput.text = currentQuantity.ToString();

        int totalCost = myIngredientData.buyPrice * currentQuantity;

        if (buyButtonText != null)
        {
            buyButtonText.text = $"±¸¸Å ({totalCost} G)";
        }
    }

    void OnBuyButtonClick()
    {
        controller.AttemptPurchaseIngredient(myIngredientData, currentQuantity);
        currentQuantity = 1;
        UpdateUI();
    }
}
