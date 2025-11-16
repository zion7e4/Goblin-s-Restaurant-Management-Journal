// Assets/Scripts/Data/Shop/SpecialIngredientPoolSO.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SpecialIngredientPool", menuName = "GoblinRestaurant/Shop/Special Ingredient Pool")]
public class SpecialIngredientPoolSO : ScriptableObject
{
    // ▼▼▼ [수정] 리스트를 여기서 즉시 초기화합니다. ▼▼▼
    public List<SpecialIngredientPoolEntry> items = new List<SpecialIngredientPoolEntry>();
    // (기존) public List<SpecialIngredientPoolEntry> items;
}