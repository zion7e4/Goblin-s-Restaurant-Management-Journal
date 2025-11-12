using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 모든 시너지를 관리하고, 현재 발동 중인 시너지를 계산하여 적용합니다.
/// </summary>
public class SynergyManager : MonoBehaviour
{
    public static SynergyManager Instance { get; private set; }

    [Header("시너지 데이터")]
    [Tooltip("게임에 존재하는 모든 시너지 SO 에셋 목록")]
    public List<Synergy> allSynergies;

    [Header("실시간 상태")]
    [Tooltip("현재 발동 중인 시너지 목록 (읽기 전용)")]
    public List<Synergy> activeSynergies = new List<Synergy>();

    private List<EmployeeInstance> lastCheckedEmployees; // 비교용

    void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }
    }

    /// <summary>
    /// EmployeeManager가 직원을 고용/해고할 때 호출해야 하는 함수.
    /// 현재 직원 목록을 기준으로 모든 시너지를 다시 계산합니다.
    /// </summary>
    public void UpdateActiveSynergies(List<EmployeeInstance> currentHiredEmployees)
    {
        lastCheckedEmployees = currentHiredEmployees;
        activeSynergies.Clear();

        foreach (Synergy synergy in allSynergies)
        {
            // 이 시너지의 모든 조건을 만족하는지 검사
            if (CheckSynergyConditions(synergy, currentHiredEmployees))
            {
                activeSynergies.Add(synergy);
                Debug.Log($"[시너지 발동!] {synergy.synergyName}");
            }
        }

        // (TODO: 시너지 발동/해제 UI 갱신 호출)
        // UIManager.Instance.UpdateSynergyUI(activeSynergies);
    }

    /// <summary>
    /// 특정 시너지의 모든 발동 조건을 확인합니다.
    /// (참고: 이 함수는 아직 '배치' 조건은 확인하지 않습니다.)
    /// </summary>
    /// <summary>
    /// 특정 시너지의 모든 발동 조건을 확인합니다.
    /// (참고: 이 함수는 아직 '배치' 조건은 확인하지 않습니다.)
    /// </summary>
    private bool CheckSynergyConditions(Synergy synergy, List<EmployeeInstance> employees)
    {
        // --- 1. 종족 조건 검사 ---
        if (synergy.speciesConditions != null && synergy.speciesConditions.Count > 0)
        {
            // 시너지의 '모든' 종족 조건을 만족해야 함
            foreach (SynergyCondition condition in synergy.speciesConditions)
            {
                // 1. 특정 종족(species)의 직원이
                // 2. condition.minCount 만큼 있는지 확인
                int count = employees.Count(emp => emp.BaseData == condition.species);

                // (TODO: '배치' 조건 확인 로직)
                // if (condition.requirePlacement) { ... }

                // 조건을 만족하는 직원이 1명이라도 부족하면 즉시 실패
                if (count < condition.minCount)
                {
                    return false;
                }
            }
        }

        // --- 2. 특정 특성 조합 검사 ---
        if (synergy.traitConditions != null && synergy.traitConditions.Count > 0)
        {
            // 시너지의 '모든' 특성 조건을 만족해야 함
            foreach (SynergyTraitCondition condition in synergy.traitConditions)
            {
                // 1. 특정 특성(requiredTrait)을 가진 직원이
                // 2. condition.minCount 만큼 있는지 확인
                int count = employees.Count(emp => emp.currentTraits.Contains(condition.requiredTrait));

                // (TODO: '배치' 조건 확인 로직)
                // if (condition.requirePlacement) { ... }

                // 조건을 만족하는 직원이 1명이라도 부족하면 즉시 실패
                if (count < condition.minCount)
                {
                    return false;
                }
            }
        }

        // --- 3. 부정적 특성 총 개수 검사 ---
        if (synergy.minNegativeTraitCount > 0)
        {
            int totalNegativeTraits = 0;
            foreach (EmployeeInstance emp in employees)
            {
                // (Trait.cs에 isPositive 변수가 있다고 가정)
                totalNegativeTraits += emp.currentTraits.Count(trait => !trait.isPositive);
            }

            // 요구하는 부정적 특성 개수보다 적으면 실패
            if (totalNegativeTraits < synergy.minNegativeTraitCount)
            {
                return false;
            }
        }

        // (예외 처리: 아무 조건도 없으면 발동 안 함)
        if ((synergy.speciesConditions == null || synergy.speciesConditions.Count == 0) &&
            (synergy.traitConditions == null || synergy.traitConditions.Count == 0) &&
            (synergy.minNegativeTraitCount <= 0))
        {
            return false;
        }

        // 모든 조건을 통과했으면 성공
        return true;
    }


    // --- 다른 스크립트가 효과를 물어볼 수 있는 함수들 ---

    /// <summary>
    /// (Employee.cs가 호출) 현재 발동 중인 '요리 속도' 보너스 총합을 반환합니다.
    /// (예: 0.05f (5%) 반환)
    /// </summary>
    public float GetCookingSpeedBonus()
    {
        // (TODO: 이 직원이 시너지 대상인지 확인하는 로직 필요)
        return activeSynergies.Sum(s => s.cookingSpeedBonus);
    }

    /// <summary>
    /// (Customer.cs가 호출) 현재 발동 중인 '명성 획득' 보너스 총합을 반환합니다.
    /// (예: 0.1f (10%) 반환)
    /// </summary>
    public float GetFameBonusPercent()
    {
        return activeSynergies.Sum(s => s.fameBonusPercent);
    }

    /// <summary>
    /// (Employee.cs가 호출) 이 직원에게 적용되는 '스탯' 보너스를 반환합니다.
    /// (참고: 이 함수는 '이 직원'이 시너지 대상인지 확인해야 함 - 지금은 단순 합산)
    /// </summary>
    public (int cook, int serve, int charm) GetStatBonuses(EmployeeInstance employee)
    {
        // (TODO: '굳건한 주방'처럼 특정 직원에게만 적용되는 로직 필요)

        int totalCook = activeSynergies.Sum(s => s.cookingStatBonus);
        int totalServe = activeSynergies.Sum(s => s.servingStatBonus);
        int totalCharm = activeSynergies.Sum(s => s.charmStatBonus);

        return (totalCook, totalServe, totalCharm);
    }

    /// <summary>
    /// (Employee.cs가 호출) 현재 발동 중인 '이동 속도' 보너스 총합을 반환합니다.
    /// (예: -0.05f (-5%) 반환)
    /// </summary>
    public float GetMoveSpeedMultiplier()
    {
        // "우울한 작업장"(-0.05) + "활기찬 식당"(+0.1) = +0.05
        return activeSynergies.Sum(s => s.moveSpeedMultiplier);
    }

    /// <summary>
    /// (Customer.cs가 호출) 현재 발동 중인 '서비스 점수' 보너스 총합을 반환합니다.
    /// (예: +2 또는 -2)
    /// </summary>
    public int GetServiceScoreBonus()
    {
        return activeSynergies.Sum(s => s.serviceScoreBonus);
    }

    /// <summary>
    /// (Employee.cs가 호출) 현재 발동 중인 '식재료 절약' 확률 총합을 반환합니다.
    /// (예: 0.05f (5%))
    /// </summary>
    public float GetIngredientSaveChance()
    {
        return activeSynergies.Sum(s => s.ingredientSaveChance);
    }
}