using UnityEngine;


[CreateAssetMenu(fileName = "Ingredient", menuName = "Game Data/Ingredient Data")]
public class IngredientData : ScriptableObject
{
    [Header("재료 고유 정보")]
    public string id; // 재료를 구분하는 고유 ID 
    public string ingredientName; // 게임에 표시될 이름
    public Sprite icon; // UI에 표시될 아이콘

    [Header("재료 등급 및 가격")]
    public Rarity rarity; // 재료의 희귀도 (일반, 고급, 희귀, 전설)
    public int buyPrice; // 상점에서 구매할 때의 가격
}
