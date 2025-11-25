using UnityEngine;
using System.Collections.Generic;
using System;

public class RecipeManager : MonoBehaviour
{
    public static RecipeManager instance;
    public Dictionary<int, PlayerRecipe> playerRecipes = new Dictionary<int, PlayerRecipe>();

    // 레시피 획득 또는 강화 시 UI에 알리는 이벤트
    public event Action onRecipeUpdated;

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

    void Start()
    {
        // 테스트용 기본 레시피 획득
        UnlockRecipe(1001);
    }

    // 새 레시피 획득 (상점 구매 시 호출)
    public void UnlockRecipe(int recipeId)
    {
        if (playerRecipes.ContainsKey(recipeId))
        {
            Debug.Log("이미 획득한 레시피입니다.");
            return;
        }

        RecipeData recipeData = GameDataManager.instance.GetRecipeDataById(recipeId);
        if (recipeData != null)
        {
            PlayerRecipe newPlayerRecipe = new PlayerRecipe(recipeData);
            playerRecipes[recipeId] = newPlayerRecipe;
            Debug.Log($"새로운 레시피 '{newPlayerRecipe.data.recipeName}' 획득!");

            // UI에게 목록 갱신 알림
            onRecipeUpdated?.Invoke();
        }
    }

    // 레시피 강화 시도
    public bool UpgradeRecipe(int recipeId)
    {
        Debug.Log($"[RecipeManager] 강화 시도: ID {recipeId}");

        if (!playerRecipes.TryGetValue(recipeId, out PlayerRecipe recipeToUpgrade))
        {
            Debug.Log("[RecipeManager] 강화 실패: 미보유 레시피");
            return false;
        }

        RecipeData baseData = recipeToUpgrade.data;
        int currentLevel = recipeToUpgrade.currentLevel;
        int nextLevel = currentLevel + 1;

        RecipeLevelEntry nextLevelData = GameDataManager.instance.GetRecipeLevelData(nextLevel);

        // 1. 데이터 확인
        if (nextLevelData == null)
        {
            Debug.Log("[RecipeManager] 강화 실패: 최대 레벨이거나 데이터 없음");
            return false;
        }

        int requiredGold = nextLevelData.Required_Gold;
        int materialMultiplier = nextLevelData.Required_Item_Count;

        // 2. 골드 확인
        if (GameManager.instance.totalGoldAmount < requiredGold)
        {
            Debug.Log($"[RecipeManager] 강화 실패: 골드 부족 (필요: {requiredGold})");
            return false;
        }

        // 3. 재료 확인
        foreach (IngredientRequirement req in baseData.requiredIngredients)
        {
            int requiredAmount = req.amount * materialMultiplier;
            if (!InventoryManager.instance.HasEnoughIngredients(req.ingredientID, requiredAmount))
            {
                Debug.Log($"[RecipeManager] 강화 실패: 재료 부족 ({req.ingredientID})");
                return false;
            }
        }

        // 4. 재화 및 재료 소모
        GameManager.instance.SpendGold(requiredGold);
        foreach (IngredientRequirement req in baseData.requiredIngredients)
        {
            int requiredAmount = req.amount * materialMultiplier;
            InventoryManager.instance.RemoveIngredients(req.ingredientID, requiredAmount);
        }

        // 5. 성공 확률 체크 (기획서 반영)
        int randomValue = UnityEngine.Random.Range(0, 100); // 0 ~ 99
        if (randomValue >= nextLevelData.SuccessRate)
        {
            Debug.Log($"강화 실패! (확률: {nextLevelData.SuccessRate}%)");
            // 실패해도 재화는 소모되었으므로 UI 갱신 필요
            onRecipeUpdated?.Invoke();
            return false;
        }

        // 6. 강화 성공 (레벨 업)
        recipeToUpgrade.currentLevel++;
        Debug.Log($"강화 성공! Lv.{recipeToUpgrade.currentLevel}");

        // UI 갱신 알림
        onRecipeUpdated?.Invoke();

        return true;
    }

    // [추가됨] 현재 레벨에 따른 판매 가격 계산 함수
    // 손님이 돈을 낼 때 이 함수를 호출해서 가격을 받아가세요.
    public int GetRecipeSellingPrice(int recipeId)
    {
        if (!playerRecipes.TryGetValue(recipeId, out PlayerRecipe playerRecipe))
        {
            // 미보유 시 기본 데이터에서 가격 가져옴 (안전장치)
            RecipeData data = GameDataManager.instance.GetRecipeDataById(recipeId);
            return data != null ? data.basePrice : 0;
        }

        int currentLevel = playerRecipe.currentLevel;
        int basePrice = playerRecipe.data.basePrice;

        // 성장 테이블에서 가격 상승률 가져오기
        RecipeLevelEntry levelData = GameDataManager.instance.GetRecipeLevelData(currentLevel);

        if (levelData != null)
        {
            // 기본가격 * (1 + 가격상승률)
            return (int)(basePrice * (1.0f + levelData.Price_Growth_Rate));
        }

        return basePrice;
    }
}