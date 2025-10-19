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
            AddIngredient("ING01", 100);
            AddIngredient("ING02", 100);
            AddIngredient("ING03", 100);
            AddIngredient("ING04", 100);
            AddIngredient("ING10", 100);
            AddIngredient("ING11", 100);
            AddIngredient("ING12", 100);
            AddIngredient("ING20", 100);
            AddIngredient("ING21", 100);
            AddIngredient("ING30", 100);
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
        Debug.Log($"재료 {ingredientID} {amount}개 추가/소모. 현재 수량: {playerIngredients[ingredientID]}");
    }

    public void ConsumeIngredient(string ingredientID, int amount)
    {
        if (playerIngredients.ContainsKey(ingredientID))
        {
            playerIngredients[ingredientID] -= amount;

            if (playerIngredients[ingredientID] < 0)
            {
                Debug.LogWarning($"재료 {ingredientID}의 재고가 부족했지만 차감되었습니다. 현재: {playerIngredients[ingredientID]}");
                playerIngredients[ingredientID] = 0;
            }
            Debug.Log($"재료 '{ingredientID}' {amount}개 소모. 남은 수량: {playerIngredients[ingredientID]}");
        }
        else
        {
            Debug.LogError($"재고에 없는 재료({ingredientID})를 소모하려고 합니다.");
        }
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