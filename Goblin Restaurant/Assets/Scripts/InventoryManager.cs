using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    public Dictionary<string, int> playerIngredients = new Dictionary<string, int>();

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
        Debug.Log($"재료 {ingredientID} {amount}개 추가. 현재 수량: {playerIngredients[ingredientID]}");
    }

    // --- ▼▼▼ 함수 이름 수정 ▼▼▼ ---
    // (기존 ConsumeIngredient -> RemoveIngredients로 이름 변경)
    /// <summary>
    /// (RecipeManager가 호출)
    /// 특정 재료를 인벤토리에서 지정된 수량만큼 제거합니다.
    /// </summary>
    public void RemoveIngredients(string ingredientID, int amount)
    {
        if (playerIngredients.ContainsKey(ingredientID))
        {
            playerIngredients[ingredientID] -= amount;

            if (playerIngredients[ingredientID] < 0)
            {
                Debug.LogWarning($"재료 {ingredientID}의 재고가 부족했지만 차감되었습니다. 현재: 0");
                playerIngredients[ingredientID] = 0; // 재고가 마이너스가 되지 않게 0으로 보정
            }
            Debug.Log($"재료 '{ingredientID}' {amount}개 소모. 남은 수량: {playerIngredients[ingredientID]}");
        }
        else
        {
            Debug.LogError($"재고에 없는 재료({ingredientID})를 소모하려고 합니다.");
        }
    }
    // --- ▲▲▲ 함수 이름 수정 완료 ▲▲▲ ---


    // --- ▼▼▼ 함수 새로 추가 ▼▼▼ ---
    /// <summary>
    /// (RecipeManager가 호출)
    /// 특정 재료를 필요한 수량(amountNeeded)만큼 보유하고 있는지 확인합니다.
    /// </summary>
    /// <returns>보유량이 충분하면 true, 아니면 false</returns>
    public bool HasEnoughIngredients(string ingredientID, int amountNeeded)
    {
        // 1. 딕셔너리에서 현재 보유량을 찾습니다.
        if (playerIngredients.TryGetValue(ingredientID, out int currentAmount))
        {
            // 2. 현재 보유량이 필요한 양보다 많거나 같은지 확인합니다.
            return currentAmount >= amountNeeded;
        }

        // 3. 딕셔너리에 재료가 아예 없는 경우
        return false;
    }
    // --- ▲▲▲ 함수 추가 완료 ▲▲▲ ---


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
}