using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EmployeeManager : MonoBehaviour
{
    public static EmployeeManager Instance { get; private set; }

    [Header("종족 템플릿 데이터")]
    public List<EmployeeData> allSpeciesTemplates;

    [Header("실시간 데이터")]
    public List<EmployeeInstance> hiredEmployees = new List<EmployeeInstance>();
    public List<GeneratedApplicant> applicants = new List<GeneratedApplicant>();

    // ★★★ [상수 정의] ★★★
    private const int MAX_TOTAL_EMPLOYEES = 10; // 고블린 쉐프 포함 총 직원 수 제한
    private const int MAX_APPLICANTS = 10;      // 지원자 목록에 표시될 최대 수
    // ★★★★★★★★★★★★★★★★★★★★★★★

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // [핵심 추가] 이 오브젝트가 씬이 바뀌어도 파괴되지 않도록 설정합니다.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 이미 다른 EmployeeManager가 있다면 자신을 파괴하여 중복을 막습니다.
            Destroy(gameObject);
        }
    }

    public void GenerateApplicants(int currentFame)
    {
        applicants.Clear();

        // 1. 명성도에 따른 최소/최대값 계산 (원본 값)
        int minApplicantsRaw = 1 + (currentFame / 1500);
        int maxApplicantsRaw = 2 + (currentFame / 1000);

        // 2. 최종 최대 상한을 10으로 제한합니다. (maxApplicantsRaw가 100이든 1000이든 10이 됨)
        int finalMaxLimit = Mathf.Min(maxApplicantsRaw, MAX_APPLICANTS);

        // 3. 최종 최소 하한을 계산합니다: 
        //    (minApplicantsRaw가 10을 넘는 경우, 강제로 finalMaxLimit(10)으로 제한하여 Random.Range 오류를 방지합니다.)
        int finalMinLimit = Mathf.Min(minApplicantsRaw, finalMaxLimit);

        // 4. 최종 지원자 수를 계산합니다. (min과 max 모두 10 이하이므로 결과는 10을 초과하지 않습니다.)
        int applicantCount = Random.Range(finalMinLimit, finalMaxLimit + 1);

        // ★★★ [로그] 최종 생성될 지원자 수를 확인합니다. (이 값이 10을 넘으면 안 됩니다.)
        Debug.Log($"[지원자 수 제한 확인] 명성도: {currentFame}, 최종 생성 수: {applicantCount} (MAX: {MAX_APPLICANTS})");

        if (!allSpeciesTemplates.Any()) return;

        for (int i = 0; i < applicantCount; i++) // 최종 제한된 applicantCount를 사용
        {
            EmployeeData selectedSpecies = allSpeciesTemplates[Random.Range(0, allSpeciesTemplates.Count)];
            float fameMultiplier = (float)currentFame / 100f;
            int finalCook = Random.Range(selectedSpecies.baseCookingStat + (int)(fameMultiplier * selectedSpecies.cookingGrowthFactor * 0.8f), selectedSpecies.baseCookingStat + (int)(fameMultiplier * selectedSpecies.cookingGrowthFactor * 1.2f) + 1);
            int finalServe = Random.Range(selectedSpecies.baseServingStat + (int)(fameMultiplier * selectedSpecies.servingGrowthFactor * 0.8f), selectedSpecies.baseServingStat + (int)(fameMultiplier * selectedSpecies.servingGrowthFactor * 1.2f) + 1);
            int finalClean = Random.Range(selectedSpecies.baseCleaningStat + (int)(fameMultiplier * selectedSpecies.cleaningGrowthFactor * 0.8f), selectedSpecies.baseCleaningStat + (int)(fameMultiplier * selectedSpecies.cleaningGrowthFactor * 1.2f) + 1);
            string jobTitle = "신입";
            if (finalCook >= finalServe && finalCook >= finalClean) { jobTitle = "요리사"; } else if (finalServe > finalCook && finalServe >= finalClean) { jobTitle = "서버"; } else { jobTitle = "매니저"; }
            string firstName = selectedSpecies.speciesName;
            if (selectedSpecies.possibleFirstNames.Any()) { firstName = selectedSpecies.possibleFirstNames[Random.Range(0, selectedSpecies.possibleFirstNames.Count)]; }
            List<Trait> finalTraits = new List<Trait>();
            if (selectedSpecies.possibleTraits.Any())
            {
                float traitChance = Mathf.Min(5 + (currentFame / 100f), 90);
                if (Random.Range(0, 100) < traitChance) { finalTraits.Add(selectedSpecies.possibleTraits[Random.Range(0, selectedSpecies.possibleTraits.Count)]); }
            }
            GeneratedApplicant newApplicant = new GeneratedApplicant(selectedSpecies, firstName, jobTitle, finalCook, finalServe, finalClean, finalTraits);
            applicants.Add(newApplicant);
        }

        if (UIManager.Instance != null) { UIManager.Instance.UpdateApplicantListUI(applicants); }
    }

    public void HireEmployee(GeneratedApplicant applicantToHire)
    {
        // ★★★ [수정] 고용 인원 확인: 총 직원 수가 MAX_TOTAL_EMPLOYEES(10) 이상이면 고용 불가 ★★★
        if (hiredEmployees.Count >= MAX_TOTAL_EMPLOYEES)
        {
            Debug.LogWarning($"최대 고용 인원({MAX_TOTAL_EMPLOYEES}명)에 도달하여 더 이상 직원을 고용할 수 없습니다.");
            return; // 고용 진행을 막고 함수 종료
        }
        // ★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★

        if (applicants.Contains(applicantToHire))
        {
            int hiringCost = applicantToHire.BaseSpeciesData.salary;
            // TODO: 경제 시스템 연동 시 여기에 SpendMoney() 체크 추가

            EmployeeInstance newEmployee = new EmployeeInstance(applicantToHire);
            hiredEmployees.Add(newEmployee);
            applicants.Remove(applicantToHire);
            Debug.Log($"{newEmployee.BaseData.speciesName} {newEmployee.firstName}(을)를 {hiringCost}원에 고용했습니다.");

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateApplicantListUI(applicants);
            }
        }
    }

    /// <summary>
    /// 고용된 직원을 해고합니다. 해고 시 목록에서 제거하고 UI를 업데이트합니다.
    /// </summary>
    public void DismissEmployee(EmployeeInstance employeeToDismiss)
    {
        if (hiredEmployees.Contains(employeeToDismiss))
        {
            hiredEmployees.Remove(employeeToDismiss);
            Debug.Log($"{employeeToDismiss.firstName} 직원({employeeToDismiss.BaseData.speciesName})을 해고했습니다.");

            // 해고 후 직원 관리 UI를 새로고침합니다.
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateHiredEmployeeListUI();
            }
        }
    }
}