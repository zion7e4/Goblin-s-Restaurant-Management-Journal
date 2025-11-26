using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    public Dictionary<string, int> playerIngredients = new Dictionary<string, int>();

    public HashSet<string> discoveredIngredients = new HashSet<string>();

    void Awake()
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

    public void AddIngredient(string ingredientID, int amount)
    {
        if (playerIngredients.ContainsKey(ingredientID))
        {
            playerIngredients[ingredientID] += amount;
        }
        else
        {
            playerIngredients[ingredientID] = amount;
        }

        if (!discoveredIngredients.Contains(ingredientID))
        {
            discoveredIngredients.Add(ingredientID);
        }

        Debug.Log($"재료 {ingredientID} {amount}개 추가. 현재 수량: {playerIngredients[ingredientID]}");
    }

    public bool IsDiscovered(string ingredientID)
    {
        return discoveredIngredients.Contains(ingredientID);
    }

    /// <summary>
    /// (GameManager가 호출) '식탐' 특성이 발동하면 인벤토리에서 랜덤 재료 1개를 훔칩니다.
    /// </summary>
    public void StealRandomIngredient(string employeeName)
    {
        List<string> availableIngredients = playerIngredients
            .Where(pair => pair.Value > 0)
            .Select(pair => pair.Key)
            .ToList();

        if (availableIngredients.Count == 0)
        {
            Debug.Log($"[식탐] {employeeName}이(가) 재료를 훔치려 했으나... 인벤토리가 비어있습니다!");
            return;
        }

        string stolenIngredientID = availableIngredients[UnityEngine.Random.Range(0, availableIngredients.Count)];
        RemoveIngredients(stolenIngredientID, 1);

        Debug.LogWarning($"[식탐!] {employeeName}이(가) {stolenIngredientID} 1개를 훔쳐 먹었습니다! (남은 수량: {playerIngredients[stolenIngredientID]})");
    }

    /// <summary>
    /// (RecipeManager가 호출) 특정 재료를 인벤토리에서 지정된 수량만큼 제거합니다.
    /// </summary>
    public void RemoveIngredients(string ingredientID, int amount)
    {
        if (playerIngredients.ContainsKey(ingredientID))
        {
            playerIngredients[ingredientID] -= amount;

            if (playerIngredients[ingredientID] < 0)
            {
                Debug.LogWarning($"재료 {ingredientID}의 재고가 부족했지만 차감되었습니다. 현재: 0");
                playerIngredients[ingredientID] = 0;
            }
            Debug.Log($"재료 '{ingredientID}' {amount}개 소모. 남은 수량: {playerIngredients[ingredientID]}");
        }
        else
        {
            Debug.LogError($"재고에 없는 재료({ingredientID})를 소모하려고 합니다.");
        }
    }

    /// <summary>
    /// (RecipeManager가 호출) 특정 재료를 필요한 수량(amountNeeded)만큼 보유하고 있는지 확인합니다.
    /// </summary>
    public bool HasEnoughIngredients(string ingredientID, int amountNeeded)
    {
        if (playerIngredients.TryGetValue(ingredientID, out int currentAmount))
        {
            return currentAmount >= amountNeeded;
        }
        return false;
    }

    public bool CanCook(PlayerRecipe recipe)
    {
        return GetMaxCookableAmount(recipe) > 0;
    }

    public int GetMaxCookableAmount(PlayerRecipe recipe)
    {
        if (recipe == null) return 0;

        int maxAmount = int.MaxValue;
        foreach (var requirement in recipe.data.requiredIngredients)
        {
            playerIngredients.TryGetValue(requirement.ingredientID, out int ownedAmount);

            if (requirement.amount == 0) continue;

            int cookableAmount = ownedAmount / requirement.amount;
            if (cookableAmount < maxAmount)
            {
                maxAmount = cookableAmount;
            }
        }
        return maxAmount;
    }

    // ▼▼▼ [새로 추가됨] UI에서 재료 목록을 가져갈 때 사용하는 함수 ▼▼▼
    public Dictionary<string, int> GetAllIngredients()
    {
        return playerIngredients;
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
}