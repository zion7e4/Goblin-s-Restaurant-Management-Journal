using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;


public class TodayShopItemUI : MonoBehaviour
{
    [Header("UI References")]
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI priceText;
    [Tooltip("재고 텍스트 (예: 5 / 5)")]
    public TextMeshProUGUI stockText;
    public Button buyButton;
    public GameObject soldOutOverlay;

    [Header("Rarity & Price Change")]
    public Image borderImage;
    public List<RarityBorder> rarityBorders;
    public GameObject upArrowIcon;
    public GameObject downArrowIcon;
    public TextMeshProUGUI priceChangeText;
    public Color priceIncreaseColor = Color.red;
    public Color priceDecreaseColor = Color.blue;

    [Header("Quantity Controls (Ingredients Only)")]
    [Tooltip("수량 조절 UI (버튼, 텍스트)를 감싸는 부모 오브젝트")]
    public GameObject quantityControlGroup;
    public Button plusButton;
    public Button minusButton;
    [Tooltip("선택한 수량을 표시할 텍스트")]
    public TMP_InputField selectedQuantityText;
    [Tooltip("구매 버튼에 총 가격을 표시할 텍스트")]
    public TextMeshProUGUI buyButtonText;

    private GeneratedShopItem currentItem;
    private ShopUIController shopController;

    private int currentSelectedQuantity = 0;

    public void Setup(GeneratedShopItem item, ShopUIController controller)
    {
        this.currentItem = item;
        this.shopController = controller;

        Rarity rarity = Rarity.Common;

        if (item.ingredientData != null)
        {
            // --- 재료 아이템 ---
            itemIcon.sprite = item.ingredientData.icon;
            itemNameText.text = item.ingredientData.ingredientName;
            rarity = item.ingredientData.rarity;

            if (quantityControlGroup) quantityControlGroup.SetActive(true);
            currentSelectedQuantity = 0;

            // 리스너 연결
            if (plusButton) plusButton.onClick.AddListener(() => ChangeQuantity(1));
            if (minusButton) minusButton.onClick.AddListener(() => ChangeQuantity(-1));

            UpdateQuantityUI();
        }
        else if (item.recipeData != null)
        {
            // --- 레시피 아이템 ---
            itemIcon.sprite = item.recipeData.icon;
            itemNameText.text = item.recipeData.recipeName;
            rarity = item.recipeData.rarity;

            if (quantityControlGroup) quantityControlGroup.SetActive(false);
            if (stockText) stockText.text = "레시피";
            if (buyButtonText) buyButtonText.text = "구매";
        }

        priceText.text = $"{item.CurrentPrice}";

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClick);

        UpdateSoldOutStatus();
        UpdateRarityBorder(rarity);
        UpdatePriceChangeUI();
    }

    void OnBuyClick()
    {
        int quantityToBuy = 1;
        if (currentItem.ItemType == ShopItemType.Ingredient)
        {
            quantityToBuy = currentSelectedQuantity;
        }

        bool success = ShopManager.Instance.PurchaseItem(currentItem, quantityToBuy);

        if (success)
        {
            shopController.RefreshTodayShopTabs();
        }
    }

    void UpdateSoldOutStatus()
    {
        if (currentItem.isSoldOut)
        {
            buyButton.interactable = false;
            if (soldOutOverlay != null) soldOutOverlay.SetActive(true);
            if (quantityControlGroup) quantityControlGroup.SetActive(false);

            if (currentItem.recipeData != null)
            {
                if (stockText) stockText.text = "보유 중";
            }
            else
            {
                if (stockText) stockText.text = "매진";
            }
        }
    }

    void UpdateRarityBorder(Rarity rarity)
    {
        if (borderImage != null && rarityBorders != null && rarityBorders.Count > 0)
        {
            Sprite targetSprite = rarityBorders.FirstOrDefault(b => b.rarity == rarity).borderSprite;
            if (targetSprite != null)
            {
                borderImage.sprite = targetSprite;
                borderImage.gameObject.SetActive(true);
            }
            else
            {
                borderImage.gameObject.SetActive(false);
            }
        }
    }

    void UpdatePriceChangeUI()
    {
        if (upArrowIcon) upArrowIcon.SetActive(false);
        if (downArrowIcon) downArrowIcon.SetActive(false);
        if (priceChangeText) priceChangeText.text = "";

        if (currentItem.BasePrice == 0 || currentItem.BasePrice == currentItem.CurrentPrice)
        {
            return;
        }

        float priceDifference = currentItem.CurrentPrice - currentItem.BasePrice;
        float percentage = (priceDifference / currentItem.BasePrice) * 100f;

        if (percentage > 0)
        {
            if (upArrowIcon) upArrowIcon.SetActive(true);
            if (priceChangeText)
            {
                priceChangeText.text = $"(+{percentage:F0}%)";
                priceChangeText.color = priceIncreaseColor;
            }
        }
        else if (percentage < 0)
        {
            if (downArrowIcon) downArrowIcon.SetActive(true);
            if (priceChangeText)
            {
                priceChangeText.text = $"({percentage:F0}%)";
                priceChangeText.color = priceDecreaseColor;
            }
        }
    }

    void ChangeQuantity(int amount)
    {
        if (currentItem.isSoldOut) return;

        int newQuantity = currentSelectedQuantity + amount;
        newQuantity = Mathf.Clamp(newQuantity, 0, currentItem.CurrentStock);

        if (newQuantity != currentSelectedQuantity)
        {
            currentSelectedQuantity = newQuantity;
            UpdateQuantityUI();
        }
    }

    void UpdateQuantityUI()
    {
        if (currentItem == null || currentItem.isSoldOut) return;

        if (selectedQuantityText)
        {
            selectedQuantityText.text = currentSelectedQuantity.ToString();
        }

        if (stockText)
        {
            stockText.text = $"{currentItem.CurrentStock} / {currentItem.InitialStock}";
        }

        if (buyButtonText)
        {
            int totalPrice = currentItem.CurrentPrice * currentSelectedQuantity;
            buyButtonText.text = $"구매 ({totalPrice} G)";
        }

        if (minusButton) minusButton.interactable = currentSelectedQuantity > 0;
        if (plusButton) plusButton.interactable = currentSelectedQuantity < currentItem.CurrentStock;
    }
}