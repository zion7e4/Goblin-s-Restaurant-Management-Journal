using UnityEngine;

public class PlayerRecipe
{
    public RecipeData data;
    public int currentLevel;

    public PlayerRecipe(RecipeData recipeData)
    {
        data = recipeData;
        currentLevel = 1;
        data.Level = currentLevel;
    }

    public int GetCurrentPrice()
    {
        float priceIncreaseRate = (currentLevel - 1) * 0.2f;
        return data.basePrice + (int)(data.basePrice * priceIncreaseRate);
    }

    public int GetCurrentGrade()
    {
        if (currentLevel >= 40) return 1;
        if (currentLevel >= 30) return 2;
        if (currentLevel >= 20) return 3;
        if (currentLevel >= 10) return 4;
        return 5;
    }
}