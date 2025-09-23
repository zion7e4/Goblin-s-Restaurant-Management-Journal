// 파일 이름: RecipeData.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New RecipeData", menuName = "GoblinChef/Recipe Data")]
public class RecipeData : ScriptableObject
{
    [Header("레시피 정보")]
    public string recipeName; // 레시피 이름
    public string description;  // 설명
    public Sprite foodImage;  // 음식 이미지

    [Header("업그레이드 및 가격")]
    public int level = 1;       // 업그레이드 레벨
    public int basePrice;       // 기본 판매 가격
    public int pricePerLevel;   // 레벨 당 추가 가격

    // 현재 판매 가격을 계산하는 프로퍼티
    public int CurrentPrice => basePrice + (pricePerLevel * (level - 1));

    [Header("필요 재료")]
    public List<Ingredient> requiredIngredients; // 필요한 재료 목록
}

// 재료 정보를 담을 간단한 클래스입니다.
// RecipeData와 같은 파일에 있어도 괜찮습니다.
[System.Serializable]
public class Ingredient
{
    public string ingredientName;
    public int count;
}