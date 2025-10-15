using UnityEngine;

// 재료의 희귀도를 정의하는 열거형(Enum)입니다.
public enum IngredientRarity
{
    Common,  // 일반
    Uncommon, // 고급
    Rare,    // 희귀
    Legendary// 전설
}

// ScriptableObject를 상속받아 재료의 '원본 데이터'를 정의하는 클래스입니다.
[CreateAssetMenu(fileName = "New Ingredient", menuName = "GoblinChef/Ingredient Data")]
public class IngredientData : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("재료의 이름 (예: 평범한 밀가루)")]
    public string ingredientName;
    [Tooltip("재료 아이콘 이미지")]
    public Sprite icon;

    [Header("설정")]
    [Tooltip("재료의 희귀도 등급")]
    public IngredientRarity rarity;
    [Tooltip("상점에서 구매할 때의 가격")]
    public int buyPrice;
}
