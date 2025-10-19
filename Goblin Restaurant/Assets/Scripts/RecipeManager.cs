using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RecipeManager : MonoBehaviour
{
    public static RecipeManager instance;

    // 게임에 존재하는 모든 레시피 원본 데이터
    private Dictionary<int, RecipeData> allRecipeData = new Dictionary<int, RecipeData>();

    // 플레이어가 획득하고 성장시킨 레시피 목록
    public Dictionary<int, PlayerRecipe> playerRecipes = new Dictionary<int, PlayerRecipe>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            LoadAllRecipeData();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UnlockRecipe(1001);
        UnlockRecipe(1002);
        UnlockRecipe(1003);
        UnlockRecipe(1003);
        UnlockRecipe(1004);
        UnlockRecipe(1005);
        UnlockRecipe(1006);
        UnlockRecipe(1007);
        UnlockRecipe(1008);
        UnlockRecipe(1009);
        UnlockRecipe(1010);

    }

    private void LoadAllRecipeData()
    {
        RecipeData[] recipes = Resources.LoadAll<RecipeData>("Recipes");
        foreach (RecipeData recipe in recipes)
        {
            allRecipeData[recipe.id] = recipe;
        }
        Debug.Log($"{allRecipeData.Count}개의 레시피 데이터를 로드했습니다.");
    }

    public void UnlockRecipe(int recipeId)
    {
        if (playerRecipes.ContainsKey(recipeId))
        {
            Debug.Log("이미 획득한 레시피입니다. 다른 보상으로 전환됩니다.");
            return;
        }

        if (allRecipeData.ContainsKey(recipeId))
        {
            PlayerRecipe newPlayerRecipe = new PlayerRecipe(allRecipeData[recipeId]);
            playerRecipes[recipeId] = newPlayerRecipe;
            Debug.Log($"새로운 레시피 '{newPlayerRecipe.data.recipeName}' 획득!");
        }
    }

    public void UpgradeRecipe(int recipeId)
    {
        if (playerRecipes.TryGetValue(recipeId, out PlayerRecipe recipeToUpgrade))
        {
            recipeToUpgrade.currentLevel++;
            Debug.Log($"'{recipeToUpgrade.data.recipeName}' 레시피가 {recipeToUpgrade.currentLevel}레벨이 되었습니다!");
        }
    }

}
