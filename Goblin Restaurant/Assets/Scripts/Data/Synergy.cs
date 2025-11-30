using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 시너지 발동을 위한 개별 '종족' 조건 (예: '엘프' 1명)
/// </summary>
[System.Serializable]
public class SynergyCondition
{
    [Tooltip("필요한 종족 (예: ElfData.asset)")]
    public EmployeeData species;

    [Tooltip("해당 종족의 최소 인원 수")]
    public int minCount = 1;

    [Tooltip("특정 위치에 배치되어야 하는지 여부 (예: '주방')")]
    public bool requirePlacement = false;

    [Tooltip("필요한 역할 (EmployeeRole enum)")]
    public EmployeeRole requiredRole;
}

/// <summary>
/// 시너지 발동을 위한 개별 '특성' 조건 (예: '꼼꼼함' 1명)
/// </summary>
[System.Serializable]
public class SynergyTraitCondition
{
    [Tooltip("필요한 특성 (예: Trait_Meticulous.asset)")]
    public Trait requiredTrait;

    [Tooltip("해당 특성을 가진 직원의 최소 인원 수")]
    public int minCount = 1;

    [Tooltip("특정 위치에 배치되어야 하는지 여부 (예: '주방')")]
    public bool requirePlacement = false;

    [Tooltip("필요한 역할 (EmployeeRole enum)")]
    public EmployeeRole requiredRole;
}


/// <summary>
/// 시너지 1개의 조건과 효과를 정의하는 Scriptable Object입니다.
/// (예: '티격태격 라이벌.asset')
/// </summary>
[CreateAssetMenu(fileName = "NewSynergy", menuName = "GoblinRestaurant/Synergy")]
public class Synergy : ScriptableObject
{
    [Tooltip("UI에 표시될 시너지 이름")]
    public string synergyName;

    [TextArea(3, 5)]
    [Tooltip("시너지에 대한 설명")]
    public string description;

    [Tooltip("이 시너지가 긍정적인 효과(버프)인지")]
    public bool isPositiveEffect = true;

    [Header("시너지 발동 조건")]
    [Tooltip("종족 기반 조건 (모두 만족해야 함)")]
    public List<SynergyCondition> speciesConditions;

    [Tooltip("특성 기반 조건 (모두 만족해야 함)")]
    public List<SynergyTraitCondition> traitConditions;

    [Tooltip("발동에 필요한 '부정적 특성'의 총 개수 (예: 5 입력 시 '우울한 작업장')")]
    public int minNegativeTraitCount = 0;


    // --- 시너지 효과 ---
    // (이 부분은 '어떤' 직원에게 적용될지 매니저가 판단합니다)
    [Header("시너지 효과 (적용 대상은 매니저가 결정)")]

    [Tooltip("요리 속도 보너스 (예: 0.05 = 5%)")]
    public float cookingSpeedBonus = 0f;

    [Tooltip("요리 스탯 보너스 (예: 10 = +10 스탯)")]
    public int cookingStatBonus = 0;

    [Tooltip("서빙 스탯 보너스 (예: 5 = +5 스탯)")]
    public int servingStatBonus = 0;

    [Tooltip("매력 스탯 보너스 (예: 5 = +5 스탯)")]
    public int charmStatBonus = 0;

    [Tooltip("명성 획득량 보너스 (예: 0.1 = 10%)")]
    public float fameBonusPercent = 0f;

    [Tooltip("이동 속도 보너스 (예: -0.05 = -5%)")]
    public float moveSpeedMultiplier = 0f;

    [Tooltip("서비스 점수 보너스 (예: 2 또는 -2)")]
    public int serviceScoreBonus = 0;

    [Tooltip("식재료 절약 확률 보너스 (예: 0.05 = 5%)")]
    public float ingredientSaveChance = 0f;
}