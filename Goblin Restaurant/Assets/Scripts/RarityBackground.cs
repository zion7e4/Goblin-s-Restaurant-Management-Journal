using System;
using UnityEngine;

[Serializable]
public struct RarityBackground
{
    public Rarity rarity; // IngredientData.cs에 있는 enum
    public Sprite backgroundSprite;
}