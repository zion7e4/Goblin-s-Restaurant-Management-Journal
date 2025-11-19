using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class IngredientDataImporter : EditorWindow
{
    // CSV 파일 경로
    private const string csvFilePath = "Assets/Data/CSV/Ingredient_Table - Ingredient_Table.csv";
    private const string savePath = "Assets/Resources/Ingredients/";

    [MenuItem("Tools/Import Ingredient Data")]
    public static void ImportData()
    {
        if (!File.Exists(csvFilePath))
        {
            Debug.LogError($"CSV file not found at {csvFilePath}");
            return;
        }

        string[] lines = File.ReadAllLines(csvFilePath);

        // 첫 번째 줄(헤더)은 건너뜀
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            // 쉼표로 분리
            string[] values = line.Split(',');

            // ▼▼▼ [수정] 열 개수 확인 (총 7개 열이 있어야 함) ▼▼▼
            if (values.Length < 7)
            {
                Debug.LogWarning($"Skipping invalid line {i + 1}: {line} (컬럼 부족)");
                continue;
            }
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

            // ▼▼▼ [수정] CSV 컬럼 순서에 맞게 인덱스 매핑 변경 ▼▼▼
            // CSV 순서: ID, Name, Rarity, Description, 획득 경로, Img_Res, Buy_Price
            string id = values[0].Trim();
            string name = values[1].Trim();
            string rarityStr = values[2].Trim();
            string description = values[3].Trim();   // [3] 설명
            // string acquisitionPath = values[4].Trim(); // [4] 획득 경로는 데이터 클래스에 없으므로 건너뜀
            string iconPath = values[5].Trim();      // [5] 이미지 경로
            string priceStr = values[6].Trim();      // [6] 가격
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

            Rarity rarity;
            if (!System.Enum.TryParse(rarityStr, true, out rarity))
            {
                Debug.LogWarning($"Invalid rarity '{rarityStr}' at line {i + 1}. Defaulting to Common.");
                rarity = Rarity.common;
            }

            int buyPrice = 0;
            // 이제 priceStr에 올바른 가격 데이터가 들어가므로 오류가 발생하지 않습니다.
            if (!int.TryParse(priceStr, out buyPrice))
            {
                Debug.LogWarning($"Invalid price '{priceStr}' at line {i + 1}. Defaulting to 0.");
            }

            // ScriptableObject 생성 또는 로드
            string assetPath = $"{savePath}{name}.asset";
            IngredientData data = AssetDatabase.LoadAssetAtPath<IngredientData>(assetPath);

            if (data == null)
            {
                data = ScriptableObject.CreateInstance<IngredientData>();
                AssetDatabase.CreateAsset(data, assetPath);
            }

            // 데이터 할당
            data.id = id;
            data.ingredientName = name;
            data.rarity = rarity;
            data.buyPrice = buyPrice;
            data.description = description;

            // 아이콘 로드 (이미지 경로가 '-'인 경우 건너뜀)
            if (iconPath != "-" && !string.IsNullOrEmpty(iconPath))
            {
                Sprite icon = Resources.Load<Sprite>(iconPath);
                if (icon != null)
                {
                    data.icon = icon;
                }
                else
                {
                    // Resources 폴더 구조에 따라 경로 수정이 필요할 수 있음 (예: "Ingredients/" + iconPath)
                    Debug.LogWarning($"Icon not found at path 'Resources/{iconPath}' for ingredient '{name}'");
                }
            }

            EditorUtility.SetDirty(data);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Ingredient Data Import Completed!");
    }
}