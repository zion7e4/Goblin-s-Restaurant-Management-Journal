using System.Collections.Generic;
using System;

[Serializable]
public class SaveData
{
    public int dayCount;
    public int gold;
    public float famePoints;

    [Serializable]
    public struct IngredientSaveData
    {
        public string id;
        public int count;
    }
    public List<IngredientSaveData> inventoryItems = new List<IngredientSaveData>();
    public List<string> discoveredIngredients = new List<string>();

    [Serializable]
    public struct RecipeSaveData
    {
        public int id;
        public int level;
    }
    public List<RecipeSaveData> unlockedRecipes = new List<RecipeSaveData>();

    [Serializable]
    public struct EmployeeSaveData
    {
        public string name;
        public int cookStat;
        public int serveStat;
        public int charmStat;
    }
    public List<EmployeeSaveData> employees = new List<EmployeeSaveData>();
}