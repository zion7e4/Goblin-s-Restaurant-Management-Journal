using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class IngredientRequirement
{
    public string ingredientID;
    public int amount;
}



[CreateAssetMenu(fileName = "RecipeData", menuName = "Scriptable Objects/RecipeData")]
public class RecipeData : ScriptableObject
{
    [Header("레시피 고유 정보")]
    public int id;
    public string recipeName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("게임 오브젝트")]
    public GameObject foodPrefab;

    [Header("기본 스탯")]
    public int basePrice;
    public float baseCookTime;
    public int Level;
    public Rarity rarity;
    public List<IngredientRequirement> requiredIngredients;
}

