using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// ����� ���� ������ ���� ����(����, �ɷ�ġ, Ư�� ��)�� �����ϰ� �����ϴ� ������ Ŭ�����Դϴ�.
/// �� Ŭ������ �ν��Ͻ��� ���� ���� �� �ε��� ����� �˴ϴ�.
/// </summary>
[System.Serializable]
public class EmployeeInstance
{
    // --- ��� ���� ---

    /// <summary>
    /// �� ������ ������ �Ǵ� ���� ������(ScriptableObject)�Դϴ�. ������ �ʴ� �⺻ ������ ��� �ֽ��ϴ�.
    /// </summary>
    public EmployeeData BaseData { get; private set; }

    /// <summary>
    /// �� ������ ���ΰ����� �����Դϴ�. (�ذ� ������)
    /// </summary>
    public bool isProtagonist { get; private set; }

    /// <summary>
    /// ���� �� �ο��� ������ �̸��Դϴ�.
    /// </summary>
    public string firstName;

    /// <summary>
    /// ������ ���� �����Դϴ�.
    /// </summary>
    public int currentLevel;

    /// <summary>
    /// ���� �������� ���� ����ġ�Դϴ�.
    /// </summary>
    public float currentExperience;

    /// <summary>
    /// ���� ������ ��ų ����Ʈ�Դϴ�.
    /// </summary>
    public int skillPoints;

    /// <summary>
    /// ������ ���� �޿��Դϴ�.
    /// </summary>
    public int currentSalary;

    /// <summary>
    /// ������ ���� �丮 �ɷ�ġ�Դϴ�.
    /// </summary>
    public int currentCookingStat;

    /// <summary>
    /// ������ ���� ���� �ɷ�ġ�Դϴ�.
    /// </summary>
    public int currentServingStat;

    /// <summary>
    /// ������ ���� �ŷ� �ɷ�ġ�Դϴ�.
    /// </summary>
    public int currentCharmStat;

    /// <summary>
    /// ������ ���� ������ Ư�� ����Դϴ�.
    /// </summary>
    public List<Trait> currentTraits;

    /// <summary>
    [Tooltip("���� ����â���� �Ҵ�� ���� (�ֹ�, Ȧ)")]
    public EmployeeRole assignedRole = EmployeeRole.Unassigned;

    public EmployeeGrade grade;

    // --- ������ ---

    /// <summary>
    /// '������(GeneratedApplicant)' �����͸� �������� ���ο� ���� �ν��Ͻ��� �����մϴ�. (�Ϲ� ����)
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

        // �������� ����� �����մϴ�.
        this.grade = applicant.grade;

        // �Ϲ� ������ false�� ����
        isProtagonist = false;
    }

    /// <summary>
    /// '���ΰ�'ó�� ������ ���ø�(EmployeeData)���� ���� ���� �ν��Ͻ��� �����մϴ�.
    /// </summary>
    public EmployeeInstance(EmployeeData baseData)
    {
        BaseData = baseData;
        // ������ ���� �ĺ��� ���� �⺻ �̸� ����
        firstName = "Goblin Chef";
        currentLevel = 1;
        currentExperience = 0;
        skillPoints = 5; // ���ΰ��� �⺻ ��ų ����Ʈ ���� (����)
        currentSalary = baseData.salary;
        currentCookingStat = baseData.baseCookingStat;
        currentServingStat = baseData.baseServingStat;
        currentCharmStat = baseData.baseCharmStat;

        // baseData(mainCharacterTemplate)�� ����� Ư��("���ΰ�")��
        // �� ������ ���� Ư��(currentTraits) ����Ʈ�� �����մϴ�.
        currentTraits = new List<Trait>(baseData.possibleTraits);

        // (���ΰ��� �⺻ ����� C������� ����. �ʿ�� ����)
        this.grade = EmployeeGrade.C;

        // ���ΰ� ���θ� �����մϴ�.
        isProtagonist = true;
    }

    // --- �ٽ� ��� �Լ� ---

    // ************* [����ġ ���� �Լ��� ���� �������� �ʾ����Ƿ� ����] *************

    /// <summary>
    /// �丮 ���ȿ� ��ų ����Ʈ�� ����ϰ� ������ ������ŵ�ϴ�.
    /// </summary>
    /// <returns>���� ������ ���������� true�� ��ȯ�մϴ�.</returns>
    public bool SpendSkillPointOnCooking()
    {
        if (skillPoints > 0)
        {
            skillPoints--;
            currentCookingStat++;
            Debug.Log($"{firstName}: �丮 ������ {currentCookingStat}���� �����߽��ϴ�. ���� ����Ʈ: {skillPoints}");
            return true;
        }
        Debug.LogWarning($"{firstName}: ��ų ����Ʈ�� �����Ͽ� �丮 ������ �ø� �� �����ϴ�.");
        return false;
    }

    /// <summary>
    /// ���� ���ȿ� ��ų ����Ʈ�� ����ϰ� ������ ������ŵ�ϴ�.
    /// </summary>
    public bool SpendSkillPointOnServing()
    {
        if (skillPoints > 0)
        {
            skillPoints--;
            currentServingStat++;
            Debug.Log($"{firstName}: ���� ������ {currentServingStat}���� �����߽��ϴ�. ���� ����Ʈ: {skillPoints}");
            return true;
        }
        Debug.LogWarning($"{firstName}: ��ų ����Ʈ�� �����Ͽ� ���� ������ �ø� �� �����ϴ�.");
        return false;
    }

    /// <summary>
    /// �ŷ� ���ȿ� ��ų ����Ʈ�� ����ϰ� ������ ������ŵ�ϴ�.
    /// </summary>
    public bool SpendSkillPointOnCharm()
    {
        if (skillPoints > 0)
        {
            skillPoints--;
            currentCharmStat++;
            Debug.Log($"{firstName}: �ŷ� ������ {currentCharmStat}���� �����߽��ϴ�. ���� ����Ʈ: {skillPoints}");
            return true;
        }
        Debug.LogWarning($"{firstName}: ��ų ����Ʈ�� �����Ͽ� �ŷ� ������ �ø� �� �����ϴ�.");
        return false;
    }
    /// <summary>
    /// �� ������ ���� ��� Ư������ '����� ����' ���ʽ� ������ ��ȯ�մϴ�.
    /// </summary>
    public float GetTraitSaveChance()
    {
        if (currentTraits == null || currentTraits.Count == 0)
        {
            return 0f;
        }

        // ��� Ư���� 'ingredientSaveChance' ���� �ջ�
        return currentTraits.Sum(trait => trait.ingredientSaveChance);
    }


    /// <summary>
    /// (��ȹ�� ����) ������ 1������ ��ŵ�ϴ�.
    /// ��� �Ҹ�, �ִ� ���� üũ, SP 1 ȹ���� �̷�����ϴ�.
    /// </summary>
    /// <returns>������ ���� ����</returns>
    public bool TryLevelUp()
    {
        // 1. ���(Grade)�� ���� �ִ� ������ �����մϴ�. (��ȹ�� ����)
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
                maxLevel = 20; // �⺻�� C���
                break;
        }

        if (currentLevel >= maxLevel)
        {
            Debug.LogWarning($"{firstName}({this.grade}���)��(��) �̹� �ִ� ����({maxLevel})�Դϴ�.");
            return false;
        }

        // 2. ��� �Ҹ� Ȯ�� (��ȹ�� ����)
        // (��ȹ�� ����: ���� ���� ��� = ���� ���� ��� * 1.1)
        int requiredGold = (int)(100 * Mathf.Pow(1.1f, currentLevel - 1)); // (��ȹ�� 10% ���� ���� �ӽ� ����)

        if (GameManager.instance.totalGoldAmount < requiredGold)
        {
            Debug.LogWarning($"{firstName} ������ ����: ��尡 �����մϴ�. (�ʿ�: {requiredGold}G)");
            return false;
        }

        // 3. ��� �Ҹ� �� ������ ó��
        GameManager.instance.SpendGold(requiredGold);
        currentLevel++;
        skillPoints++; // �ڡڡ� ��ȹ����� SP 1 ���� �ڡڡ�

        Debug.Log($"[������!] {firstName} (Lv. {currentLevel}), SP +1. (���: {requiredGold}G)");
        return true;
    }
    /// <summary>
    /// �� ������ ���� ��� Ư������ '����� ��ĥ' ���ʽ� ������ ��ȯ�մϴ�.
    /// </summary>
    public float GetTraitStealChance()
    {
        if (currentTraits == null || currentTraits.Count == 0)
        {
            return 0f;
        }

        // ��� Ư���� 'ingredientStealChance' ���� �ջ�
        return currentTraits.Sum(trait => trait.ingredientStealChance);
    }
    /// <summary>
    /// �� ������ ���� ��� Ư������ '�丮 ���� ����' ���ʽ� ������ ��ȯ�մϴ�.
    /// </summary>
    public float GetTraitCookingStatMultiplier()
    {
        if (currentTraits == null || currentTraits.Count == 0)
        {
            return 0f;
        }

        // ��� Ư���� 'cookingStatMultiplier' ���� �ջ�
        return currentTraits.Sum(trait => trait.cookingStatMultiplier);
    }
    /// <summary>
    /// �� ������ ���� ��� Ư������ '�̵� �ӵ� ����' ���ʽ� ������ ��ȯ�մϴ�.
    /// </summary>
    public float GetTraitMoveSpeedMultiplier()
    {
        if (currentTraits == null || currentTraits.Count == 0)
        {
            return 0f;
        }
        // "������"(-0.1) ���� Ư���� ���� �ջ�
        return currentTraits.Sum(trait => trait.moveSpeedMultiplier);
    }

    /// <summary>
    /// �� ������ ���� ��� Ư������ '�۾� �ӵ� ����' ���ʽ� ������ ��ȯ�մϴ�.
    /// </summary>
    public float GetTraitWorkSpeedMultiplier()
    {
        if (currentTraits == null || currentTraits.Count == 0)
        {
            return 0f;
        }
        // "������"(-0.1) ���� Ư���� ���� �ջ�
        return currentTraits.Sum(trait => trait.workSpeedMultiplier);
    }
    /// <summary>
    /// �� ������ ���� ��� Ư������ '���� ����' ���ʽ�/���Ƽ ������ ��ȯ�մϴ�.
    /// </summary>
    public int GetTraitServiceScoreBonus()
    {
        if (currentTraits == null || currentTraits.Count == 0)
        {
            return 0;
        }

        // "�Ǽ�������"(-5)�� "������"(+?) ���� Ư���� ���� �ջ�
        return currentTraits.Sum(trait => trait.serviceScoreModifier);
    }
    /// <summary>
    /// �� ������ ���� ��� Ư������ '�� Ȯ��' ���ʽ� ������ ��ȯ�մϴ�.
    /// </summary>
    public float GetTraitTipChanceBonus()
    {
        if (currentTraits == null || currentTraits.Count == 0)
        {
            return 0f;
        }
        // "��Ȥ" Ư���� ���� �ջ�
        return currentTraits.Sum(trait => trait.tipChanceBonus);
    }

    /// <summary>
    /// �� ������ ���� ��� Ư������ '��� ���� ����' ���ʽ� ������ ��ȯ�մϴ�.
    /// </summary>
    public float GetTraitAllStatMultiplier()
    {
        if (currentTraits == null || currentTraits.Count == 0)
        {
            return 0f;
        }
        // "���ΰ�" Ư���� ���� �ջ�
        return currentTraits.Sum(trait => trait.allStatMultiplier);
    }
}