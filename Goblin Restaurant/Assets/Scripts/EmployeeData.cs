using UnityEngine;
using System.Collections.Generic;

// 이 스크립트는 '종족'이 아닌 '직원종'의 특성을 정의합니다.
// 예: Elf.asset, Dwarf.asset
[CreateAssetMenu(fileName = "New SpeciesData", menuName = "GoblinChef/Species Data")]
public class EmployeeData : ScriptableObject
{
    [Header("종족 기본 정보")]
    [Tooltip("UI에 표시될 종족 이름 (예: 엘프, 드워프)")]
    public string speciesName;

    [Tooltip("이 종족이 가질 수 있는 이름 목록. 여기서 랜덤으로 이름이 정해집니다.")]
    public List<string> possibleFirstNames;

    [Tooltip("종족을 대표하는 기본 초상화 (나중에 변경 가능)")]
    public Sprite portrait;

    [Header("채용 조건")]
    [Tooltip("이 종족의 직원을 고용할 수 있게 되는 최소 명성 단계")]
    public int requiredFameTier = 1;

    [Header("직원 기본 능력치 설정")]
    [Tooltip("이 종족의 기본 요리 능력치")]
    public int baseCookingStat = 1;

    [Tooltip("이 종족의 기본 서빙 능력치")]
    public int baseServingStat = 1;

    [Tooltip("이 종족의 기본 매력 능력치")]
    public int baseCharmStat = 1;

    [Tooltip("이 종족의 기본 이동 속도")]
    public float baseMoveSpeed = 1f; // (기본값 1)

    [Header("능력치 성장 계수")]
    [Tooltip("경험치 100당 능력치가 증가하는 비율. (예: 0.1이면 경험치 1000에 1 증가)")]
    public float cookingGrowthFactor = 0.1f;

    [Tooltip("경험치 100당 능력치가 증가하는 비율.")]
    public float servingGrowthFactor = 0.1f;

    [Tooltip("경험치 100당 능력치가 증가하는 비율.")]
    public float charmGrowthFactor = 0.1f;

    [Header("기본 급여")]
    [Tooltip("이 종족의 기본 급여. 능력치가 높은 직원일수록 급여가 비싸집니다.")]
    public int salary = 100;

    [Header("가능한 특성 목록")]
    [Tooltip("이 종족이 가질 수 있는 모든 특성 목록. Trait ScriptableObject를 연결해주세요.")]
    public List<Trait> possibleTraits;

    [Header("종족 프리팹")]
    [Tooltip("이 종족의 직원 캐릭터 프리팹")]
    public GameObject speciesPrefab;
}
