using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using System.Collections.Generic;

public class MenuPlanner : MonoBehaviour
{
    public static MenuPlanner instance;
    public bool isSoldOut = false;
    public PlayerRecipe[] dailyMenu = new PlayerRecipe[5];
    public Dictionary<int, int> dailyMenuQuantities = new Dictionary<int, int>();
    public Dictionary<int, int> dailyMenuSalesTracker = new Dictionary<int, int>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetDailyMenu(int slotIndex, PlayerRecipe recipe)
    {
        if (slotIndex >= 0 && slotIndex < 5)
        {
            dailyMenu[slotIndex] = recipe;
        }
    }

    public void ClearDailyMenu()
    {
        dailyMenu = new PlayerRecipe[5];

        dailyMenuQuantities.Clear();
    }

    public void SetDailyMenu(int slotIndex, PlayerRecipe recipe, int quantity)
    {
        if (slotIndex >= 0 && slotIndex < 5)
        {
            if (dailyMenu[slotIndex] != null)
            {
                dailyMenuQuantities.Remove(dailyMenu[slotIndex].data.id);
            }

            dailyMenu[slotIndex] = recipe;
            if (recipe != null)
            {
                dailyMenuQuantities[recipe.data.id] = quantity;
            }
        }
    }

    public void SetQuantity(int recipeId, int quantity)
    {
        if (dailyMenuQuantities.ContainsKey(recipeId))
        {
            dailyMenuQuantities[recipeId] = quantity;
        }
    }

    public int GetQuantity(int recipeId)
    {
        dailyMenuQuantities.TryGetValue(recipeId, out int quantity);
        return quantity;
    }

    public void ConsumeIngredientsForToday()
    {
        Debug.Log("오늘의 메뉴에 맞춰 재료를 소모합니다.");

        // '오늘의 메뉴' 배열을 순회합니다.
        foreach (PlayerRecipe recipe in dailyMenu)
        {
            // 빈 슬롯은 건너뜁니다.
            if (recipe == null) continue;

            // 해당 레시피의 판매 수량을 가져옵니다.
            int quantity = GetQuantity(recipe.data.id);

            // 해당 레시피에 필요한 각 재료를 순회합니다.
            foreach (IngredientRequirement requirement in recipe.data.requiredIngredients)
            {
                // 총 필요 수량 계산 (재료 요구량 * 판매 수량)
                int totalAmountNeeded = requirement.amount * quantity;

                // InventoryManager에 재료 소모를 요청합니다.
                if (totalAmountNeeded > 0)
                {
                    InventoryManager.instance.RemoveIngredients(requirement.ingredientID, totalAmountNeeded);
                }
            }
        }
    }

    public void StartDaySales()
    {
        dailyMenuSalesTracker.Clear();
        foreach (var item in dailyMenuQuantities)
        {
            dailyMenuSalesTracker[item.Key] = item.Value;
        }
        Debug.Log($"오늘의 판매 재고가 확정되었습니다. (총 {dailyMenuSalesTracker.Count} 종류)");
    }

    public void RecordSale(int recipeId)
    {
        if (dailyMenuSalesTracker.ContainsKey(recipeId) && dailyMenuSalesTracker[recipeId] > 0)
        {
            dailyMenuSalesTracker[recipeId]--;
            Debug.Log($"[판매 기록] ID {recipeId} 메뉴 판매. 남은 재고: {dailyMenuSalesTracker[recipeId]}");
        }

        if (!AreAnyItemsLeftToSell())
        {
            isSoldOut = true;
            NotificationController.instance.ShowNotification("준비한 메뉴가 모두 판매되었습니다! (완판)");
        }
    }

    public int GetRemainingStock(int recipeId)
    {
        dailyMenuSalesTracker.TryGetValue(recipeId, out int stock);
        return stock;
    }

    public bool AreAnyItemsLeftToSell()
    {
        // 재고(Value)가 0보다 큰 아이템이 하나라도(Any) 있으면 true 반환
        return dailyMenuSalesTracker.Values.Any(stock => stock > 0);
    }
}
