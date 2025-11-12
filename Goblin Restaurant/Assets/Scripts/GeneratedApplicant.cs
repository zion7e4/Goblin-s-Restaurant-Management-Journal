using System.Collections.Generic;

// 역할: 모든 계산이 끝난 지원자의 최종 정보를 담는 '결과물' 클래스
public class GeneratedApplicant
{
    // 어떤 '종족' 템플릿에서 생성되었는지 저장합니다.
    public EmployeeData BaseSpeciesData { get; private set; }

    // 동적으로 생성된 정보들
    public string GeneratedFirstName { get; private set; }
    public string GeneratedJobTitle { get; private set; }

    // 랜덤으로 생성된 능력치 및 특성
    public int GeneratedCookingStat { get; private set; }
    public int GeneratedServingStat { get; private set; }
    public int GeneratedCleaningStat { get; private set; }
    public List<Trait> GeneratedTraits { get; private set; }

    // 생성자: EmployeeManager가 모든 계산을 마친 후 결과값을 전달받습니다.
    public GeneratedApplicant(EmployeeData speciesData, string firstName, string jobTitle, int cook, int serve, int clean, List<Trait> traits)
    {
        BaseSpeciesData = speciesData;
        GeneratedFirstName = firstName;
        GeneratedJobTitle = jobTitle;
        GeneratedCookingStat = cook;
        GeneratedServingStat = serve;
        GeneratedCleaningStat = clean;
        GeneratedTraits = traits;
    }
}

