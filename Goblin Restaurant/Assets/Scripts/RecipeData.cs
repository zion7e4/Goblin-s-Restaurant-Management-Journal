using UnityEngine;
using System.Collections.Generic;
using System.Linq; // LINQ를 사용하여 희귀도를 쉽게 계산합니다.

// 레시피의 등급을 정의하는 열거형(Enum)입니다.
public enum RecipeGrade
{
    Ordinary,    // 평범한 (1성)
    Skilled,     // 숙련된 (2성)
    FirstClass,  // 일류 (3성)
    Artisan,     // 장인의 (4성)
    Master       // 대가의 (5성)
}

// 레시피를 만들기 위해 필요한 재료와 그 수량을 묶어서 관리하는 클래스입니다.
[System.Serializable]
public class IngredientRequirement
{
    [Tooltip("필요한 재료의 원본 데이터")]
    public IngredientData ingredient;
    [Tooltip("필요한 재료의 수량")]
    public int amount;
}

[CreateAssetMenu(fileName = "New Recipe", menuName = "GoblinChef/Recipe Data")]
public class RecipeData : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("레시피의 이름 (예: 고블린 오믈렛)")]
    public string recipeName;
    [Tooltip("레시피 아이콘 이미지")]
    public Sprite icon;
    [TextArea(3, 5)]
    [Tooltip("레시피에 대한 설명")]
    public string description;

    [Header("핵심 능력치")]
    [Tooltip("1레벨 기준 기본 판매 가격")]
    public int basePrice;
    [Tooltip("1레벨 기준 기본 요리 시간(초)")]
    public float baseCookTime;

    [Header("필요 재료")]
    [Tooltip("이 레시피를 만드는 데 필요한 재료 목록")]
    public List<IngredientRequirement> requirements;

    // --- 내부 계산 프로퍼티 ---

    /// <summary>
    /// 필요 재료 중 가장 높은 희귀도를 자동으로 계산하여 반환합니다.
    /// </summary>
    public IngredientRarity Rarity
    {
        get
        {
            if (requirements == null || !requirements.Any())
            {
                return IngredientRarity.Common;
            }
            // 모든 재료의 희귀도 중 가장 높은 값을 찾아 반환합니다.
            return requirements.Max(req => req.ingredient.rarity);
        }
    }

    /// <summary>
    /// 현재 레시피의 레벨에 따라 등급(별점)을 자동으로 계산하여 반환합니다.
    /// </summary>
    public RecipeGrade GetGrade(int currentLevel)
    {
        if (currentLevel >= 9) return RecipeGrade.Master;
        if (currentLevel >= 7) return RecipeGrade.Artisan;
        if (currentLevel >= 5) return RecipeGrade.FirstClass;
        if (currentLevel >= 3) return RecipeGrade.Skilled;
        return RecipeGrade.Ordinary;
    }
}