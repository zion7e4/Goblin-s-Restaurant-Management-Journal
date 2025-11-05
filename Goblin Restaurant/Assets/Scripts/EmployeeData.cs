using UnityEngine;
using System.Collections.Generic;

// 이 파일은 '직업'이 아닌 '종족'의 특성을 정의합니다.
// 예: Elf.asset, Dwarf.asset
[CreateAssetMenu(fileName = "New SpeciesData", menuName = "GoblinChef/Species Data")]
public class EmployeeData : ScriptableObject
{
    [Header("종족 기본 정보")]
    [Tooltip("UI에 표시될 종족 이름 (예: 엘프, 드워프)")]
    public string speciesName;

    [Tooltip("이 종족에 사용될 수 있는 이름 목록. 여기서 무작위로 이름이 생성됩니다.")]
    public List<string> possibleFirstNames;

    [Tooltip("종족을 대표하는 기본 초상화 (나중에 개별화 가능)")]
    public Sprite portrait;

    [Header("등장 조건")]
    [Tooltip("이 종족이 지원자 목록에 나타나기 위해 필요한 최소 명성 등급")]
    public int requiredFameTier = 1;

    [Header("종족별 기본 능력치 성향")]
    [Tooltip("이 종족의 기본 요리 능력치")]
    public int baseCookingStat = 1;
    [Tooltip("이 종족의 기본 서빙 능력치")]
    public int baseServingStat = 1;

    [Tooltip("이 종족의 기본 매력 능력치")]
    public int baseCharmStat = 1;

    [Header("종족별 성장 잠재력")]
    [Tooltip("명성 100당 이 능력치가 상승하는 평균값. (예: 0.1로 설정하면 명성 1000당 1 상승)")]
    public float cookingGrowthFactor = 0.1f;
    [Tooltip("명성 100당 이 능력치가 상승하는 평균값.")]
    public float servingGrowthFactor = 0.1f;
    [Tooltip("명성 100당 이 능력치가 상승하는 평균값.")]
    public float charmGrowthFactor = 0.1f;

    [Header("기본 급여")]
    [Tooltip("이 종족의 기본 급여. 능력치가 높게 생성되면 급여도 보너스를 받습니다.")]
    public int salary = 100;

    [Header("보유 가능 특성")]
    [Tooltip("이 종족이 가질 수 있는 모든 특성 목록. Trait 에셋 파일을 연결해주세요.")]
    public List<Trait> possibleTraits;

    [Header("맵 스폰용 프리팹")]
    public GameObject speciesPrefab;
}