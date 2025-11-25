using UnityEngine;
using System.Collections.Generic;
using System; // Action 사용을 위해 추가

public class RecipeManager : MonoBehaviour
{
    public static RecipeManager instance;
    public Dictionary<int, PlayerRecipe> playerRecipes = new Dictionary<int, PlayerRecipe>();

    // 레시피 획득/강화 시 UI에 알리는 이벤트
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
        UnlockRecipe(1001);
    }

    /// <summary>
    /// 새 레시피를 획득합니다. (상점 구매 시 호출됨)
    /// </summary>
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

    /// <summary>
    /// 레시피 강화를 시도합니다.
    /// </summary>
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

        if (nextLevelData == null)
        {
            Debug.Log("[RecipeManager] 강화 실패: 최대 레벨이거나 데이터 없음");
            return false;
        }

        int requiredGold = nextLevelData.Required_Gold;
        int materialMultiplier = nextLevelData.Required_Item_Count;

        if (GameManager.instance.totalGoldAmount < requiredGold)
        {
            Debug.Log($"[RecipeManager] 강화 실패: 골드 부족 (필요: {requiredGold})");
            return false;
        }

        foreach (IngredientRequirement req in baseData.requiredIngredients)
        {
            int requiredAmount = req.amount * materialMultiplier;
            if (!InventoryManager.instance.HasEnoughIngredients(req.ingredientID, requiredAmount))
            {
                Debug.Log($"[RecipeManager] 강화 실패: 재료 부족 ({req.ingredientID})");
                return false;
            }
        }

        // 재화 소모 및 강화 실행
        GameManager.instance.SpendGold(requiredGold);
        foreach (IngredientRequirement req in baseData.requiredIngredients)
        {
            int requiredAmount = req.amount * materialMultiplier;
            InventoryManager.instance.RemoveIngredients(req.ingredientID, requiredAmount);
        }

        recipeToUpgrade.currentLevel++;
        Debug.Log($"강화 성공! Lv.{recipeToUpgrade.currentLevel}");

        // UI에게 정보 갱신 알림
        onRecipeUpdated?.Invoke();

        return true;
    }
}