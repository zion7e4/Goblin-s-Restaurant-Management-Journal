using UnityEngine;
using System.Collections.Generic;

public class RecipeManager : MonoBehaviour
{
    public static RecipeManager instance;

    public Dictionary<int, PlayerRecipe> playerRecipes = new Dictionary<int, PlayerRecipe>();

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
        // (테스트용)
        UnlockRecipe(1001);
    }

    /// <summary>
    /// 새 레시피를 획득하여 playerRecipes 딕셔너리에 추가합니다.
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
        }
    }

    /// <summary>
    /// 레시피 강화를 시도합니다. (데이터 테이블 기반)
    /// </summary>
    /// <returns>강화 성공 시 true, 실패 시 false</returns>
    public bool UpgradeRecipe(int recipeId)
    {
        Debug.Log($"[RecipeManager] UpgradeRecipe({recipeId}) 함수 진입.");

        if (!playerRecipes.TryGetValue(recipeId, out PlayerRecipe recipeToUpgrade))
        {
            Debug.Log("[RecipeManager] 강화 실패: 보유하지 않은 레시피.");
            return false;
        }

        RecipeData baseData = recipeToUpgrade.data;
        int currentLevel = recipeToUpgrade.currentLevel;
        int nextLevel = currentLevel + 1;

        // ▼▼▼ [수정됨] GameDataManager에서 레벨업 정보 가져오기 ▼▼▼
        // (임시 공식 삭제)
        RecipeLevelEntry nextLevelData = GameDataManager.instance.GetRecipeLevelData(nextLevel);

        // 1. 레벨 데이터 확인
        if (nextLevelData == null)
        {
            Debug.Log($"[RecipeManager] 강화 실패: 최대 레벨(Lv.{currentLevel})이거나, 레벨 {nextLevel}의 데이터를 RecipeLevelTable에서 찾을 수 없습니다.");
            return false;
        }

        // 2. 필요 골드 및 재료 배수 가져오기
        int requiredGold = nextLevelData.Required_Gold;
        int materialMultiplier = nextLevelData.Required_Item_Count;
        // ▲▲▲ [수정 완료] ▲▲▲

        Debug.Log($"[RecipeManager] 필요 재화 계산: {requiredGold} 골드, 재료 배수 x{materialMultiplier}");
        Debug.Log($"[RecipeManager] 현재 보유 골드: {GameManager.instance.totalGoldAmount} G");

        // 3. 조건 확인: 재화 (골드)
        if (GameManager.instance.totalGoldAmount < requiredGold)
        {
            Debug.Log($"[RecipeManager] 강화 실패: 골드가 부족합니다. (필요: {requiredGold} G)");
            return false;
        }

        // 4. 조건 확인: 재료
        foreach (IngredientRequirement req in baseData.requiredIngredients)
        {
            int requiredAmount = req.amount * materialMultiplier;

            Debug.Log($"[RecipeManager] 재료 확인 중: {req.ingredientID} (필요 수량: {requiredAmount})");

            if (!InventoryManager.instance.HasEnoughIngredients(req.ingredientID, requiredAmount))
            {
                Debug.Log($"[RecipeManager] 강화 실패: {req.ingredientID} 재료가 부족합니다. (필요: {requiredAmount} 개)");
                return false;
            }
        }

        Debug.Log("[RecipeManager] 모든 조건 통과. 강화 실행.");

        // 5. 재화/재료 차감
        GameManager.instance.SpendGold(requiredGold);
        foreach (IngredientRequirement req in baseData.requiredIngredients)
        {
            int requiredAmount = req.amount * materialMultiplier;
            InventoryManager.instance.RemoveIngredients(req.ingredientID, requiredAmount);
        }

        // 6. 레벨 +1
        recipeToUpgrade.currentLevel++;

        Debug.Log($"'{baseData.recipeName}' 레시피가 {recipeToUpgrade.currentLevel}레벨이 되었습니다!");
        return true;
    }
}