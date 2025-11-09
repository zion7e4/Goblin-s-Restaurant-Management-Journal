using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 고용된 개별 직원의 현재 상태(레벨, 능력치, 특성 등)를 저장하고 관리하는 데이터 클래스입니다.
/// 이 클래스의 인스턴스가 실제 저장 및 로드의 대상이 됩니다.
/// </summary>
[System.Serializable]
public class EmployeeInstance
{
    // --- 멤버 변수 ---

    /// <summary>
    /// 이 직원의 원본이 되는 종족 데이터(ScriptableObject)입니다. 변하지 않는 기본 정보를 담고 있습니다.
    /// </summary>
    public EmployeeData BaseData { get; private set; }

    /// <summary>
    /// 이 직원이 주인공인지 여부입니다. (해고 방지용)
    /// </summary>
    public bool isProtagonist { get; private set; }

    /// <summary>
    /// 생성 시 부여된 직원의 이름입니다.
    /// </summary>
    public string firstName;

    /// <summary>
    /// 직원의 현재 레벨입니다.
    /// </summary>
    public int currentLevel;

    /// <summary>
    /// 현재 레벨에서 쌓인 경험치입니다.
    /// </summary>
    public float currentExperience;

    /// <summary>
    /// 투자 가능한 스킬 포인트입니다.
    /// </summary>
    public int skillPoints;

    /// <summary>
    /// 직원의 현재 급여입니다.
    /// </summary>
    public int currentSalary;

    /// <summary>
    /// 직원의 현재 요리 능력치입니다.
    /// </summary>
    public int currentCookingStat;

    /// <summary>
    /// 직원의 현재 서빙 능력치입니다.
    /// </summary>
    public int currentServingStat;

    /// <summary>
    /// 직원의 현재 매력 능력치입니다.
    /// </summary>
    public int currentCharmStat;

    /// <summary>
    /// 직원이 현재 보유한 특성 목록입니다.
    /// </summary>
    public List<Trait> currentTraits;

    /// <summary>
    [Tooltip("직원 관리창에서 할당된 역할 (주방, 홀)")]
    public EmployeeRole assignedRole = EmployeeRole.Unassigned;

    public EmployeeGrade grade;

    // --- 생성자 ---

    /// <summary>
    /// '지원자(GeneratedApplicant)' 데이터를 바탕으로 새로운 직원 인스턴스를 생성합니다. (일반 직원)
    /// </summary>
    public EmployeeInstance(GeneratedApplicant applicant)
    {
        BaseData = applicant.BaseSpeciesData;
        firstName = applicant.GeneratedFirstName;
        currentLevel = 1;
        currentExperience = 0;
        skillPoints = 0;
        currentSalary = applicant.BaseSpeciesData.salary;
        currentTraits = new List<Trait>(applicant.GeneratedTraits);
        currentCookingStat = applicant.GeneratedCookingStat;
        currentServingStat = applicant.GeneratedServingStat;
        currentCharmStat = applicant.GeneratedCharmStat;

        // 지원자의 등급을 복사합니다.
        this.grade = applicant.grade;

        // 일반 직원은 false로 설정
        isProtagonist = false;
    }

    /// <summary>
    /// '주인공'처럼 정해진 템플릿(EmployeeData)에서 직접 직원 인스턴스를 생성합니다.
    /// </summary>
    public EmployeeInstance(EmployeeData baseData)
    {
        BaseData = baseData;
        // 고블린 쉐프 식별을 위한 기본 이름 설정
        firstName = "Goblin Chef";
        currentLevel = 1;
        currentExperience = 0;
        skillPoints = 5; // 주인공은 기본 스킬 포인트 제공 (예시)
        currentSalary = baseData.salary;
        currentCookingStat = baseData.baseCookingStat;
        currentServingStat = baseData.baseServingStat;
        currentCharmStat = baseData.baseCharmStat;

        // baseData(mainCharacterTemplate)에 연결된 특성("주인공")을
        // 이 직원의 현재 특성(currentTraits) 리스트에 복사합니다.
        currentTraits = new List<Trait>(baseData.possibleTraits);

        // (주인공의 기본 등급을 C등급으로 설정. 필요시 수정)
        this.grade = EmployeeGrade.C;

        // 주인공 여부를 설정합니다.
        isProtagonist = true;
    }

    // --- 핵심 기능 함수 ---

    // ************* [경험치 관련 함수는 현재 구현되지 않았으므로 생략] *************

    /// <summary>
    /// 요리 스탯에 스킬 포인트를 사용하고 스탯을 증가시킵니다.
    /// </summary>
    /// <returns>스탯 증가에 성공했으면 true를 반환합니다.</returns>
    public bool SpendSkillPointOnCooking()
    {
        if (skillPoints > 0)
        {
            skillPoints--;
            currentCookingStat++;
            Debug.Log($"{firstName}: 요리 스탯이 {currentCookingStat}으로 증가했습니다. 남은 포인트: {skillPoints}");
            return true;
        }
        Debug.LogWarning($"{firstName}: 스킬 포인트가 부족하여 요리 스탯을 올릴 수 없습니다.");
        return false;
    }

    /// <summary>
    /// 서빙 스탯에 스킬 포인트를 사용하고 스탯을 증가시킵니다.
    /// </summary>
    public bool SpendSkillPointOnServing()
    {
        if (skillPoints > 0)
        {
            skillPoints--;
            currentServingStat++;
            Debug.Log($"{firstName}: 서빙 스탯이 {currentServingStat}으로 증가했습니다. 남은 포인트: {skillPoints}");
            return true;
        }
        Debug.LogWarning($"{firstName}: 스킬 포인트가 부족하여 서빙 스탯을 올릴 수 없습니다.");
        return false;
    }

    /// <summary>
    /// 매력 스탯에 스킬 포인트를 사용하고 스탯을 증가시킵니다.
    /// </summary>
    public bool SpendSkillPointOnCharm()
    {
        if (skillPoints > 0)
        {
            skillPoints--;
            currentCharmStat++;
            Debug.Log($"{firstName}: 매력 스탯이 {currentCharmStat}으로 증가했습니다. 남은 포인트: {skillPoints}");
            return true;
        }
        Debug.LogWarning($"{firstName}: 스킬 포인트가 부족하여 매력 스탯을 올릴 수 없습니다.");
        return false;
    }
    /// <summary>
    /// 이 직원이 가진 모든 특성에서 '식재료 절약' 보너스 총합을 반환합니다.
    /// </summary>
    public float GetTraitSaveChance()
    {
        if (currentTraits == null || currentTraits.Count == 0)
        {
            return 0f;
        }

        // 모든 특성의 'ingredientSaveChance' 값을 합산
        return currentTraits.Sum(trait => trait.ingredientSaveChance);
    }


    /// <summary>
    /// (기획서 기준) 직원을 1레벨업 시킵니다.
    /// 골드 소모, 최대 레벨 체크, SP 1 획득이 이루어집니다.
    /// </summary>
    /// <returns>레벨업 성공 여부</returns>
    public bool TryLevelUp()
    {
        // 1. 등급(Grade)에 따라 최대 레벨을 결정합니다. (기획서 기준)
        int maxLevel;
        switch (this.grade)
        {
            case EmployeeGrade.C:
                maxLevel = 20;
                break;
            case EmployeeGrade.B:
                maxLevel = 30;
                break;
            case EmployeeGrade.A:
                maxLevel = 40;
                break;
            case EmployeeGrade.S:
                maxLevel = 50;
                break;
            default:
                maxLevel = 20; // 기본값 C등급
                break;
        }

        if (currentLevel >= maxLevel)
        {
            Debug.LogWarning($"{firstName}({this.grade}등급)은(는) 이미 최대 레벨({maxLevel})입니다.");
            return false;
        }

        // 2. 골드 소모 확인 (기획서 기준)
        // (기획서 예시: 다음 레벨 비용 = 현재 레벨 비용 * 1.1)
        int requiredGold = (int)(100 * Mathf.Pow(1.1f, currentLevel - 1)); // (기획서 10% 증가 공식 임시 적용)

        if (GameManager.instance.totalGoldAmount < requiredGold)
        {
            Debug.LogWarning($"{firstName} 레벨업 실패: 골드가 부족합니다. (필요: {requiredGold}G)");
            return false;
        }

        // 3. 골드 소모 및 레벨업 처리
        GameManager.instance.SpendGold(requiredGold);
        currentLevel++;
        skillPoints++; // ★★★ 기획서대로 SP 1 지급 ★★★

        Debug.Log($"[레벨업!] {firstName} (Lv. {currentLevel}), SP +1. (비용: {requiredGold}G)");
        return true;
    }
    /// <summary>
    /// 이 직원이 가진 모든 특성에서 '식재료 훔칠' 보너스 총합을 반환합니다.
    /// </summary>
    public float GetTraitStealChance()
    {
        if (currentTraits == null || currentTraits.Count == 0)
        {
            return 0f;
        }

        // 모든 특성의 'ingredientStealChance' 값을 합산
        return currentTraits.Sum(trait => trait.ingredientStealChance);
    }
    /// <summary>
    /// 이 직원이 가진 모든 특성에서 '요리 스탯 배율' 보너스 총합을 반환합니다.
    /// </summary>
    public float GetTraitCookingStatMultiplier()
    {
        if (currentTraits == null || currentTraits.Count == 0)
        {
            return 0f;
        }

        // 모든 특성의 'cookingStatMultiplier' 값을 합산
        return currentTraits.Sum(trait => trait.cookingStatMultiplier);
    }
    /// <summary>
    /// 이 직원이 가진 모든 특성에서 '이동 속도 배율' 보너스 총합을 반환합니다.
    /// </summary>
    public float GetTraitMoveSpeedMultiplier()
    {
        if (currentTraits == null || currentTraits.Count == 0)
        {
            return 0f;
        }
        // "게으름"(-0.1) 같은 특성의 값을 합산
        return currentTraits.Sum(trait => trait.moveSpeedMultiplier);
    }

    /// <summary>
    /// 이 직원이 가진 모든 특성에서 '작업 속도 배율' 보너스 총합을 반환합니다.
    /// </summary>
    public float GetTraitWorkSpeedMultiplier()
    {
        if (currentTraits == null || currentTraits.Count == 0)
        {
            return 0f;
        }
        // "게으름"(-0.1) 같은 특성의 값을 합산
        return currentTraits.Sum(trait => trait.workSpeedMultiplier);
    }
    /// <summary>
    /// 이 직원이 가진 모든 특성에서 '서비스 점수' 보너스/페널티 총합을 반환합니다.
    /// </summary>
    public int GetTraitServiceScoreBonus()
    {
        if (currentTraits == null || currentTraits.Count == 0)
        {
            return 0;
        }

        // "실수투성이"(-5)나 "긍정적"(+?) 같은 특성의 값을 합산
        return currentTraits.Sum(trait => trait.serviceScoreModifier);
    }
    /// <summary>
    /// 이 직원이 가진 모든 특성에서 '팁 확률' 보너스 총합을 반환합니다.
    /// </summary>
    public float GetTraitTipChanceBonus()
    {
        if (currentTraits == null || currentTraits.Count == 0)
        {
            return 0f;
        }
        // "매혹" 특성의 값을 합산
        return currentTraits.Sum(trait => trait.tipChanceBonus);
    }

    /// <summary>
    /// 이 직원이 가진 모든 특성에서 '모든 스탯 배율' 보너스 총합을 반환합니다.
    /// </summary>
    public float GetTraitAllStatMultiplier()
    {
        if (currentTraits == null || currentTraits.Count == 0)
        {
            return 0f;
        }
        // "주인공" 특성의 값을 합산
        return currentTraits.Sum(trait => trait.allStatMultiplier);
    }
}