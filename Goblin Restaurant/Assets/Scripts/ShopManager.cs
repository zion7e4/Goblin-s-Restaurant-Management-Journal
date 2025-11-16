using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("데이터 풀 (SO)")]
    public SpecialIngredientPoolSO specialIngredientPool;
    public RecipePoolSO recipePool;

    [Header("오늘의 상품 목록 (자동 생성)")]
    public List<GeneratedShopItem> TodaySpecialIngredients = new List<GeneratedShopItem>();

    [Header("상점 설정")]
    public int ingredientSlotCount = 3;
    public int refreshCost = 100;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GenerateTodayItems(float currentFame)
    {
        TodaySpecialIngredients.Clear();

        // 1. 특수 식재료 생성
        if (specialIngredientPool != null)
        {
            // 1. 추첨 가능한 전체 풀 (CSV의 모든 아이템)
            List<SpecialIngredientPoolEntry> availablePool = new List<SpecialIngredientPoolEntry>(specialIngredientPool.items);

            // 2. 3번(ingredientSlotCount) 반복
            for (int i = 0; i < ingredientSlotCount; i++)
            {
                // 3. 가중치 기반으로 1개 추첨
                GeneratedShopItem newItem = GetWeightedRandomIngredient(availablePool);

                if (newItem != null)
                {
                    TodaySpecialIngredients.Add(newItem);

                    // 4. (중복 방지) 추첨된 아이템은 다음 추첨 풀에서 제외
                    var entryToRemove = availablePool.FirstOrDefault(e => e.ingredient_id == newItem.ItemID);
                    if (entryToRemove != null)
                    {
                        availablePool.Remove(entryToRemove);
                    }
                }
            }
        }
    }

    private GeneratedShopItem GetWeightedRandomIngredient(List<SpecialIngredientPoolEntry> availablePool)
    {
        if (availablePool == null || availablePool.Count == 0) return null;

        // 1. 전체 확률(가중치)의 합을 계산
        float totalWeight = availablePool.Sum(item => item.appearance_probability);
        if (totalWeight <= 0) return null;

        // 2. 0 ~ 전체 합 사이의 랜덤 값 선택
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        // 3. 풀을 순회하며 당첨 아이템 검색
        foreach (var entry in availablePool)
        {
            currentWeight += entry.appearance_probability;
            if (randomValue <= currentWeight)
            {
                IngredientData data = GameDataManager.instance.GetIngredientDataById(entry.ingredient_id);
                if (data == null) return null; // 데이터 매칭 실패

                // 가격 및 재고 계산
                int basePrice = data.buyPrice;
                float variance = 1f + Random.Range(-entry.price_variance, entry.price_variance);
                int finalPrice = Mathf.RoundToInt(basePrice * variance);
                int stock = Random.Range(entry.min_stock, entry.max_stock + 1);

                return new GeneratedShopItem(data, basePrice, finalPrice, stock);
            }
        }
        return null;
    }

    public void RefreshTodayShop()
    {
        if (GameManager.instance.totalGoldAmount >= refreshCost)
        {
            GameManager.instance.SpendGold(refreshCost);
            GenerateTodayItems(FameManager.instance.CurrentFamePoints);
            NotificationController.instance.ShowNotification($"상점 목록 새로고침!\n-{refreshCost} G");
        }
        else
        {
            NotificationController.instance.ShowNotification("새로고침 비용이 부족합니다!");
        }
    }

    public bool PurchaseItem(GeneratedShopItem item, int quantity)
    {
        if (item.isSoldOut) return false;

        // 1. 재고 확인
        if (item.CurrentStock < quantity)
        {
            NotificationController.instance.ShowNotification("재고가 부족합니다!");
            return false;
        }

        // 2. 총 가격 계산
        int totalPrice = item.CurrentPrice * quantity;

        // 3. 골드 확인
        if (GameManager.instance.totalGoldAmount >= totalPrice)
        {
            GameManager.instance.SpendGold(totalPrice);

            if (item.ItemType == ShopItemType.Ingredient)
            {
                // 재료 구매
                InventoryManager.instance.AddIngredient(item.ItemID, quantity);
                item.CurrentStock -= quantity; // 재고 차감
                NotificationController.instance.ShowNotification($"{item.ingredientData.ingredientName} {quantity}개 구매!");
            }
            else // Recipe
            {
                // 레시피 구매 (quantity는 1임)
                RecipeManager.instance.UnlockRecipe(item.recipeData.id);
                item.CurrentStock = 0; // 즉시 품절
                NotificationController.instance.ShowNotification($"레시피 '{item.recipeData.recipeName}' 획득!");
            }
            return true; // 구매 성공
        }
        else
        {
            NotificationController.instance.ShowNotification("골드가 부족합니다!");
            return false; // 골드 부족
        }
    }
}