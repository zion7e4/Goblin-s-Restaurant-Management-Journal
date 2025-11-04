using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class RecipeDataImporter
{
    [MenuItem("Tools/Import Recipe Data from CSV")]
    public static void Import()
    {
        string path = EditorUtility.OpenFilePanel("Select Recipe CSV", "", "csv");
        if (string.IsNullOrEmpty(path)) return;

        string[] allLines = File.ReadAllLines(path);
        
        for (int i = 1; i < allLines.Length; i++)
        {
            string[] cells = allLines[i].Split(',');

            RecipeData recipe = ScriptableObject.CreateInstance<RecipeData>();

            recipe.id = int.Parse(cells[0]);
            recipe.recipeName = cells[1];
            recipe.description = cells[2].Replace("\"", "").Trim();
            int.TryParse(cells[3], out recipe.basePrice);
            float.TryParse(cells[4], out recipe.baseCookTime);

            recipe.requiredIngredients = new List<IngredientRequirement>();

            for (int j = 5; j < cells.Length; j++)
            {
                string ingredientCell = cells[j].Replace("\"", "").Trim();
                if (string.IsNullOrWhiteSpace(ingredientCell)) continue;

                string[] parts = ingredientCell.Split(':');
                if (parts.Length == 2)
                {
                    IngredientRequirement req = new IngredientRequirement();
                    req.ingredientID = parts[0].Trim();

                    if (int.TryParse(parts[1].Trim(), out int amount))
                    {
                        req.amount = amount;
                        recipe.requiredIngredients.Add(req);
                    }
                    else
                    {
                        Debug.LogError($"CSV 파일 {i + 1}번째 줄, {j + 1}번째 셀의 재료 수량 변환 실패: '{parts[1]}'");
                    }
                }
            }

            // ScriptableObject 에셋 파일 생성
            AssetDatabase.CreateAsset(recipe, $"Assets/Resources/Recipes/Recipe_{recipe.id}.asset");
        }
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Import Complete", "Recipe data imported successfully!", "OK");
    }
}