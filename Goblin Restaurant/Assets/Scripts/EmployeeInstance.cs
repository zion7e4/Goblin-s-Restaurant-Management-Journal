using System.Collections.Generic;
using UnityEngine;

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
    /// '지원자(GeneratedApplicant)' 데이터를 바탕으로 새로운 직원 인스턴스를 생성합니다.
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
    }

    /// <summary>
    /// '주인공'처럼 정해진 템플릿(EmployeeData)에서 직접 직원 인스턴스를 생성합니다.
    /// </summary>
    public EmployeeInstance(EmployeeData baseData)
    {
        BaseData = baseData;
        firstName = baseData.speciesName;
        currentLevel = 1;
        currentExperience = 0;
        skillPoints = 0;
        currentSalary = baseData.salary;
        currentCookingStat = baseData.baseCookingStat;
        currentServingStat = baseData.baseServingStat;
        currentCleaningStat = baseData.baseCleaningStat;
        currentTraits = new List<Trait>(baseData.possibleTraits);
    }

    // --- 핵심 기능 함수 ---

    /// <summary>
    /// 경험치를 추가하고, 필요 경험치를 충족하면 레벨업을 진행하여 스킬 포인트를 얻습니다.
    /// </summary>
    /// <param name="amount">추가할 경험치 양</param>
    public void AddExperience(float amount)
    {
        float requiredExp = currentLevel * 100;
        currentExperience += amount;

        while (currentExperience >= requiredExp)
        {
            currentLevel++;
            currentExperience -= requiredExp;
            skillPoints++;
            currentSalary += 10;
            Debug.Log($"축하합니다! {firstName}(이)가 {currentLevel}레벨로 상승했습니다! (보유 스킬포인트: {skillPoints})");
            requiredExp = currentLevel * 100;
        }
    }

    /// <summary>
    /// 스킬 포인트를 1 사용하여 '요리' 능력치를 1 올립니다.
    /// </summary>
    /// <returns>성공하면 true, 스킬 포인트가 부족하면 false를 반환합니다.</returns>
    public bool SpendSkillPointOnCooking()
    {
        if (skillPoints > 0)
        {
            skillPoints--;
            currentCookingStat++;
            Debug.Log($"{firstName}의 요리 능력치가 {currentCookingStat}(으)로 상승했습니다!");
            return true;
        }
        return false;
    }

    /// <summary>
    /// 스킬 포인트를 1 사용하여 '서빙' 능력치를 1 올립니다.
    /// </summary>
    /// <returns>성공하면 true, 스킬 포인트가 부족하면 false를 반환합니다.</returns>
    public bool SpendSkillPointOnServing()
    {
        if (skillPoints > 0)
        {
            skillPoints--;
            currentServingStat++;
            Debug.Log($"{firstName}의 서빙 능력치가 {currentServingStat}(으)로 상승했습니다!");
            return true;
        }
        return false;
    }

    /// <summary>
    /// 스킬 포인트를 1 사용하여 '정리' 능력치를 1 올립니다.
    /// </summary>
    /// <returns>성공하면 true, 스킬 포인트가 부족하면 false를 반환합니다.</returns>
    public bool SpendSkillPointOnCleaning()
    {
        if (skillPoints > 0)
        {
            skillPoints--;
            currentCleaningStat++;
            Debug.Log($"{firstName}의 정리 능력치가 {currentCleaningStat}(으)로 상승했습니다!");
            return true;
        }
        return false;
    }
}