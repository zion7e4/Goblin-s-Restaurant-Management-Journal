using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SpecialIngredientPool", menuName = "GoblinRestaurant/Shop/Special Ingredient Pool")]
public class SpecialIngredientPoolSO : ScriptableObject
{
    public List<SpecialIngredientPoolEntry> items = new List<SpecialIngredientPoolEntry>();
}