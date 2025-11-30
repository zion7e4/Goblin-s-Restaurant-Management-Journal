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
    [TextArea] public string description; // 와이어프레임 상세 설명용
    public Sprite icon; // 목록용 작은 아이콘

    [Header("도감 상세용 추가 데이터")]
    public Sprite fullImage; // 와이어프레임 Page 7 우측 상단 큰 이미지

    [Header("게임 오브젝트")]
    public GameObject foodPrefab;

    [Header("기본 스탯")]
    public int basePrice;
    public float baseCookTime;
    public int Level; // 시작 레벨
    public Rarity rarity; // 등급 (별점 표시에 사용)
    public List<IngredientRequirement> requiredIngredients;
}