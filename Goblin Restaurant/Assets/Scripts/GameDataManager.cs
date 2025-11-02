using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager instance;

    [Header("Game Data Tables")]
    [SerializeField] private List<RecipeData> allRecipeDataList;
    public RecipeLevelTable recipeLevelTable;

    // ▼▼▼ 1. 재료 리스트 변수 추가 (이름은 실제 변수명에 맞게 수정) ▼▼▼
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

    // --- ▼▼▼ 2. 이 함수가 새로 추가되었습니다 ▼▼▼ ---
    // (InventoryUIController가 이 함수를 찾고 있습니다)

    /// <summary>
    /// 모든 재료 데이터 리스트를 반환합니다.
    /// </summary>
    public List<IngredientData> GetAllIngredientData()
    {
        // (TODO: 이 부분은 실제 재료 리스트 변수명에 맞게 수정하세요)
        if (allIngredientDataList == null)
        {
            allIngredientDataList = new List<IngredientData>();
        }
        return allIngredientDataList;
    }
    // --- ▲▲▲ 함수 추가 완료 ▲▲▲ ---


    /// <summary>
    /// ID로 특정 레시피 데이터를 찾아서 반환합니다.
    /// </summary>
    public RecipeData GetRecipeDataById(int id)
    {
        if (allRecipeDataList == null) return null;

        return allRecipeDataList.Find(recipe => recipe.id == id);
    }

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