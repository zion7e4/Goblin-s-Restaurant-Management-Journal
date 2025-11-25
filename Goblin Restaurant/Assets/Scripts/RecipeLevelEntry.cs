using UnityEngine;

[System.Serializable]
public class RecipeLevelEntry
{
    [Tooltip("레벨")]
    public int Level;

    [Tooltip("필요 골드")]
    public int Required_Gold;

    [Tooltip("필요 재료 배수")]
    public int Required_Item_Count;

    [Tooltip("가격 상승률")]
    public float Price_Growth_Rate;

    //  강화 성공 확률 (0 ~ 100%)
    [Range(0, 100)]
    public int SuccessRate = 100;
}