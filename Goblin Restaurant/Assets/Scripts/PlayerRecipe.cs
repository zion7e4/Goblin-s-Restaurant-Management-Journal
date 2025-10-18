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

    public int GetCurrentPrice()
    {
        float priceIncreaseRate = (currentLevel - 1) * 0.2f;
        return data.basePrice + (int)(data.basePrice * priceIncreaseRate);
    }

    public int GetCurrentGrade()
    {
        if (currentLevel >= 9) return 5;
        if (currentLevel >= 7) return 4;
        if (currentLevel >= 5) return 3;
        if (currentLevel >= 3) return 2;
        return 1;
    }
}