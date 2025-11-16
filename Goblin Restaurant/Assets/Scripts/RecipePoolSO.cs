// Assets/Scripts/Data/Shop/RecipePoolSO.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RecipePool", menuName = "GoblinRestaurant/Shop/Recipe Pool")]
public class RecipePoolSO : ScriptableObject
{
    // ▼▼▼ [수정] 리스트를 여기서 즉시 초기화합니다. ▼▼▼
    public List<RecipePoolEntry> items = new List<RecipePoolEntry>();
    // (기존) public List<RecipePoolEntry> items;
}