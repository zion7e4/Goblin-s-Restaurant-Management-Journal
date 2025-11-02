using UnityEngine;

/// <summary>
/// 기획서 1.6.B. '레시피 성장 테이블'의 한 행을 정의합니다. [cite: 173, 174]
/// MonoBehaviour를 상속하지 않으며, [System.Serializable]로 만들어
/// 다른 스크립트의 인스펙터 창에 리스트로 표시될 수 있게 합니다.
/// </summary>
[System.Serializable]
public class RecipeLevelEntry
{

    [Tooltip("규칙이 적용될 레벨 (Key)")]
    public int Level; // [cite: 174]

    [Tooltip("해당 레벨로 업그레이드하는 데 필요한 골드")]
    public int Required_Gold; // [cite: 174]

    [Tooltip("해당 레시피의 기본 재료가 몇 배 필요한지 (배수)")]
    public int Required_Item_Count; // [cite: 174]

    [Tooltip("기본 가격 대비 추가되는 가격의 비율 (예: 0.1 = 10%)")]
    public float Price_Growth_Rate; // [cite: 174]
}