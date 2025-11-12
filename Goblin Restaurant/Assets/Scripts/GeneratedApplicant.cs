using System.Collections.Generic;

/// <summary>
/// EmployeeManager가 생성한 '지원자'의 임시 데이터를 보관하는 클래스입니다.
/// </summary>
public class GeneratedApplicant
{
    public EmployeeData BaseSpeciesData { get; private set; }
    public string GeneratedFirstName { get; private set; }
    public string GeneratedJobTitle { get; private set; }

    public int GeneratedCookingStat { get; private set; }
    public int GeneratedServingStat { get; private set; }

    public int GeneratedCharmStat { get; private set; }

    public List<Trait> GeneratedTraits { get; private set; }

    public GeneratedApplicant(EmployeeData selectedSpecies, string firstName, string jobTitle,
                               int cook, int serve, int charm, List<Trait> finalTraits)
    {
        BaseSpeciesData = selectedSpecies;
        GeneratedFirstName = firstName;
        GeneratedJobTitle = jobTitle;

        GeneratedCookingStat = cook;
        GeneratedServingStat = serve;
        GeneratedCharmStat = charm;

        GeneratedTraits = finalTraits;
    }
}