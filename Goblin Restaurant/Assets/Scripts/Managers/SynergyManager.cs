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
    /// </summary>
    private bool CheckSynergyConditions(Synergy synergy, List<EmployeeInstance> employees)
    {
        // --- 1. 종족 조건 검사 ---
        if (synergy.speciesConditions != null && synergy.speciesConditions.Count > 0)
        {
            foreach (SynergyCondition condition in synergy.speciesConditions)
            {
                // 배치(Role) 조건까지 포함하여 카운트
                int count = employees.Count(emp =>
                    emp.BaseData == condition.species &&
                    (!condition.requirePlacement || emp.assignedRole == condition.requiredRole)
                );

                if (count < condition.minCount) return false;
            }
        }

        // --- 2. 특정 특성 조합 검사 ---
        if (synergy.traitConditions != null && synergy.traitConditions.Count > 0)
        {
            foreach (SynergyTraitCondition condition in synergy.traitConditions)
            {
                // 배치(Role) 조건까지 포함하여 카운트
                int count = employees.Count(emp =>
                    emp.currentTraits.Contains(condition.requiredTrait) &&
                    (!condition.requirePlacement || emp.assignedRole == condition.requiredRole)
                );

                if (count < condition.minCount) return false;
            }
        }

        // --- 3. 부정적 특성 총 개수 검사 ---
        if (synergy.minNegativeTraitCount > 0)
        {
            int totalNegativeTraits = 0;
            foreach (EmployeeInstance emp in employees)
            {
                totalNegativeTraits += emp.currentTraits.Count(trait => !trait.isPositive);
            }

            if (totalNegativeTraits < synergy.minNegativeTraitCount) return false;
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
    /// (Employee.cs가 호출) '특정 직원'에게 적용되는 '요리 속도' 보너스 총합을 반환합니다.
    /// </summary>
    public float GetCookingSpeedBonus(EmployeeInstance employee)
    {
        float totalBonus = 0f;

        foreach (Synergy synergy in activeSynergies)
        {
            // (GetStatBonuses와 동일한 로직으로 '효과 대상'인지 확인)

            // (1. 종족 시너지인가?)
            if (synergy.speciesConditions != null && synergy.speciesConditions.Count > 0)
            {
                bool isTargeted = synergy.speciesConditions.Any(condition =>
                    condition.species == employee.BaseData &&
                    (!condition.requirePlacement || employee.assignedRole == condition.requiredRole)
                );
                if (isTargeted) totalBonus += synergy.cookingSpeedBonus;
            }

            // (2. 특성 시너지인가?)
            if (synergy.traitConditions != null && synergy.traitConditions.Count > 0)
            {
                bool isTargeted = synergy.traitConditions.Any(condition =>
                    employee.currentTraits.Contains(condition.requiredTrait) &&
                    (!condition.requirePlacement || employee.assignedRole == condition.requiredRole)
                );
                if (isTargeted) totalBonus += synergy.cookingSpeedBonus;
            }

            // (3. 부정적 특성 시너지인가?)
            if (synergy.minNegativeTraitCount > 0)
            {
                // (이 시너지는 '모든' 직원에게 적용된다고 가정)
                totalBonus += synergy.cookingSpeedBonus;
            }
        }
        return totalBonus;
    }

    /// <summary>
    /// (Customer.cs가 호출) 현재 발동 중인 '명성 획득' 보너스 총합을 반환합니다.
    /// </summary>
    public float GetFameBonusPercent()
    {
        return activeSynergies.Sum(s => s.fameBonusPercent);
    }

    /// <summary>
    /// (Employee.cs가 호출) '특정 직원'에게 적용되는 '스탯' 보너스 총합을 반환합니다.
    /// </summary>
    public (int cook, int serve, int charm) GetStatBonuses(EmployeeInstance employee)
    {
        int totalCook = 0;
        int totalServe = 0;
        int totalCharm = 0;

        // 현재 발동 중인 모든 시너지를 확인
        foreach (Synergy synergy in activeSynergies)
        {
            // --- 이 직원이 이 시너지의 '효과 대상'인지 확인 ---

            // (1. 종족 시너지인가?)
            if (synergy.speciesConditions != null && synergy.speciesConditions.Count > 0)
            {
                // "이 직원의 종족"이 시너지 조건 목록에 포함되어 있는지 확인
                bool isTargeted = synergy.speciesConditions.Any(condition =>
                    condition.species == employee.BaseData &&
                    // (배치 조건 확인) 배치가 필요 없거나, 직원의 역할이 일치하는가?
                    (!condition.requirePlacement || employee.assignedRole == condition.requiredRole)
                );

                if (isTargeted)
                {
                    totalCook += synergy.cookingStatBonus;
                    totalServe += synergy.servingStatBonus;
                    totalCharm += synergy.charmStatBonus;
                }
            }

            // (2. 특성 시너지인가?)
            if (synergy.traitConditions != null && synergy.traitConditions.Count > 0)
            {
                // "이 직원의 특성"이 시너지 조건 목록에 포함되어 있는지 확인
                bool isTargeted = synergy.traitConditions.Any(condition =>
                    employee.currentTraits.Contains(condition.requiredTrait) &&
                    // (배치 조건 확인) 배치가 필요 없거나, 직원의 역할이 일치하는가?
                    (!condition.requirePlacement || employee.assignedRole == condition.requiredRole)
                );

                if (isTargeted)
                {
                    totalCook += synergy.cookingStatBonus;
                    totalServe += synergy.servingStatBonus;
                    totalCharm += synergy.charmStatBonus;
                }
            }

            // (3. 부정적 특성 시너지인가?)
            if (synergy.minNegativeTraitCount > 0)
            {
                // (이 시너지는 '모든' 직원에게 적용된다고 가정)
                totalCook += synergy.cookingStatBonus;
                totalServe += synergy.servingStatBonus;
                totalCharm += synergy.charmStatBonus;
            }
        }

        return (totalCook, totalServe, totalCharm);
    }

    /// <summary>
    /// (Employee.cs가 호출) 현재 발동 중인 '이동 속도' 보너스 총합을 반환합니다.
    /// </summary>
    public float GetMoveSpeedMultiplier()
    {
        // (참고: 이 함수는 '우울한 작업장'처럼 모든 직원에게 적용됩니다)
        return activeSynergies.Sum(s => s.moveSpeedMultiplier);
    }

    /// <summary>
    /// (Customer.cs가 호출) 현재 발동 중인 '서비스 점수' 보너스 총합을 반환합니다.
    /// </summary>
    public int GetServiceScoreBonus()
    {
        // (참고: 이 함수는 '활기찬 식당'처럼 모든 손님에게 적용됩니다)
        return activeSynergies.Sum(s => s.serviceScoreBonus);
    }

    /// <summary>
    /// (Employee.cs가 호출) 현재 발동 중인 '식재료 절약' 확률 총합을 반환합니다.
    /// </summary>
    public float GetIngredientSaveChance()
    {
        // (참고: 이 함수는 '완벽주의 주방'처럼 주방 전체에 적용됩니다)
        return activeSynergies.Sum(s => s.ingredientSaveChance);
    }
}