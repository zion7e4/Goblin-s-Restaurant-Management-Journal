using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class EmployeeInstance
{
    public EmployeeData BaseData { get; private set; }

    // isProtagonist's setter is now public to allow modification during loading
    public bool isProtagonist { get; set; }

    public string firstName;
    public int currentLevel;
    public float currentExperience;
    public int skillPoints;
    public int currentSalary;
    public int currentCookingStat;
    public int currentServingStat;
    public int currentCharmStat;
    public List<Trait> currentTraits;
    public EmployeeRole assignedRole = EmployeeRole.Unassigned;
    public EmployeeGrade grade;

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
        this.grade = applicant.grade;
        isProtagonist = false;
    }

    public EmployeeInstance(EmployeeData baseData)
    {
        BaseData = baseData;
        firstName = "Goblin Chef";
        currentLevel = 1;
        currentExperience = 0;
        skillPoints = 5;
        currentSalary = baseData.salary;
        currentCookingStat = baseData.baseCookingStat;
        currentServingStat = baseData.baseServingStat;
        currentCharmStat = baseData.baseCharmStat;
        currentTraits = new List<Trait>(baseData.possibleTraits);
        this.grade = EmployeeGrade.C;
        isProtagonist = true;
    }

    public bool SpendSkillPointOnCooking()
    {
        if (skillPoints > 0)
        {
            skillPoints--;
            currentCookingStat++;
            Debug.Log($"{firstName}: 요리 스탯이 {currentCookingStat}으로 상승했습니다. 남은 포인트: {skillPoints}");
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
            Debug.Log($"{firstName}: 서빙 스탯이 {currentServingStat}으로 상승했습니다. 남은 포인트: {skillPoints}");
            return true;
        }
        return false;
    }

    public bool SpendSkillPointOnCharm()
    {
        if (skillPoints > 0)
        {
            skillPoints--;
            currentCharmStat++;
            Debug.Log($"{firstName}: 매력 스탯이 {currentCharmStat}으로 상승했습니다. 남은 포인트: {skillPoints}");
            return true;
        }
        return false;
    }

    public float GetTraitSaveChance()
    {
        if (currentTraits == null || currentTraits.Count == 0) return 0f;
        return currentTraits.Sum(trait => trait.ingredientSaveChance);
    }

    public bool TryLevelUp()
    {
        int maxLevel;
        switch (this.grade)
        {
            case EmployeeGrade.C: maxLevel = 20; break;
            case EmployeeGrade.B: maxLevel = 30; break;
            case EmployeeGrade.A: maxLevel = 40; break;
            case EmployeeGrade.S: maxLevel = 50; break;
            default: maxLevel = 20; break;
        }

        if (currentLevel >= maxLevel)
        {
            Debug.LogWarning($"{firstName}({this.grade}등급)은(는) 이미 최대 레벨({maxLevel})입니다.");
            return false;
        }

        int requiredGold = (int)(100 * Mathf.Pow(1.1f, currentLevel - 1));
        if (GameManager.instance.totalGoldAmount < requiredGold)
        {
            Debug.LogWarning($"{firstName} 레벨업 실패: 골드가 부족합니다. (필요: {requiredGold}G)");
            return false;
        }

        GameManager.instance.SpendGold(requiredGold);
        currentLevel++;
        skillPoints++;
        Debug.Log($"[레벨업!] {firstName} (Lv. {currentLevel}), SP +1. (비용: {requiredGold}G)");
        return true;
    }

    public float GetTraitStealChance()
    {
        if (currentTraits == null || currentTraits.Count == 0) return 0f;
        return currentTraits.Sum(trait => trait.ingredientStealChance);
    }

    public float GetTraitCookingStatMultiplier()
    {
        if (currentTraits == null || currentTraits.Count == 0) return 0f;
        return currentTraits.Sum(trait => trait.cookingStatMultiplier);
    }

    public float GetTraitMoveSpeedMultiplier()
    {
        if (currentTraits == null || currentTraits.Count == 0) return 0f;
        return currentTraits.Sum(trait => trait.moveSpeedMultiplier);
    }

    public float GetTraitWorkSpeedMultiplier()
    {
        if (currentTraits == null || currentTraits.Count == 0) return 0f;
        return currentTraits.Sum(trait => trait.workSpeedMultiplier);
    }

    public int GetTraitServiceScoreBonus()
    {
        if (currentTraits == null || currentTraits.Count == 0) return 0;
        return currentTraits.Sum(trait => trait.serviceScoreModifier);
    }

    public float GetTraitTipChanceBonus()
    {
        if (currentTraits == null || currentTraits.Count == 0) return 0f;
        return currentTraits.Sum(trait => trait.tipChanceBonus);
    }

    public float GetTraitAllStatMultiplier()
    {
        if (currentTraits == null || currentTraits.Count == 0) return 0f;
        return currentTraits.Sum(trait => trait.allStatMultiplier);
    }
}