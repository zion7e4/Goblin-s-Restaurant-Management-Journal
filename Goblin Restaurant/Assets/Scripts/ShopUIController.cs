using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System.Text;

public class ShopUIController : MonoBehaviour
{
    [Header("���� �� ��ư")]
    public Button recipeTabButton;
    public Button ingredientTabButton;
    public Button todayShopTabButton; // "������ ��ǰ" �� ��ư

    [Header("�� �г�")]
    public GameObject recipeShopPanel; // "������" �� �г�
    public GameObject ingredientShopPanel; // "���" �� �г�
    public GameObject todayShopPanel; // "������ ��ǰ" �� �г�

    [Header("1. ��� �� (�⺻)")]
    public GameObject basicIngredientItemPrefab; // (ShopIngredientItemUI.cs ������)
    public Transform basicIngredientContentParent;
    public Button bulkPurchaseButton; // �ϰ����� ��ư
    public TextMeshProUGUI bulkTotalCostText;
    private List<ShopIngredientItemUI> spawnedBasicItems = new List<ShopIngredientItemUI>();

    [Header("2. ������ ��ǰ �� (Ư�� ���)")]
    public GameObject todayShopItemPrefab; // (TodayShopItemUI.cs ������)
    public Transform todayShopContentParent; // "������ ��ǰ" ��ũ�Ѻ� Content

    [Header("3. ������ �� (������ ������)")]
    public GameObject basicRecipeItemPrefab;
    public Transform todayRecipeContentParent;
    public Transform permanentRecipeContentParent;
    // (todayShopItemPrefab�� ������ �ǿ����� �������� ����մϴ�)

    void Awake()
    {
        recipeTabButton.onClick.AddListener(SwitchToRecipeTab);
        ingredientTabButton.onClick.AddListener(SwitchToIngredientTab);
        todayShopTabButton.onClick.AddListener(SwitchToTodayShopTab); // [�߰�]

        if (bulkPurchaseButton != null)
        {
            bulkPurchaseButton.onClick.AddListener(OnBulkPurchaseClick);
        }
    }

    void OnEnable()
    {
        CloseAllTabs();
    }

    void OnDisable()
    {
        if (TooltipSystem.instance != null) TooltipSystem.instance.Hide();
        spawnedBasicItems.Clear();
        CloseAllTabs();
    }

    private void CloseAllTabs()
    {
        if (recipeShopPanel != null) recipeShopPanel.SetActive(false);
        if (ingredientShopPanel != null) ingredientShopPanel.SetActive(false);
        if (todayShopPanel != null) todayShopPanel.SetActive(false); // [�߰�]

        if (bulkPurchaseButton != null) bulkPurchaseButton.gameObject.SetActive(false);
        if (bulkTotalCostText != null) bulkTotalCostText.gameObject.SetActive(false);
    }

    public void SwitchToRecipeTab()
    {
        recipeShopPanel.SetActive(true);
        ingredientShopPanel.SetActive(false);
        todayShopPanel.SetActive(false); // [�߰�]

        if (bulkPurchaseButton != null) bulkPurchaseButton.gameObject.SetActive(false);
        if (bulkTotalCostText != null) bulkTotalCostText.gameObject.SetActive(false);

        PopulateRecipeTab();
    }

    public void SwitchToIngredientTab()
    {
        recipeShopPanel.SetActive(false);
        ingredientShopPanel.SetActive(true);
        todayShopPanel.SetActive(false); // [�߰�]

        if (bulkPurchaseButton != null) bulkPurchaseButton.gameObject.SetActive(true);
        if (bulkTotalCostText != null) bulkTotalCostText.gameObject.SetActive(true);

        PopulateIngredientTab();
    }

    public void SwitchToTodayShopTab()
    {
        recipeShopPanel.SetActive(false);
        ingredientShopPanel.SetActive(false);
        todayShopPanel.SetActive(true);

        if (bulkPurchaseButton != null) bulkPurchaseButton.gameObject.SetActive(false);
        if (bulkTotalCostText != null) bulkTotalCostText.gameObject.SetActive(false);

        PopulateTodayShopTab();
    }
    void PopulateIngredientTab()
    {
        foreach (Transform child in basicIngredientContentParent) Destroy(child.gameObject);
        spawnedBasicItems.Clear();

        var allIngredients = GameDataManager.instance.GetAllIngredientData();
        var basicIngredients = allIngredients.Where(ing => ing.rarity == Rarity.Common);

        foreach (var ingredientData in basicIngredients)
        {
            GameObject itemGO = Instantiate(basicIngredientItemPrefab, basicIngredientContentParent);
            ShopIngredientItemUI itemUI = itemGO.GetComponent<ShopIngredientItemUI>();
            itemUI.Setup(ingredientData, this);
            spawnedBasicItems.Add(itemUI);
        }
        UpdateBulkTotalCost();
    }

    void PopulateRecipeTab()
    {
        if (permanentRecipeContentParent == null || basicRecipeItemPrefab == null) return;
        foreach (Transform child in permanentRecipeContentParent) Destroy(child.gameObject);

        if (todayRecipeContentParent != null)
        {
            foreach (Transform child in todayRecipeContentParent) Destroy(child.gameObject);
        }

        if (ShopManager.Instance == null || ShopManager.Instance.recipePool == null)
        {
            Debug.LogError("ShopManager �Ǵ� RecipePoolSO�� ������� �ʾҽ��ϴ�.");
            return;
        }

        foreach (var poolEntry in ShopManager.Instance.recipePool.items)
        {
            GameObject itemGO = Instantiate(basicRecipeItemPrefab, permanentRecipeContentParent);
            itemGO.GetComponent<ShopRecipeItemUI>().Setup(poolEntry, this);
        }
    }

    void PopulateTodayShopTab()
    {
        if (todayShopContentParent == null || todayShopItemPrefab == null) return;
        foreach (Transform child in todayShopContentParent) Destroy(child.gameObject);
        if (ShopManager.Instance == null) return;

        foreach (var item in ShopManager.Instance.TodaySpecialIngredients) 
        {
            GameObject itemGO = Instantiate(todayShopItemPrefab, todayShopContentParent);
            itemGO.GetComponent<TodayShopItemUI>().Setup(item, this);
        }
    }

    public void RefreshTodayShopTabs()
    {
        if (ingredientShopPanel.activeSelf)
        {
        }
        else if (recipeShopPanel.activeSelf)
        {
            PopulateRecipeTab();
        }
        else if (todayShopPanel.activeSelf)
        {
            PopulateTodayShopTab();
        }
    }

    public void OnBulkPurchaseClick()
    {
        int totalCost = 0;
        Dictionary<IngredientData, int> itemsToBuy = new Dictionary<IngredientData, int>();

        foreach (var itemUI in spawnedBasicItems)
        {
            int quantity = itemUI.GetCurrentQuantity();
            if (quantity > 0)
            {
                IngredientData data = itemUI.GetIngredientData();
                itemsToBuy.Add(data, quantity);
                totalCost += data.buyPrice * quantity;
            }
        }

        if (totalCost == 0) return;

        if (GameManager.instance.totalGoldAmount >= totalCost)
        {
            GameManager.instance.SpendGold(totalCost);
            StringBuilder sb = new StringBuilder("�ϰ� ���� �Ϸ�:\n");

            foreach (var item in itemsToBuy)
            {
                InventoryManager.instance.AddIngredient(item.Key.id, item.Value);
                sb.AppendLine($"- {item.Key.ingredientName} {item.Value}��");
            }

            sb.Append($"\n�� ����: -{totalCost} G");
            NotificationController.instance.ShowNotification(sb.ToString());
            
            PopulateIngredientTab();
        }
        else
        {
            NotificationController.instance.ShowNotification("��尡 �����մϴ�!");
        }
    }

    public void UpdateBulkTotalCost()
    {
        int totalCost = 0;
        foreach (var itemUI in spawnedBasicItems)
        {
            totalCost += itemUI.GetCurrentTotalCost();
        }

        if (bulkTotalCostText != null) bulkTotalCostText.text = $"���հ�: {totalCost} G";
        if (bulkPurchaseButton != null) bulkPurchaseButton.interactable = totalCost > 0;
    }

    public void AttemptPurchaseIngredient(IngredientData ingredientData, int quantity)
    {
        if (quantity <= 0) return;
        int totalPrice = ingredientData.buyPrice * quantity;
        if (GameManager.instance.totalGoldAmount >= totalPrice)
        {
            GameManager.instance.SpendGold(totalPrice);
            InventoryManager.instance.AddIngredient(ingredientData.id, quantity);
            Debug.Log($"'{ingredientData.ingredientName}' {quantity}�� ���� ����!");

            NotificationController.instance.ShowNotification($"-{totalPrice} G\n ({ingredientData.ingredientName} {quantity}�� ����)");
        }
        else
        {
            Debug.Log("��尡 �����Ͽ� ��Ḧ ������ �� �����ϴ�.");
            NotificationController.instance.ShowNotification("��尡 �����մϴ�!");
        }
    }

    public void AttemptPurchaseRecipe(RecipeData recipeData)
    {
        int price = (int)(recipeData.basePrice * 1.5f);
        if (GameManager.instance.totalGoldAmount >= price)
        {
            GameManager.instance.SpendGold(price);
            RecipeManager.instance.UnlockRecipe(recipeData.id);
            Debug.Log($"'{recipeData.recipeName}' ������ ���� ����!");

            PopulateRecipeTab();

            NotificationController.instance.ShowNotification($"-{price} G\n (������ ����)");
        }
        else
        {
            Debug.Log("��尡 �����Ͽ� �����Ǹ� ������ �� �����ϴ�.");
            NotificationController.instance.ShowNotification("��尡 �����մϴ�!");
        }
    }
}