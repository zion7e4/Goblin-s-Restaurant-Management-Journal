// Assets/Editor/NewShopPoolParser.cs
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Globalization;
using Codice.Client.Common;

public class NewShopPoolParser : EditorWindow
{
    // CSV 경로 (사용자 환경에 맞게 수정)
    private const string SPECIAL_ING_CSV_PATH = "Assets/Data/CSV/td_shop_pool_table - td_shop_pool_special_ing.csv";
    private const string RECIPE_CSV_PATH = "Assets/Data/CSV/td_shop_pool_table - td_shop_pool_recipe.csv";

    // SO 에셋 경로 (사용자 환경에 맞게 수정)
    private const string SPECIAL_ING_ASSET_PATH = "Assets/Data/ShopPools/SpecialIngredientPool.asset";
    private const string RECIPE_ASSET_PATH = "Assets/Data/ShopPools/RecipePool.asset";

    [MenuItem("Tools/Parse NEW Shop Pools (CSV Spec)")]
    public static void ShowWindow()
    {
        GetWindow<NewShopPoolParser>("New Shop Pool Parser");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Parse ALL Shop Pools (New Spec)"))
        {
            ParseIngredients();
            ParseRecipes();
        }
    }

    private void ParseIngredients()
    {
        SpecialIngredientPoolSO poolSO = AssetDatabase.LoadAssetAtPath<SpecialIngredientPoolSO>(SPECIAL_ING_ASSET_PATH);
        if (poolSO == null) { Debug.LogError("SpecialIngredientPoolSO 에셋을 찾을 수 없습니다: " + SPECIAL_ING_ASSET_PATH); return; }

        string[] lines = File.ReadAllLines(SPECIAL_ING_CSV_PATH);
        poolSO.items.Clear();

        for (int i = 1; i < lines.Length; i++) // 1 (헤더)부터 시작
        {
            string[] values = lines[i].Split(',');
            if (values.Length < 5) continue;

            SpecialIngredientPoolEntry entry = new SpecialIngredientPoolEntry();
            entry.ingredient_id = values[0].Trim();
            entry.min_stock = int.Parse(values[1].Trim());
            entry.max_stock = int.Parse(values[2].Trim());
            // "30%" -> 0.3f로 변환
            entry.price_variance = float.Parse(values[3].Trim().TrimEnd('%'), CultureInfo.InvariantCulture) / 100f;
            entry.appearance_probability = float.Parse(values[4].Trim().TrimEnd('%'), CultureInfo.InvariantCulture) / 100f;

            poolSO.items.Add(entry);
        }
        EditorUtility.SetDirty(poolSO);
        Debug.Log("특수 재료 풀 파싱 완료: " + SPECIAL_ING_ASSET_PATH);
    }

    private void ParseRecipes()
    {
        RecipePoolSO poolSO = AssetDatabase.LoadAssetAtPath<RecipePoolSO>(RECIPE_ASSET_PATH);
        if (poolSO == null) { Debug.LogError("RecipePoolSO 에셋을 찾을 수 없습니다: " + RECIPE_ASSET_PATH); return; }

        string[] lines = File.ReadAllLines(RECIPE_CSV_PATH);
        poolSO.items.Clear();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length < 2) continue;

            RecipePoolEntry entry = new RecipePoolEntry();
            entry.rcp_id = int.Parse(values[0].Trim());
            entry.required_lv = int.Parse(values[1].Trim());

            poolSO.items.Add(entry);
        }
        EditorUtility.SetDirty(poolSO);
        Debug.Log("레시피 풀 파싱 완료: " + RECIPE_ASSET_PATH);
    }
}