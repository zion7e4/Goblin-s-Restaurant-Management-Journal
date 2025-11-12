using UnityEngine;
using System.Collections.Generic;

// �� ������ '����'�� �ƴ� '����'�� Ư���� �����մϴ�.
// ��: Elf.asset, Dwarf.asset
[CreateAssetMenu(fileName = "New SpeciesData", menuName = "GoblinChef/Species Data")]
public class EmployeeData : ScriptableObject
{
    [Header("���� �⺻ ����")]
    [Tooltip("UI�� ǥ�õ� ���� �̸� (��: ����, �����)")]
    public string speciesName;

    [Tooltip("�� ������ ���� �� �ִ� �̸� ���. ���⼭ �������� �̸��� �����˴ϴ�.")]
    public List<string> possibleFirstNames;

    [Tooltip("������ ��ǥ�ϴ� �⺻ �ʻ�ȭ (���߿� ����ȭ ����)")]
    public Sprite portrait;

    [Header("���� ����")]
    [Tooltip("�� ������ ������ ��Ͽ� ��Ÿ���� ���� �ʿ��� �ּ� ���� ���")]
    public int requiredFameTier = 1;

    [Header("������ �⺻ �ɷ�ġ ����")]
    [Tooltip("�� ������ �⺻ �丮 �ɷ�ġ")]
    public int baseCookingStat = 1;
    [Tooltip("�� ������ �⺻ ���� �ɷ�ġ")]
    public int baseServingStat = 1;

    [Tooltip("�� ������ �⺻ �ŷ� �ɷ�ġ")]
    public int baseCharmStat = 1;

    [Tooltip("�� ������ �⺻ �̵� �ӵ�")]
    public float baseMoveSpeed = 1f; // (�⺻�� 1)

    [Header("������ ���� �����")]
    [Tooltip("���� 100�� �� �ɷ�ġ�� ����ϴ� ��հ�. (��: 0.1�� �����ϸ� ���� 1000�� 1 ���)")]
    public float cookingGrowthFactor = 0.1f;
    [Tooltip("���� 100�� �� �ɷ�ġ�� ����ϴ� ��հ�.")]
    public float servingGrowthFactor = 0.1f;
    [Tooltip("���� 100�� �� �ɷ�ġ�� ����ϴ� ��հ�.")]
    public float charmGrowthFactor = 0.1f;

    [Header("�⺻ �޿�")]
    [Tooltip("�� ������ �⺻ �޿�. �ɷ�ġ�� ���� �����Ǹ� �޿��� ���ʽ��� �޽��ϴ�.")]
    public int salary = 100;

    [Header("���� ���� Ư��")]
    [Tooltip("�� ������ ���� �� �ִ� ��� Ư�� ���. Trait ���� ������ �������ּ���.")]
    public List<Trait> possibleTraits;

    [Header("�� ������ ������")]
    public GameObject speciesPrefab;
}