using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager instance;

    private Dictionary<int, RecipeData> allRecipeData = new Dictionary<int, RecipeData>();
    private Dictionary<string, IngredientData> allIngredientData = new Dictionary<string, IngredientData>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            LoadAllGameData();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadAllGameData()
    {
        RecipeData[] recipes = Resources.LoadAll<RecipeData>("Recipes");
        foreach (RecipeData recipe in recipes)
        {
            allRecipeData[recipe.id] = recipe;
        }
        Debug.Log($"{allRecipeData.Count}개의 레시피 데이터를 로드했습니다.");

        IngredientData[] ingredients = Resources.LoadAll<IngredientData>("Ingredients");
        foreach (IngredientData ingredient in ingredients)
        {
            allIngredientData[ingredient.id] = ingredient;
        }
        Debug.Log($"{allIngredientData.Count}개의 재료 데이터를 로드했습니다.");
    }


    public RecipeData GetRecipeDataById(int id)
    {
        allRecipeData.TryGetValue(id, out RecipeData data);
        return data;
    }

    public IngredientData GetIngredientDataById(string id)
    {
        allIngredientData.TryGetValue(id, out IngredientData data);
        return data;
    }

    public List<RecipeData> GetAllRecipeData()
    {
        return allRecipeData.Values.ToList();
    }

    public List<IngredientData> GetAllIngredientData()
    {
        return allIngredientData.Values.ToList();
    }
}
