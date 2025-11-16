using UnityEngine;

[System.Serializable]
public class ShopPoolEntry
{
    public int id; // CSV의 ID

    [Tooltip("RecipeData의 ID 또는 IngredientData의 ID(string)")]
    public string itemID; // CSV의 ItemID

    [Tooltip("등장에 필요한 최소 명성도 레벨")]
    public int fameLevelMin; // CSV의 FameLevel_Min

    [Tooltip("등장 시 최소 수량")]
    public int qtyMin; // CSV의 Qty_Min (레시피는 0)

    [Tooltip("등장 시 최대 수량")]
    public int qtyMax; // CSV의 Qty_Max (레시피는 0)

    [Tooltip("등장 확률 가중치")]
    public int weight; // CSV의 Weight
}