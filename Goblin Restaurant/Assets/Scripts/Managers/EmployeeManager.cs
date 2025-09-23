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
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }
    }

    public void GenerateApplicants(int currentFame)
    {
        // [추적 로그 1] 이 함수가 시작되었는지 확인
        Debug.Log("--- GenerateApplicants 함수 시작 ---");
        applicants.Clear();

        int minApplicants = 1 + (currentFame / 1500);
        int maxApplicants = 2 + (currentFame / 1000);
        int applicantCount = Random.Range(minApplicants, Mathf.Min(maxApplicants, 10) + 1);

        if (!allSpeciesTemplates.Any())
        {
            // [추적 로그] 만약 종족 템플릿이 없으면 알려줌
            Debug.LogWarning("EmployeeManager의 AllSpeciesTemplates 리스트가 비어있어 지원자를 생성할 수 없습니다!");
            return;
        }

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

        Debug.Log($"[명성: {currentFame}] 오늘 도착한 지원자 수: {applicants.Count}명");

        if (UIManager.Instance != null)
        {
            // [추적 로그 2] UIManager에게 신호를 보내는지 확인
            Debug.Log("UIManager에게 UI 업데이트를 요청합니다.");
            UIManager.Instance.UpdateApplicantListUI(applicants);
        }
    }

    public void HireEmployee(GeneratedApplicant applicantToHire)
    {
        // [추적 로그 3] 고용 버튼이 제대로 눌렸는지 확인
        Debug.Log($"--- HireEmployee 함수 시작: {applicantToHire.GeneratedFirstName} 고용 시도 ---");
        if (applicants.Contains(applicantToHire))
        {
            EmployeeInstance newEmployee = new EmployeeInstance(applicantToHire);
            hiredEmployees.Add(newEmployee);
            applicants.Remove(applicantToHire);

            Debug.Log($"{newEmployee.BaseData.speciesName} {newEmployee.firstName}(을)를 성공적으로 고용했습니다! 남은 지원자 수: {applicants.Count}명");

            if (UIManager.Instance != null)
            {
                // [추적 로그 4] 고용 후 UI 업데이트를 다시 요청하는지 확인
                Debug.Log("고용이 완료되어 UIManager에게 UI 업데이트를 다시 요청합니다.");
                UIManager.Instance.UpdateApplicantListUI(applicants);
            }
        }
    }
}
