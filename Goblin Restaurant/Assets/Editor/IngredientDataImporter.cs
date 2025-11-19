using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class IngredientDataImporter
{
    [MenuItem("Tools/Import Ingredient Data from CSV")]
    public static void Import()
    {
        string path = EditorUtility.OpenFilePanel("Select Ingredient CSV", "", "csv");
        if (string.IsNullOrEmpty(path)) return;

        string[] allLines = File.ReadAllLines(path);

        for (int i = 1; i < allLines.Length; i++)
        {
            string[] cells = allLines[i].Split(',');
            if (cells.Length < 4) continue;

            IngredientData ingredient = ScriptableObject.CreateInstance<IngredientData>();

            ingredient.id = cells[0].Trim();
            ingredient.ingredientName = cells[1].Trim();

            ingredient.rarity = GetRarityFromString(cells[2].Trim());

            int.TryParse(cells[3].Trim(), out ingredient.buyPrice);

            AssetDatabase.CreateAsset(ingredient, $"Assets/Resources/Ingredients/{ingredient.id}.asset");
        }
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Import Complete", "Ingredient data imported successfully!", "OK");
    }

    private static Rarity GetRarityFromString(string rarityString)
    {
        switch (rarityString)
        {
            case "일반":
                return Rarity.Common;
            case "고급":
                return Rarity.Uncommon;
            case "희귀":
                return Rarity.Rare;
            case "전설":
                return Rarity.Legendary;
            default:
                Debug.LogWarning($"알 수 없는 희귀도 값입니다: '{rarityString}'. 기본값(Common)으로 설정됩니다.");
                return Rarity.Common;
        }
    }
}