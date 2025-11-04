using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 레시피나 강화에 필요한 '재료와 수량'을 짝지어 저장하는 데이터 클래스입니다.
/// (예: 고기 2개)
/// [System.Serializable]은 이 클래스가 인스펙터에 표시될 수 있게 합니다.
/// </summary>
[System.Serializable]
public class IngredientCost
{
    public string ingredientID; // 재료의 고유 ID (IngredientData의 ID)
    public int count;          // 필요한 수량
}