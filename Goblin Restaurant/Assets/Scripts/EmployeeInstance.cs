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
    /// 직원의 현재 정리 능력치입니다.
    /// </summary>
    public int currentCleaningStat;

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
        currentCleaningStat = applicant.GeneratedCleaningStat;

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
        currentCleaningStat = baseData.baseCleaningStat;
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
    /// 정리 스탯에 스킬 포인트를 사용하고 스탯을 증가시킵니다.
    /// </summary>
    public bool SpendSkillPointOnCleaning()
    {
        if (skillPoints > 0)
        {
            skillPoints--;
            currentCleaningStat++;
            Debug.Log($"{firstName}: 정리 스탯이 {currentCleaningStat}으로 증가했습니다. 남은 포인트: {skillPoints}");
            return true;
        }
        Debug.LogWarning($"{firstName}: 스킬 포인트가 부족하여 정리 스탯을 올릴 수 없습니다.");
        return false;
    }
}