using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager instance;

    [Header("Game Data Tables")]
    [SerializeField] private List<RecipeData> allRecipeDataList;
    public RecipeLevelTable recipeLevelTable;

    // (이 리스트는 이전에 연결했습니다)
    [SerializeField] private List<IngredientData> allIngredientDataList;

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

    /// <summary>
    /// 모든 레시피 데이터 리스트를 반환합니다.
    /// </summary>
    public List<RecipeData> GetAllRecipeData()
    {
        if (allRecipeDataList == null)
        {
            allRecipeDataList = new List<RecipeData>();
        }
        return allRecipeDataList;
    }

    /// <summary>
    /// 모든 재료 데이터 리스트를 반환합니다.
    /// </summary>
    public List<IngredientData> GetAllIngredientData()
    {
        if (allIngredientDataList == null)
        {
            allIngredientDataList = new List<IngredientData>();
        }
        return allIngredientDataList;
    }

    /// <summary>
    /// ID로 특정 레시피 데이터를 찾아서 반환합니다.
    /// </summary>
    public RecipeData GetRecipeDataById(int id)
    {
        if (allRecipeDataList == null) return null;

        return allRecipeDataList.Find(recipe => recipe.id == id);
    }

    // --- ▼▼▼ 1. 이 함수가 새로 추가되었습니다 ▼▼▼ ---

    /// <summary>
    /// ID로 특정 '재료' 데이터를 찾아서 반환합니다.
    /// (RecipeBook_UI가 재료 이름을 표시하기 위해 호출)
    /// </summary>
    public IngredientData GetIngredientDataById(string id)
    {
        if (allIngredientDataList == null)
        {
            Debug.LogError("GameDataManager에 allIngredientDataList가 비어있습니다.");
            return null;
        }

        // (가정: IngredientData.cs 스크립트에도 'id'라는 이름의 string 변수가 있다고 가정)
        // (만약 'ingredientID'라면 'item.id == id'를 'item.ingredientID == id'로 수정하세요)
        return allIngredientDataList.Find(item => item.id == id);
    }
    // --- ▲▲▲ 함수 추가 완료 ▲▲▲ ---


    /// <summary>
    /// 레벨 테이블에서 특정 레벨(level)에 대한 강화 데이터를 반환합니다.
    /// </summary>
    public RecipeLevelEntry GetRecipeLevelData(int level)
    {
        if (recipeLevelTable == null)
        {
            Debug.LogError("RecipeLevelTable이 GameDataManager에 연결되지 않았습니다!");
            return null;
        }

        RecipeLevelEntry entry = recipeLevelTable.levelEntries.Find(e => e.Level == level);

        if (entry == null)
        {
            Debug.LogWarning($"RecipeLevelTable에서 Level {level}에 해당하는 데이터를 찾을 수 없습니다.");
        }

        return entry;
    }
}