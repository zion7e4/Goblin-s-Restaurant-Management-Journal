using UnityEngine;
using System.Collections; // 코루틴을 사용하기 위해 필요합니다.

// 역할: 게임의 시간, 명성, 돈 등 전반적인 상태를 관리하고 다른 매니저들에게 신호를 보냅니다.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("게임 시간 설정")]
    [Tooltip("게임 속 하루가 현실 시간으로 몇 초인지 설정합니다.")]
    public float secondsPerDay = 5f;

    [Header("게임 상태 변수")]
    [Tooltip("현재 게임의 날짜")]
    public int currentDay = 1;
    [Tooltip("현재 식당의 명성도. 이 값을 바꿔서 테스트할 수 있습니다.")]
    public int currentFame = 100;

    [Header("주인공 설정")]
    [Tooltip("주인공으로 사용할 직원의 설계도(EmployeeData 에셋)를 연결해주세요.")]
    public EmployeeData mainCharacterTemplate;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        CreateMainCharacter();
        StartCoroutine(DayCycleCoroutine());
    }

    // 주인공 캐릭터를 생성해서 고용 목록에 추가하는 함수
    void CreateMainCharacter()
    {
        if (mainCharacterTemplate != null)
        {
            EmployeeInstance mainCharacter = new EmployeeInstance(mainCharacterTemplate);
            EmployeeManager.Instance.hiredEmployees.Add(mainCharacter);

            // [수정된 부분] employeeName -> firstName
            // EmployeeInstance에 저장된 주인공의 이름(firstName)을 사용합니다.
            Debug.Log($"주인공 '{mainCharacter.firstName}'이(가) 식당에 합류했습니다!");
        }
    }

    // 설정된 시간마다 하루를 진행시키는 코루틴
    IEnumerator DayCycleCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(secondsPerDay);
            currentDay++;
            Debug.Log($"{currentDay}일차 아침이 되었습니다.");

            if ((currentDay - 1) % 7 == 0 && currentDay > 1)
            {
                EmployeeManager.Instance.GenerateApplicants(currentFame);
            }
        }
    }
}