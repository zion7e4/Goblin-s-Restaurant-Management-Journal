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

    // 상수 정의
    private const int MAX_TOTAL_EMPLOYEES = 10; // 고블린 쉐프 포함 총 직원 수 제한
    private const int MAX_APPLICANTS = 10;     // 지원자 목록에 표시될 최대 수

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 이 오브젝트가 씬이 바뀌어도 파괴되지 않도록 설정합니다.
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

        // 템플릿 리스트에서 Null이 아닌 유효한 템플릿만 필터링합니다. (널 참조 방지)
        List<EmployeeData> validTemplates = allSpeciesTemplates
            .Where(t => t != null)
            .ToList();

        if (!validTemplates.Any())
        {
            Debug.LogError("GenerateApplicants 오류: 유효한 EmployeeData 템플릿이 EmployeeManager에 연결되어 있지 않습니다!");
            return;
        }

        // (기존 명성도 기반 계산식 주석 처리)
        // int minApplicantsRaw = 1 + (currentFame / 1500);
        // int maxApplicantsRaw = 2 + (currentFame / 1000);

        // 기획서 기준으로 3~5명의 후보가 등장 
        int minApplicantsRaw = 3;
        int maxApplicantsRaw = 5;

        // 2. 최종 최대 상한을 10으로 제한합니다. (MAX_APPLICANTS 값은 유지)
        int finalMaxLimit = Mathf.Min(maxApplicantsRaw, MAX_APPLICANTS);

        // 3. 최종 최소 하한을 계산합니다: (Random.Range 오류 방지)
        int finalMinLimit = Mathf.Min(minApplicantsRaw, finalMaxLimit);

        // 4. 최종 지원자 수를 계산합니다. 
        int applicantCount = Random.Range(finalMinLimit, finalMaxLimit + 1);

        // 최종 생성될 지원자 수를 확인합니다.
        Debug.Log($"[지원자 수 제한 확인] 명성도: {currentFame}, 최종 생성 수: {applicantCount} (기획서 기준 3~5명)");


        for (int i = 0; i < applicantCount; i++)
        {
            // 유효한 템플릿 리스트 중에서 랜덤 선택
            EmployeeData selectedSpecies = validTemplates[Random.Range(0, validTemplates.Count)];

            // (능력치 배율) 최대 4.0까지 (0 ~ 400 / 100)
            float fameMultiplier = (float)currentFame / 100f;

            // [참고] 이 스탯 계산식은 기획서의 '등급' 기반이 아닌 '명성도' 기반으로 유지했습니다.
            // 스탯이 너무 낮다면, EmployeeData의 base...Stat이나 ...GrowthFactor 값을 조정해주세요.
            int finalCook = Random.Range(selectedSpecies.baseCookingStat + (int)(fameMultiplier * selectedSpecies.cookingGrowthFactor * 0.8f), selectedSpecies.baseCookingStat + (int)(fameMultiplier * selectedSpecies.cookingGrowthFactor * 1.2f) + 1);
            int finalServe = Random.Range(selectedSpecies.baseServingStat + (int)(fameMultiplier * selectedSpecies.servingGrowthFactor * 0.8f), selectedSpecies.baseServingStat + (int)(fameMultiplier * selectedSpecies.servingGrowthFactor * 1.2f) + 1);
            int finalCharm = Random.Range(selectedSpecies.baseCharmStat + (int)(fameMultiplier * selectedSpecies.charmGrowthFactor * 0.8f), selectedSpecies.baseCharmStat + (int)(fameMultiplier * selectedSpecies.charmGrowthFactor * 1.2f) + 1);

            string jobTitle = "신입";
            if (finalCook >= finalServe && finalCook >= finalCharm) { jobTitle = "요리사"; }
            else if (finalServe > finalCook && finalServe >= finalCharm) { jobTitle = "서버"; }
            else { jobTitle = "매니저"; }

            string firstName = selectedSpecies.speciesName;

            // Null 체크: possibleFirstNames 리스트가 Null이 아니며, 항목이 있는지 확인
            if (selectedSpecies.possibleFirstNames != null && selectedSpecies.possibleFirstNames.Any())
            {
                firstName = selectedSpecies.possibleFirstNames[Random.Range(0, selectedSpecies.possibleFirstNames.Count)];
            }

            List<Trait> finalTraits = new List<Trait>();
            // Null 체크: possibleTraits 리스트가 Null이 아니며, 항목이 있는지 확인
            if (selectedSpecies.possibleTraits != null && selectedSpecies.possibleTraits.Any())
            {
                // (기존 코드 주석 처리)
                // float famePercent = (float)currentFame / 400f;
                // float traitChance = 5f + (famePercent * 40f); 

                // 40%를 기본 확률로, 100 명성도당 10%씩 증가
                float traitChance = 40f + ((float)currentFame / 100f) * 10f;

                // (안전 장치) 확률이 100%를 넘지 않도록 제한합니다.
                traitChance = Mathf.Min(traitChance, 100f);

                if (Random.Range(0, 100) < traitChance)
                {
                    Trait selectedTrait = selectedSpecies.possibleTraits[Random.Range(0, selectedSpecies.possibleTraits.Count)];
                    if (selectedTrait != null) // 선택된 특성 객체 자체도 Null인지 체크
                    {
                        finalTraits.Add(selectedTrait);
                    }
                }
            }

            GeneratedApplicant newApplicant = new GeneratedApplicant(selectedSpecies, firstName, jobTitle, finalCook, finalServe, finalCharm, finalTraits);
        }

        // 지원자 목록 UI 갱신 (Null 체크 추가)
        if (EmployeeUI_Controller.Instance != null && EmployeeUI_Controller.Instance.employeeSubMenuPanel != null && EmployeeUI_Controller.Instance.employeeSubMenuPanel.activeSelf)
        {
            EmployeeUI_Controller.Instance.UpdateApplicantListUI(applicants);
        }
    }

    public void HireEmployee(GeneratedApplicant applicantToHire)
    {
        // 고용 인원 확인: 총 직원 수가 MAX_TOTAL_EMPLOYEES(10) 이상이면 고용 불가
        if (hiredEmployees.Count >= MAX_TOTAL_EMPLOYEES)
        {
            Debug.LogWarning($"최대 고용 인원({MAX_TOTAL_EMPLOYEES}명)에 도달하여 더 이상 직원을 고용할 수 없습니다.");
            return; // 고용 진행을 막고 함수 종료
        }

        if (applicants.Contains(applicantToHire))
        {
            int hiringCost = applicantToHire.BaseSpeciesData.salary;
            // TODO: 경제 시스템 연동 시 여기에 SpendMoney() 체크 추가

            // 1. 새 직원 인스턴스(데이터) 생성
            EmployeeInstance newEmployee = new EmployeeInstance(applicantToHire);

            // 2. 데이터 리스트에 추가
            hiredEmployees.Add(newEmployee);
            applicants.Remove(applicantToHire);
            Debug.Log($"{newEmployee.BaseData.speciesName} {newEmployee.firstName}(을)를 {hiringCost}원에 고용했습니다.");

            // 3. 스폰할 프리팹 가져오기 (EmployeeData에 speciesPrefab 변수 필요)
            GameObject prefabToSpawn = applicantToHire.BaseSpeciesData.speciesPrefab;

            // 4. RestaurantManager의 스폰 함수 호출
            if (RestaurantManager.instance != null && prefabToSpawn != null)
            {
                RestaurantManager.instance.SpawnSingleWorker(newEmployee, prefabToSpawn);
            }
            else
            {
                Debug.LogError($"[EmployeeManager] 스폰 실패! RestaurantManager.instance가 없거나 " +
                               $"{newEmployee.firstName}의 Prefab ({newEmployee.BaseData.speciesName})이 null입니다.");
            }

            // 5. UI 갱신 (기존 코드)
            if (EmployeeUI_Controller.Instance != null)
            {
                // 지원자 목록과 고용된 직원 목록 모두 갱신
                EmployeeUI_Controller.Instance.UpdateApplicantListUI(applicants);
                EmployeeUI_Controller.Instance.UpdateHiredEmployeeListUI();
            }
        }
    }

    /// <summary>
    /// 고용된 직원을 해고합니다. 해고 시 목록에서 제거하고 UI를 업데이트합니다.
    /// </summary>
    public void DismissEmployee(EmployeeInstance employeeToDismiss)
    {
        // (이 함수는 수정되지 않았습니다)
        if (hiredEmployees.Contains(employeeToDismiss))
        {
            hiredEmployees.Remove(employeeToDismiss);
            Debug.Log($"{employeeToDismiss.firstName} 직원({employeeToDismiss.BaseData.speciesName})을 해고했습니다.");

            // 해고 후 직원 관리 UI를 새로고침합니다.
            if (EmployeeUI_Controller.Instance != null)
            {
                EmployeeUI_Controller.Instance.UpdateHiredEmployeeListUI();
            }
        }
    }
}