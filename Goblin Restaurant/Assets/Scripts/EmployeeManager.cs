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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GenerateApplicants(int currentFame)
    {
        applicants.Clear();

        // 템플릿 리스트에서 Null이 아닌 유효한 템플릿만 필터링합니다.
        List<EmployeeData> validTemplates = allSpeciesTemplates
            .Where(t => t != null)
            .ToList();

        if (!validTemplates.Any())
        {
            Debug.LogError("GenerateApplicants 오류: 유효한 EmployeeData 템플릿이 EmployeeManager에 연결되어 있지 않습니다!");
            return;
        }

        // 기획서 기준으로 3~5명의 후보가 등장 
        int minApplicantsRaw = 3;
        int maxApplicantsRaw = 5;

        // 최종 최대 상한을 10으로 제한 (MAX_APPLICANTS)
        int finalMaxLimit = Mathf.Min(maxApplicantsRaw, MAX_APPLICANTS);

        // 최종 최소 하한 계산
        int finalMinLimit = Mathf.Min(minApplicantsRaw, finalMaxLimit);

        // 최종 지원자 수 계산
        int applicantCount = Random.Range(finalMinLimit, finalMaxLimit + 1);

        Debug.Log($"[지원자 수 제한 확인] 명성도: {currentFame}, 최종 생성 수: {applicantCount}");

        for (int i = 0; i < applicantCount; i++)
        {
            EmployeeData selectedSpecies = validTemplates[Random.Range(0, validTemplates.Count)];

            float fameMultiplier = (float)currentFame / 100f;

            int finalCook = Random.Range(selectedSpecies.baseCookingStat + (int)(fameMultiplier * selectedSpecies.cookingGrowthFactor * 0.8f), selectedSpecies.baseCookingStat + (int)(fameMultiplier * selectedSpecies.cookingGrowthFactor * 1.2f) + 1);
            int finalServe = Random.Range(selectedSpecies.baseServingStat + (int)(fameMultiplier * selectedSpecies.servingGrowthFactor * 0.8f), selectedSpecies.baseServingStat + (int)(fameMultiplier * selectedSpecies.servingGrowthFactor * 1.2f) + 1);
            int finalCharm = Random.Range(selectedSpecies.baseCharmStat + (int)(fameMultiplier * selectedSpecies.charmGrowthFactor * 0.8f), selectedSpecies.baseCharmStat + (int)(fameMultiplier * selectedSpecies.charmGrowthFactor * 1.2f) + 1);

            string jobTitle = "신입";
            if (finalCook >= finalServe && finalCook >= finalCharm) { jobTitle = "요리사"; }
            else if (finalServe > finalCook && finalServe >= finalCharm) { jobTitle = "서버"; }
            else { jobTitle = "매니저"; }

            string firstName = selectedSpecies.speciesName;

            if (selectedSpecies.possibleFirstNames != null && selectedSpecies.possibleFirstNames.Any())
            {
                firstName = selectedSpecies.possibleFirstNames[Random.Range(0, selectedSpecies.possibleFirstNames.Count)];
            }

            // --- 1. 등급 추첨 ---
            EmployeeGrade finalGrade = DetermineGrade(currentFame);

            // --- 2. 특성 추첨 ---
            List<Trait> finalTraits = new List<Trait>();
            if (selectedSpecies.possibleTraits != null && selectedSpecies.possibleTraits.Any())
            {
                float traitChance = 40f + ((float)currentFame / 100f) * 10f;
                traitChance = Mathf.Min(traitChance, 100f);

                if (UnityEngine.Random.Range(0, 100) < traitChance)
                {
                    Trait selectedTrait = selectedSpecies.possibleTraits[Random.Range(0, selectedSpecies.possibleTraits.Count)];
                    if (selectedTrait != null)
                    {
                        finalTraits.Add(selectedTrait);
                    }
                }
            }

            // --- 3. 지원자 생성 ---
            GeneratedApplicant newApplicant = new GeneratedApplicant(
                selectedSpecies, firstName, jobTitle,
                finalCook, finalServe, finalCharm,
                finalTraits, finalGrade);

            applicants.Add(newApplicant);
        }

        // [수정됨] employeeSubMenuPanel 체크 제거 -> UI 컨트롤러가 존재하면 리스트만 갱신해둠
        if (EmployeeUI_Controller.Instance != null)
        {
            EmployeeUI_Controller.Instance.UpdateApplicantListUI(applicants);
        }
    }

    private EmployeeGrade DetermineGrade(int currentFame)
    {
        float fameRatio = (float)currentFame / 400f;
        fameRatio = Mathf.Clamp01(fameRatio);

        float s_Chance = 0.05f + (fameRatio * 0.05f);
        float a_Chance = 0.10f + (fameRatio * 0.15f);
        float b_Chance = 0.25f + (fameRatio * 0.05f);

        float gradeRoll = UnityEngine.Random.Range(0f, 1f);

        if (gradeRoll < s_Chance) return EmployeeGrade.S;
        else if (gradeRoll < s_Chance + a_Chance) return EmployeeGrade.A;
        else if (gradeRoll < s_Chance + a_Chance + b_Chance) return EmployeeGrade.B;
        else return EmployeeGrade.C;
    }

    public void HireEmployee(GeneratedApplicant applicantToHire)
    {
        if (hiredEmployees.Count >= MAX_TOTAL_EMPLOYEES)
        {
            Debug.LogWarning($"최대 고용 인원({MAX_TOTAL_EMPLOYEES}명)에 도달했습니다.");
            return;
        }

        if (applicants.Contains(applicantToHire))
        {
            int hiringCost = applicantToHire.BaseSpeciesData.salary;

            // 1. 새 직원 인스턴스 생성
            EmployeeInstance newEmployee = new EmployeeInstance(applicantToHire);

            // 2. 리스트 이동
            hiredEmployees.Add(newEmployee);
            applicants.Remove(applicantToHire);
            Debug.Log($"{newEmployee.BaseData.speciesName} {newEmployee.firstName} 고용 완료 (비용: {hiringCost})");

            // 3. 프리팹 스폰
            GameObject prefabToSpawn = applicantToHire.BaseSpeciesData.speciesPrefab;

            if (RestaurantManager.instance != null && prefabToSpawn != null)
            {
                RestaurantManager.instance.SpawnSingleWorker(newEmployee, prefabToSpawn);
            }
            else
            {
                Debug.LogError($"[EmployeeManager] 스폰 실패: RestaurantManager 또는 Prefab이 없습니다.");
            }

            // 4. UI 갱신
            if (EmployeeUI_Controller.Instance != null)
            {
                EmployeeUI_Controller.Instance.UpdateApplicantListUI(applicants);
                EmployeeUI_Controller.Instance.UpdateHiredEmployeeListUI();
            }

            // 5. 퀘스트 갱신
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.SetProgress(QuestTargetType.Collect, "고용한 직원 수", hiredEmployees.Count);
            }
        }
    }

    public void DismissEmployee(EmployeeInstance employeeToDismiss)
    {
        if (hiredEmployees.Contains(employeeToDismiss))
        {
            hiredEmployees.Remove(employeeToDismiss);
            Debug.Log($"{employeeToDismiss.firstName} 해고 완료.");

            if (QuestManager.Instance != null)
            {
                int hiredCount = hiredEmployees.Count(e => !e.isProtagonist);
                QuestManager.Instance.SetProgress(QuestTargetType.Collect, "고용한 직원 수", hiredCount);
            }
        }
    }
}
