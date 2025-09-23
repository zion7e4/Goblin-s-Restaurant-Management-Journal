using System.Collections.Generic;

[System.Serializable]
public class EmployeeInstance
{
    public EmployeeData BaseData { get; private set; }
    public string firstName;

    public int currentLevel;
    public float currentExperience;
    public int skillPoints;
    public int currentSalary;

    public int currentCookingStat;
    public int currentServingStat;
    public int currentCleaningStat;
    public List<Trait> currentTraits;

    // 생성자: GeneratedApplicant(랜덤 지원자)를 받아 인스턴스를 생성
    public EmployeeInstance(GeneratedApplicant applicant)
    {
        BaseData = applicant.BaseSpeciesData;
        firstName = applicant.GeneratedFirstName;

        currentLevel = 1;
        currentExperience = 0;
        skillPoints = 0; // 처음 고용 시 스킬 포인트는 0입니다.
        currentSalary = applicant.BaseSpeciesData.salary;

        currentTraits = new List<Trait>(applicant.GeneratedTraits);
        currentCookingStat = applicant.GeneratedCookingStat;
        currentServingStat = applicant.GeneratedServingStat;
        currentCleaningStat = applicant.GeneratedCleaningStat;
    }

    // 주인공 생성을 위한 특별 생성자
    public EmployeeInstance(EmployeeData baseData)
    {
        BaseData = baseData;
        firstName = baseData.speciesName;

        currentLevel = 1;
        currentExperience = 0;
        skillPoints = 0; // 주인공도 처음엔 스킬 포인트가 0입니다.
        currentSalary = baseData.salary;

        currentCookingStat = baseData.baseCookingStat;
        currentServingStat = baseData.baseServingStat;
        currentCleaningStat = baseData.baseCleaningStat;
        currentTraits = new List<Trait>(baseData.possibleTraits);
    }

    // --- 레벨업 시 스킬 포인트를 지급하는 로직 ---
    public void AddExperience(float amount)
    {
        float requiredExp = currentLevel * 100;
        currentExperience += amount;

        while (currentExperience >= requiredExp)
        {
            currentLevel++;
            currentExperience -= requiredExp;

            // 능력치를 직접 올리는 대신, 스킬 포인트를 1 지급합니다.
            skillPoints++;

            currentSalary += 10;
            UnityEngine.Debug.Log($"축하합니다! {firstName}(이)가 {currentLevel}레벨로 상승했습니다! (보유 스킬포인트: {skillPoints})");

            requiredExp = currentLevel * 100;
        }
    }

    // --- 스킬 포인트를 사용하여 능력치를 올리는 함수들 ---

    public bool SpendSkillPointOnCooking()
    {
        if (skillPoints > 0)
        {
            skillPoints--;
            currentCookingStat++;
            return true;
        }
        return false;
    }

    public bool SpendSkillPointOnServing()
    {
        if (skillPoints > 0)
        {
            skillPoints--;
            currentServingStat++;
            return true;
        }
        return false;
    }

    public bool SpendSkillPointOnCleaning()
    {
        if (skillPoints > 0)
        {
            skillPoints--;
            currentCleaningStat++;
            return true;
        }
        return false;
    }
}
