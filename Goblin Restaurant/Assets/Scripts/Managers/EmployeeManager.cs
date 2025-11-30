using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EmployeeManager : MonoBehaviour
{
    public static EmployeeManager Instance { get; private set; }

    [Header("직원 종족 템플릿")]
    public List<EmployeeData> allSpeciesTemplates;

    [Header("실시간 데이터")]
    public List<EmployeeInstance> hiredEmployees = new List<EmployeeInstance>();
    public List<GeneratedApplicant> applicants = new List<GeneratedApplicant>();

    private const int MAX_TOTAL_EMPLOYEES = 10;
    private const int MAX_APPLICANTS = 10;

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

        List<EmployeeData> validTemplates = allSpeciesTemplates
            .Where(t => t != null)
            .ToList();

        if (!validTemplates.Any())
        {
            Debug.LogError("GenerateApplicants 오류: 유효한 EmployeeData 템플릿이 EmployeeManager에 설정되어 있지 않습니다!");
            return;
        }

        int minApplicantsRaw = 3;
        int maxApplicantsRaw = 5;
        int finalMaxLimit = Mathf.Min(maxApplicantsRaw, MAX_APPLICANTS);
        int finalMinLimit = Mathf.Min(minApplicantsRaw, finalMaxLimit);
        int applicantCount = Random.Range(finalMinLimit, finalMaxLimit + 1);

        Debug.Log($"[지원자 수 생성 확인] 명성: {currentFame}, 생성된 지원자 수: {applicantCount}");

        for (int i = 0; i < applicantCount; i++)
        {
            EmployeeData selectedSpecies = validTemplates[Random.Range(0, validTemplates.Count)];
            float fameMultiplier = (float)currentFame / 100f;
            int finalCook = Random.Range(selectedSpecies.baseCookingStat + (int)(fameMultiplier * selectedSpecies.cookingGrowthFactor * 0.8f),
                                         selectedSpecies.baseCookingStat + (int)(fameMultiplier * selectedSpecies.cookingGrowthFactor * 1.2f) + 1);
            int finalServe = Random.Range(selectedSpecies.baseServingStat + (int)(fameMultiplier * selectedSpecies.servingGrowthFactor * 0.8f),
                                          selectedSpecies.baseServingStat + (int)(fameMultiplier * selectedSpecies.servingGrowthFactor * 1.2f) + 1);
            int finalCharm = Random.Range(selectedSpecies.baseCharmStat + (int)(fameMultiplier * selectedSpecies.charmGrowthFactor * 0.8f),
                                          selectedSpecies.baseCharmStat + (int)(fameMultiplier * selectedSpecies.charmGrowthFactor * 1.2f) + 1);

            string jobTitle = "웨이터";
            if (finalCook >= finalServe && finalCook >= finalCharm) { jobTitle = "요리사"; }
            else if (finalServe > finalCook && finalServe >= finalCharm) { jobTitle = "서빙"; }
            else { jobTitle = "매니저"; }

            string firstName = selectedSpecies.speciesName;
            if (selectedSpecies.possibleFirstNames != null && selectedSpecies.possibleFirstNames.Any())
            {
                firstName = selectedSpecies.possibleFirstNames[Random.Range(0, selectedSpecies.possibleFirstNames.Count)];
            }

            EmployeeGrade finalGrade = DetermineGrade(currentFame);
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

            GeneratedApplicant newApplicant = new GeneratedApplicant(selectedSpecies, firstName, jobTitle, finalCook, finalServe, finalCharm, finalTraits, finalGrade);
            applicants.Add(newApplicant);
        }

        if (EmployeeUI_Controller.Instance != null &&
            EmployeeUI_Controller.Instance.employeeSubMenuPanel != null &&
            EmployeeUI_Controller.Instance.employeeSubMenuPanel.activeSelf)
        {
            EmployeeUI_Controller.Instance.UpdateApplicantListUI(applicants);
        }
    }

    private EmployeeGrade DetermineGrade(int currentFame)
    {
        float fameRatio = Mathf.Clamp01((float)currentFame / 400f);
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
            Debug.LogWarning($"최대 고용 인원({MAX_TOTAL_EMPLOYEES}명)에 도달하여 더 이상 고용할 수 없습니다.");
            return;
        }

        if (applicants.Contains(applicantToHire))
        {
            int hiringCost = applicantToHire.BaseSpeciesData.salary;
            EmployeeInstance newEmployee = new EmployeeInstance(applicantToHire);
            hiredEmployees.Add(newEmployee);
            applicants.Remove(applicantToHire);
            Debug.Log($"{newEmployee.BaseData.speciesName} {newEmployee.firstName}님을 {hiringCost}골드에 고용했습니다.");

            GameObject prefabToSpawn = applicantToHire.BaseSpeciesData.speciesPrefab;
            if (RestaurantManager.instance != null && prefabToSpawn != null)
            {
                RestaurantManager.instance.SpawnSingleWorker(newEmployee, prefabToSpawn);
            }
            else
            {
                Debug.LogError($"[EmployeeManager] 직원 생성 실패! RestaurantManager.instance가 없거나 {newEmployee.firstName}의 Prefab ({newEmployee.BaseData.speciesName})이 null입니다.");
            }

            if (EmployeeUI_Controller.Instance != null)
            {
                EmployeeUI_Controller.Instance.UpdateApplicantListUI(applicants);
                EmployeeUI_Controller.Instance.UpdateHiredEmployeeListUI();
            }
        }
    }

    public void DismissEmployee(EmployeeInstance employeeToDismiss)
    {
        if (hiredEmployees.Contains(employeeToDismiss))
        {
            hiredEmployees.Remove(employeeToDismiss);
            Debug.Log($"{employeeToDismiss.firstName} 님({employeeToDismiss.BaseData.speciesName})을 해고했습니다.");

            if (EmployeeUI_Controller.Instance != null)
            {
                EmployeeUI_Controller.Instance.UpdateHiredEmployeeListUI();
            }
        }
    }
}