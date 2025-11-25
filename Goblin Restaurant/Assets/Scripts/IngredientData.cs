using UnityEngine;

// 등급 (Page 8 등급 표시용)
public enum Rarity { Common, Uncommon, Rare, Legendary }

[CreateAssetMenu(fileName = "Ingredient", menuName = "Game Data/Ingredient Data")]
public class IngredientData : ScriptableObject
{
    [Header("재료 고유 정보")]
    public string id;
    public string ingredientName;
    public Sprite icon;

    [Header("도감 표시용")]
    [TextArea] public string description; // 와이어프레임 Page 8 재료 설명

    [Header("재료 등급 및 가격")]
    public Rarity rarity;
    public int buyPrice;
}