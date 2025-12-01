using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 고용된 직원의 개별 정보(이름, 능력치, 특성 등)를 저장하고 관리하는 데이터 클래스입니다.
/// 이 클래스의 인스턴스는 게임 내의 한 명의 직원을 나타냅니다.
/// </summary>
[System.Serializable]
public class EmployeeInstance
{
    // --- 기본 정보 ---

    /// <summary>
    /// 이 직원의 기반이 되는 종족 데이터(ScriptableObject)입니다. 변하지 않는 기본 정보를 담고 있습니다.
    /// </summary>
    public EmployeeData BaseData { get; private set; }

    /// <summary>
    /// 이 직원이 주인공인지 여부입니다. (해고 불가능)
    /// </summary>
    public bool isProtagonist { get; set; }

    /// <summary>
    /// 게임 내 표시될 직원의 이름입니다.
    /// </summary>
    public string firstName;

    /// <summary>
    /// 직원의 현재 레벨입니다.
    /// </summary>
    public int currentLevel;

    /// <summary>
    /// 다음 레벨까지의 현재 경험치입니다.
    /// </summary>
    public float currentExperience;

    /// <summary>
    /// 현재 보유한 스킬 포인트입니다.
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
    /// [Tooltip("직원 관리창에서 할당된 역할 (주방, 홀, 미지정)")]
    /// </summary>
    public EmployeeRole assignedRole = EmployeeRole.Unassigned;

    /// <summary>
    /// 직원의 등급입니다. (S, A, B, C)
    /// </summary>
    public EmployeeGrade grade;

    // --- 생성자 ---

    /// <summary>
    /// '지원자(GeneratedApplicant)' 데이터를 바탕으로 새로운 직원 인스턴스를 생성합니다. (일반 고용)
    /// </summary>
    public EmployeeInstance(GeneratedApplicant applicant)
    {
        BaseData = applicant.BaseSpeciesData;
        firstName = applicant.GeneratedFirstName;
        currentLevel = 1;
        currentExperience = 0;
        skillPoints = 5;
        currentSalary = applicant.BaseSpeciesData.salary;
        currentTraits = new List<Trait>(applicant.GeneratedTraits);
        currentCookingStat = applicant.GeneratedCookingStat;
        currentServingStat = applicant.GeneratedServingStat;
        currentCharmStat = applicant.GeneratedCharmStat;

        // 등급(Grade)을 설정합니다.
        this.grade = applicant.grade;

        // 일반 직원은 false로 설정
        isProtagonist = false;
    }

    /// <summary>
    /// '주인공'처럼 종족 템플릿(EmployeeData)에서 직접 직원 인스턴스를 생성합니다.
    /// </summary>
    public EmployeeInstance(EmployeeData baseData)
    {
        BaseData = baseData;
        // 주인공 식별을 위한 기본 이름 설정
        firstName = "Goblin Chef";
        currentLevel = 1;
        currentExperience = 0;
        skillPoints = 5; // 주인공은 기본 스킬 포인트 보유 (보너스)
        currentSalary = baseData.salary;
        currentCookingStat = baseData.baseCookingStat;
        currentServingStat = baseData.baseServingStat;
        currentCharmStat = baseData.baseCharmStat;

        // baseData(mainCharacterTemplate)에 정의된 특성('주인공')을
        // 이 직원의 보유 특성(currentTraits) 리스트로 복사합니다.
        currentTraits = new List<Trait>(baseData.possibleTraits);

        // (주인공의 기본 등급은 C등급으로 설정. 필요시 변경)
        this.grade = EmployeeGrade.C;

        // 주인공 여부를 설정합니다.
        isProtagonist = true;
    }

    // --- 스탯 조작 함수 ---

    // ************* [능력치 관련 함수] *************

    /// <summary>
    /// 요리 스탯에 스킬 포인트를 사용하여 능력을 향상시킵니다.
    /// </summary>
    /// <returns>스탯 상승에 성공하면 true를 반환합니다.</returns>
    public bool SpendSkillPointOnCooking()
    {
        if (skillPoints > 0)
        {
            skillPoints--;
            currentCookingStat++;
            Debug.Log($"{firstName}: 요리 스탯이 {currentCookingStat}로 상승했습니다. 남은 포인트: {skillPoints}");
            return true;
        }
        Debug.LogWarning($"{firstName}: 스킬 포인트가 부족하여 요리 스탯을 올릴 수 없습니다.");
        return false;
    }

    /// <summary>
    /// 서빙 스탯에 스킬 포인트를 사용하여 능력을 향상시킵니다.
    /// </summary>
    public bool SpendSkillPointOnServing()
    {
        if (skillPoints > 0)
        {
            skillPoints--;
            currentServingStat++;
            Debug.Log($"{firstName}: 서빙 스탯이 {currentServingStat}로 상승했습니다. 남은 포인트: {skillPoints}");
            return true;
        }
        Debug.LogWarning($"{firstName}: 스킬 포인트가 부족하여 서빙 스탯을 올릴 수 없습니다.");
        return false;
    }

    /// <summary>
    /// 매력 스탯에 스킬 포인트를 사용하여 능력을 향상시킵니다.
    /// </summary>
    public bool SpendSkillPointOnCharm()
    {
        if (skillPoints > 0)
        {
            skillPoints--;
            currentCharmStat++;
            Debug.Log($"{firstName}: 매력 스탯이 {currentCharmStat}로 상승했습니다. 남은 포인트: {skillPoints}");
            return true;
        }
        Debug.LogWarning($"{firstName}: 스킬 포인트가 부족하여 매력 스탯을 올릴 수 없습니다.");
        return false;
    }

    /// <summary>
    /// 이 직원이 보유한 모든 특성에서 '재료 절약' 확률의 합을 반환합니다.
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
    /// (기획서 기반) 레벨을 1단계 상승시킵니다.
    /// 비용 소모, 최대 레벨 체크, SP 1 획득이 이루어집니다.
    /// </summary>
    /// <returns>레벨업 성공 여부</returns>
    public bool TryLevelUp()
    {
        // 1. 등급(Grade)에 따른 최대 레벨을 결정합니다. (기획서 참고)
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

        // 2. 비용 소모 확인 (기획서 기반)
        // (기획서 내용: 다음 레벨 비용 = 현재 레벨 비용 * 1.1)
        int requiredGold = (int)(100 * Mathf.Pow(1.1f, currentLevel - 1)); // (기획서 10% 증가 공식 임시 적용)

        if (GameManager.instance.totalGoldAmount < requiredGold)
        {
            Debug.LogWarning($"{firstName} 레벨업 실패: 골드가 부족합니다. (필요: {requiredGold}G)");
            return false;
        }

        // 3. 비용 소모 및 레벨업 처리
        GameManager.instance.SpendGold(requiredGold);
        currentLevel++;
        skillPoints++; // ★ 기획서대로 SP 1 지급 ★

        Debug.Log($"[레벨업!] {firstName} (Lv. {currentLevel}), SP +1. (소모: {requiredGold}G)");
        return true;
    }

    /// <summary>
    /// 이 직원이 보유한 모든 특성에서 '재료 훔침' 확률의 합을 반환합니다.
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
    /// 이 직원이 보유한 모든 특성에서 '요리 능력치 보정' 배수의 합을 반환합니다.
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
    /// 이 직원이 보유한 모든 특성에서 '이동 속도 보정' 배수의 합을 반환합니다.
    /// </summary>
    public float GetTraitMoveSpeedMultiplier()
    {
        if (currentTraits == null || currentTraits.Count == 0)
        {
            return 0f;
        }
        // '느림보'(-0.1) 같은 특성 값의 합산
        return currentTraits.Sum(trait => trait.moveSpeedMultiplier);
    }

    /// <summary>
    /// 이 직원이 보유한 모든 특성에서 '작업 속도 보정' 배수의 합을 반환합니다.
    /// </summary>
    public float GetTraitWorkSpeedMultiplier()
    {
        if (currentTraits == null || currentTraits.Count == 0)
        {
            return 0f;
        }
        // 특성 값의 합산
        return currentTraits.Sum(trait => trait.workSpeedMultiplier);
    }

    /// <summary>
    /// 이 직원이 보유한 모든 특성에서 '서비스 점수' 보너스/페널티 합을 반환합니다.
    /// </summary>
    public int GetTraitServiceScoreBonus()
    {
        if (currentTraits == null || currentTraits.Count == 0)
        {
            return 0;
        }

        // '불친절함'(-5)이나 '친절함'(+?) 같은 특성 값의 합산
        return currentTraits.Sum(trait => trait.serviceScoreModifier);
    }

    /// <summary>
    /// 이 직원이 보유한 모든 특성에서 '팁 확률' 보너스 합을 반환합니다.
    /// </summary>
    public float GetTraitTipChanceBonus()
    {
        if (currentTraits == null || currentTraits.Count == 0)
        {
            return 0f;
        }
        // '매혹' 특성 등의 값 합산
        return currentTraits.Sum(trait => trait.tipChanceBonus);
    }

    /// <summary>
    /// 이 직원이 보유한 모든 특성에서 '모든 능력치 보정' 배수의 합을 반환합니다.
    /// </summary>
    public float GetTraitAllStatMultiplier()
    {
        if (currentTraits == null || currentTraits.Count == 0)
        {
            return 0f;
        }
        // '주인공' 특성 등의 값 합산
        return currentTraits.Sum(trait => trait.allStatMultiplier);
    }
}
