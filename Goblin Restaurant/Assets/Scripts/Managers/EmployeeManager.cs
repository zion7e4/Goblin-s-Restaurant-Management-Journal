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
        int minApplicants = 1 + (currentFame / 1500);
        int maxApplicants = 2 + (currentFame / 1000);
        int applicantCount = Random.Range(minApplicants, Mathf.Min(maxApplicants, 10) + 1);
        if (!allSpeciesTemplates.Any()) return;

        for (int i = 0; i < applicantCount; i++)
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
}