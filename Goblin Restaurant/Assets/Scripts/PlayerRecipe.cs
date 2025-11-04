using UnityEngine;

public class PlayerRecipe
{
    public RecipeData data;
    public int currentLevel;

    public PlayerRecipe(RecipeData recipeData)
    {
        data = recipeData;
        currentLevel = 1;
    }

    /// <summary>
    /// RecipeLevelTable의 Price_Growth_Rate를 기반으로 현재 판매가를 계산합니다.
    /// </summary>
    public int GetCurrentPrice()
    {
        // 1. 현재 레벨이 1이면 기본 가격(Base_Price)을 반환
        if (currentLevel == 1)
        {
            return data.basePrice;
        }

        // 2. GameDataManager에서 '현재 레벨'의 성장 데이터를 가져옵니다.
        //    (주의: '다음 레벨'이 아니라 '현재 레벨'의 데이터를 가져옵니다.)
        RecipeLevelEntry levelData = GameDataManager.instance.GetRecipeLevelData(currentLevel);

        if (levelData != null)
        {
            // 3. 테이블에서 Price_Growth_Rate 값을 읽어와서 가격을 계산
            float priceIncreaseRate = levelData.Price_Growth_Rate;
            int finalPrice = data.basePrice + (int)(data.basePrice * priceIncreaseRate);
            return finalPrice;
        }
        else
        {
            Debug.LogWarning($"Level {currentLevel}의 데이터를 RecipeLevelTable에서 찾지 못해 임시 공식을 사용합니다.");
            float priceIncreaseRate = (currentLevel - 1) * 0.2f; // 기존 공식
            return data.basePrice + (int)(data.basePrice * priceIncreaseRate);
        }
    }

    /// <summary>
    /// 현재 레벨을 기준으로 등급(1~5)을 반환합니다.
    /// </summary>
    public int GetCurrentGrade()
    {
 
        if (currentLevel >= 9) return 5;
        if (currentLevel >= 7) return 4;
        if (currentLevel >= 5) return 3;
        if (currentLevel >= 3) return 2;
        return 1;
    }
}