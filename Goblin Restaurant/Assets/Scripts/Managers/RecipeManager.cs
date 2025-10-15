using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 역할: 게임에 존재하는 모든 레시피와 플레이어가 소유한 레시피를 관리하는 중앙 관리자입니다.
public class RecipeManager : MonoBehaviour
{
    public static RecipeManager Instance { get; private set; }

    [Header("레시피 데이터베이스")]
    [Tooltip("게임에 존재하는 모든 레시피의 원본(.asset) 목록")]
    public List<RecipeData> allRecipesInGame;

    [Header("플레이어 데이터")]
    [Tooltip("플레이어가 현재 소유하고 있는 레시피 목록")]
    public List<RecipeInstance> ownedRecipes = new List<RecipeInstance>();

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

    /// <summary>
    /// 새로운 레시피를 획득하여 '레시피 도감'(ownedRecipes)에 추가합니다.
    /// </summary>
    /// <param name="recipeToLearn">획득할 레시피의 원본 데이터</param>
    public void LearnNewRecipe(RecipeData recipeToLearn)
    {
        // 이미 배운 레시피인지 확인합니다.
        if (ownedRecipes.Any(r => r.BaseData == recipeToLearn))
        {
            Debug.LogWarning($"{recipeToLearn.recipeName}은(는) 이미 배운 레시피입니다!");
            // TODO: 기획서에 따라 골드나 재료로 변환하여 지급하는 로직 추가
            return;
        }

        // 새로운 레시피 인스턴스를 생성하여 목록에 추가합니다.
        RecipeInstance newRecipe = new RecipeInstance(recipeToLearn);
        ownedRecipes.Add(newRecipe);
        Debug.Log($"새로운 레시피 '{newRecipe.BaseData.recipeName}'을(를) 배웠습니다!");
    }
    // RecipeManager.cs의 Start() 함수에 추가
    void Start()
    {
        if (allRecipesInGame.Count > 0)
        {
            LearnNewRecipe(allRecipesInGame[0]);
        }
    }
}
