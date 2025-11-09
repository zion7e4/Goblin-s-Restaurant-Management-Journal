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
        currentTraits = new List<Trait>(); // 주인공은 기본 특성 없음으로 시작 (수정 가능)

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
            currentCharmStat++; // [수정]
            Debug.Log($"{firstName}: 매력 스탯이 {currentCharmStat}으로 증가했습니다. 남은 포인트: {skillPoints}"); // [수정]
            return true;
        }
        Debug.LogWarning($"{firstName}: 스킬 포인트가 부족하여 매력 스탯을 올릴 수 없습니다."); // [수정]
        return false;
    }
    /// <summary>
    /// (기획서 기준) 직원을 1레벨업 시킵니다.
    /// 골드 소모, 최대 레벨 체크, SP 1 획득이 이루어집니다.
    /// </summary>
    /// <returns>레벨업 성공 여부</returns>
    public bool TryLevelUp()
    {
        // 1. 최대 레벨인지 확인 (기획서 기준)
        // (참고: 이 기능은 EmployeeData에 '등급(Grade)' 변수가 있어야 완벽히 작동합니다)
        // int maxLevel = BaseData.GetMaxLevelForGrade(); // (예시)
        int maxLevel = 50; // (임시: 기획서 S등급 최대 레벨 50)

        if (currentLevel >= maxLevel)
        {
            Debug.LogWarning($"{firstName}은(는) 이미 최대 레벨({maxLevel})입니다.");
            return false;
        }

        // 2. 골드 소모 확인 (기획서 기준)
        // (TODO: PlayerRecipe처럼 LevelTable을 만들거나 공식을 적용해야 합니다)
        // (기획서 예시: 다음 레벨 비용 = 현재 레벨 비용 * 1.1)
        int requiredGold = (int)(100 * Mathf.Pow(1.1f, currentLevel - 1)); // (기획서 10% 증가 공식 임시 적용)

        if (GameManager.instance.totalGoldAmount < requiredGold)
        {
            Debug.LogWarning($"{firstName} 레벨업 실패: 골드가 부족합니다. (필요: {requiredGold}G)");
            return false;
        }

        // 3. 골드 소모 및 레벨업 처리
        GameManager.instance.SpendGold(requiredGold); // (GameManager에 SpendGold 함수가 필요합니다)
        currentLevel++;
        skillPoints++; // ★★★ 기획서대로 SP 1 지급 ★★★

        Debug.Log($"[레벨업!] {firstName} (Lv. {currentLevel}), SP +1. (비용: {requiredGold}G)");
        return true;
    }
}
