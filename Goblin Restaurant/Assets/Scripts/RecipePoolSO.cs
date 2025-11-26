using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RecipePool", menuName = "GoblinRestaurant/Shop/Recipe Pool")]
public class RecipePoolSO : ScriptableObject
{
    public List<RecipePoolEntry> items = new List<RecipePoolEntry>();
}