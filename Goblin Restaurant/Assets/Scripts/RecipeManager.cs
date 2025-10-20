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
        UnlockRecipe(1001);
        UnlockRecipe(1002);
    }

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

    public void UpgradeRecipe(int recipeId)
    {
        if (playerRecipes.TryGetValue(recipeId, out PlayerRecipe recipeToUpgrade))
        {
            // 재료 및 골드 소모 조건 확인 로직 (나중에 구현)
            recipeToUpgrade.currentLevel++;
            Debug.Log($"'{recipeToUpgrade.data.recipeName}' 레시피가 {recipeToUpgrade.currentLevel}레벨이 되었습니다!");
        }
    }
}
