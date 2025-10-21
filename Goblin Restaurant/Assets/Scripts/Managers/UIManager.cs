using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

/// <summary>
/// 게임의 모든 UI 요소를 관리하고, 다른 매니저로부터 받은 데이터를 화면에 표시하는 중앙 관리자입니다.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("메인 UI 및 서브 패널")]
    public GameObject managementUIParent;
    public GameObject applicantListPanel;
    public GameObject manageEmployeePanel;
    public GameObject recipeBookPanel;
    public GameObject recruitmentPanel;
    public GameObject storePanel;
    public GameObject interiorPanel;

    [Header("탭 UI 요소")]
    public Button Button_OpenHirePanel;
    public Button Button_OpenManagePanel;
    public Button recipeTabButton;

    [Header("카드 프리팹 및 위치")]
    public GameObject applicantCardPrefab;
    public GameObject hiredCardPrefab;
    public Transform applicantCardParent;
    public Transform hiredCardParent;

    [Header("탭 시각 효과")]
    public Color normalTabColor = Color.white;
    public Color activeTabColor = new Color(0.8f, 0.9f, 1f);

    [Header("디버그 테스트용 버튼")]
    public Button Button_DebugLevelUp; // Unity Inspector에서 연결할 임시 버튼

    [Header("해고 확인 팝업")]
    public GameObject dismissalConfirmationPanel; // 확인 팝업 패널 자체 (인스펙터 연결 필수)
    public TextMeshProUGUI dismissalNameText;      // 해고될 직원 이름을 표시할 텍스트 컴포넌트 (인스펙터 연결 필수)
    public Button Button_ConfirmDismiss;           // '네' 버튼 (해고 실행) (인스펙터 연결 필수)
    public Button Button_CancelDismiss;            // '아니오' 버튼 (해고 취소) (인스펙터 연결 필수)

    private EmployeeInstance employeeToDismiss; // 팝업에서 최종 해고할 직원을 임시 저장할 변수

    private List<GameObject> spawnedApplicantCards = new List<GameObject>();
    private List<GameObject> spawnedHiredCards = new List<GameObject>();

    // UI의 현재 활성화 상태
    private bool isUIVisible = false;

    void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }
    }

    void Start()
    {
        // 각 탭 버튼에 OpenTab 기능 연결
        if (Button_OpenHirePanel != null) Button_OpenHirePanel.onClick.AddListener(() => OpenTab(applicantListPanel, Button_OpenHirePanel));
        if (Button_OpenManagePanel != null) Button_OpenManagePanel.onClick.AddListener(() => OpenTab(manageEmployeePanel, Button_OpenManagePanel));
        if (recipeTabButton != null) recipeTabButton.onClick.AddListener(() => OpenTab(recipeBookPanel, recipeTabButton));

        // 디버그 버튼 기능 연결
        if (Button_DebugLevelUp != null) Button_DebugLevelUp.onClick.AddListener(Debug_LevelUpAllEmployees);

        // 해고 확인 팝업 버튼 이벤트 연결 (Start에서 딱 한 번 연결)
        if (Button_ConfirmDismiss != null) Button_ConfirmDismiss.onClick.AddListener(ConfirmDismissal);
        if (Button_CancelDismiss != null) Button_CancelDismiss.onClick.AddListener(HideDismissalConfirmation);
        if (dismissalConfirmationPanel != null) dismissalConfirmationPanel.SetActive(false); // 시작 시 팝업 닫기

        // 게임 시작 시 UI 끄기
        isUIVisible = false;
        if (managementUIParent != null) managementUIParent.SetActive(isUIVisible);
    }

    /// <summary>
    /// UI가 켜져 있을 때 'Escape' 키를 누르면 UI가 닫히도록 처리합니다.
    /// </summary>
    void Update()
    {
        if (isUIVisible && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // 메인 UI가 켜져 있을 때 ESC 키를 누르면 전체 UI를 닫습니다.
            isUIVisible = false;
            if (managementUIParent != null) managementUIParent.SetActive(isUIVisible);
        }
    }

    /// <summary>
    /// 특정 패널(탭)을 열거나, 이미 열린 탭을 다시 누르면 메인 UI 전체를 닫습니다.
    /// </summary>
    void OpenTab(GameObject panelToShow, Button clickedButton)
    {
        // 1. 닫기 로직: UI가 켜져있고, 이미 활성화된 탭을 다시 눌렀는지 확인
        if (isUIVisible && panelToShow != null && panelToShow.activeSelf)
        {
            // 이미 열린 탭을 다시 눌렀으므로, 메인 UI 전체를 닫습니다.
            isUIVisible = false;
            if (managementUIParent != null) managementUIParent.SetActive(false);
            return;
        }

        // 2. 열기 또는 탭 전환 로직
        // 2a. 메인 UI가 닫혀있었다면 켭니다.
        if (!isUIVisible)
        {
            isUIVisible = true;
            if (managementUIParent != null) managementUIParent.SetActive(true);
        }

        // 2b. 모든 컨텐츠 패널을 끕니다. (탭 전환을 위해)
        if (applicantListPanel != null) applicantListPanel.SetActive(false);
        if (manageEmployeePanel != null) manageEmployeePanel.SetActive(false);
        if (recipeBookPanel != null) recipeBookPanel.SetActive(false);

        // 2c. 요청된 컨텐츠 패널만 켭니다.
        if (panelToShow != null) panelToShow.SetActive(true);

        // 2d. 모든 탭 버튼 색상을 '일반'으로 초기화하고, 클릭된 탭 버튼만 '활성' 색상으로 변경합니다.
        Image[] tabImages = new Image[]
        {
            Button_OpenHirePanel?.GetComponent<Image>(),
            Button_OpenManagePanel?.GetComponent<Image>(),
            recipeTabButton?.GetComponent<Image>()
        };

        foreach (Image img in tabImages)
        {
            if (img != null) img.color = normalTabColor;
        }

        Image clickedBtnImage = clickedButton?.GetComponent<Image>();
        if (clickedBtnImage != null) clickedBtnImage.color = activeTabColor;

        // 2e. '직원 관리' 탭을 열었다면 목록을 새로고침합니다.
        if (panelToShow == manageEmployeePanel)
        {
            UpdateHiredEmployeeListUI();
        }
    }

    // --- 새로 추가된 패널 닫기 함수 ---

    /// <summary>
    /// 서브 메뉴 패널(채용, 상점, 인테리어 등)을 닫고 메인 화면으로 돌아갑니다.
    /// 이 함수를 모든 'Button_...out' 버튼에 연결합니다.
    /// </summary>
    /// <param name="panelToClose">비활성화할 GameObject 패널</param>
    public void CloseUIPanel(GameObject panelToClose)
    {
        if (panelToClose != null)
        {
            panelToClose.SetActive(false);
        }
    }

    // --- 디버그 테스트 함수 (전체 직원 레벨업) ---

    /// <summary>
    /// [디버그용] 고용된 '모든 직원'의 레벨을 올리고 스킬 포인트를 부여합니다.
    /// </summary>
    public void Debug_LevelUpAllEmployees()
    {
        if (EmployeeManager.Instance == null)
        {
            Debug.LogError("EmployeeManager.Instance가 존재하지 않습니다.");
            return;
        }

        if (EmployeeManager.Instance.hiredEmployees.Count > 0)
        {
            // 리스트의 모든 직원에게 적용되도록 반복문(foreach) 사용
            foreach (EmployeeInstance employee in EmployeeManager.Instance.hiredEmployees)
            {
                employee.currentLevel += 1;
                employee.skillPoints += 5; // 예시로 스킬 포인트 5 부여

                Debug.Log($"{employee.firstName}의 레벨이 {employee.currentLevel}로 증가했고, 스킬 포인트 5점을 얻었습니다.");
            }

            // 모든 직원 업데이트 후 UI를 한 번 새로고침합니다.
            UpdateHiredEmployeeListUI();
        }
        else
        {
            Debug.LogWarning("현재 고용된 직원이 없습니다. 먼저 직원을 고용하세요.");
        }
    }

    // --- 해고 확인 팝업 로직 ---

    /// <summary>
    /// 직원 해고 전에 확인 팝업을 띄우고, 해고할 직원을 임시 저장합니다. (해고 버튼 OnClick에 연결)
    /// </summary>
    /// <param name="employee">해고할 직원 인스턴스</param>
    public void ShowDismissalConfirmation(EmployeeInstance employee)
    {
        if (dismissalConfirmationPanel != null && employee != null)
        {
            employeeToDismiss = employee; // 최종 해고 시 사용할 직원을 임시 저장

            // 텍스트 업데이트: "OOO을(를) 정말로 해고하시겠습니까?"
            if (dismissalNameText != null)
            {
                dismissalNameText.text = $"'{employee.firstName}'을(를) 정말로 해고 하시겠습니까?";
            }

            // 팝업 패널 활성화
            dismissalConfirmationPanel.SetActive(true);
        }
    }

    /// <summary>
    /// '네' 버튼에 연결: 확인 후 실제 해고를 실행합니다.
    /// </summary>
    public void ConfirmDismissal()
    {
        if (employeeToDismiss != null && EmployeeManager.Instance != null)
        {
            // EmployeeManager에 해고 요청
            EmployeeManager.Instance.DismissEmployee(employeeToDismiss);

            employeeToDismiss = null; // 임시 저장 변수 초기화
        }

        // 팝업 닫기
        HideDismissalConfirmation();
    }

    /// <summary>
    /// '아니오' 버튼에 연결: 해고를 취소하고 팝업을 닫습니다.
    /// </summary>
    public void HideDismissalConfirmation()
    {
        if (dismissalConfirmationPanel != null)
        {
            dismissalConfirmationPanel.SetActive(false);
        }
        employeeToDismiss = null; // 임시 저장 변수 초기화
    }

    // --- 이하 카드 업데이트 함수들은 동일하게 유지됩니다 ---

    /// <summary>
    /// 지원자 목록 UI를 최신 정보로 새로고침합니다.
    /// </summary>
    public void UpdateApplicantListUI(List<GeneratedApplicant> applicants)
    {
        foreach (GameObject card in spawnedApplicantCards) { Destroy(card); }
        spawnedApplicantCards.Clear();
        foreach (GeneratedApplicant applicant in applicants)
        {
            GameObject newCard = Instantiate(applicantCardPrefab, applicantCardParent);
            UpdateApplicantCardUI(newCard, applicant);
            spawnedApplicantCards.Add(newCard);
        }
    }

    /// <summary>
    /// 고용된 직원 목록 UI를 최신 정보로 새로고침합니다.
    /// </summary>
    public void UpdateHiredEmployeeListUI()
    {
        foreach (GameObject card in spawnedHiredCards) { Destroy(card); }
        spawnedHiredCards.Clear();
        // EmployeeManager.Instance가 존재한다고 가정
        if (EmployeeManager.Instance != null)
        {
            foreach (EmployeeInstance employee in EmployeeManager.Instance.hiredEmployees)
            {
                if (employee == null) continue;
                GameObject newCard = Instantiate(hiredCardPrefab, hiredCardParent);
                UpdateHiredCardUI(newCard, employee);
                spawnedHiredCards.Add(newCard);
            }
        }
    }

    /// <summary>
    /// '지원자' 카드(ApplicantSlot) 한 개의 내용을 채웁니다.
    /// </summary>
    private void UpdateApplicantCardUI(GameObject card, GeneratedApplicant applicant)
    {
        Image portraitImage = card.transform.Find("PortraitImage")?.GetComponent<Image>();
        TextMeshProUGUI nameText = card.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI statsText = card.transform.Find("StatsText")?.GetComponent<TextMeshProUGUI>();
        Button hireButton = card.transform.Find("HireButton")?.GetComponent<Button>();

        if (portraitImage != null) portraitImage.sprite = applicant.BaseSpeciesData.portrait;
        if (nameText != null)
        {
            nameText.text = $"{applicant.GeneratedFirstName}\n<size=20>({applicant.BaseSpeciesData.speciesName})</size>";
        }
        if (statsText != null)
        {
            var statsBuilder = new System.Text.StringBuilder();
            statsBuilder.AppendLine($"요리: {applicant.GeneratedCookingStat}");
            statsBuilder.AppendLine($"서빙: {applicant.GeneratedServingStat}");
            statsBuilder.AppendLine($"정리: {applicant.GeneratedCleaningStat}");
            if (applicant.GeneratedTraits.Any())
            {
                statsBuilder.AppendLine($"\n특성: <color=yellow>{applicant.GeneratedTraits[0].traitName}</color>");
            }
            statsText.text = statsBuilder.ToString();
        }
        if (hireButton != null)
        {
            hireButton.onClick.RemoveAllListeners();
            // EmployeeManager.Instance가 존재한다고 가정
            if (EmployeeManager.Instance != null)
            {
                hireButton.onClick.AddListener(() => EmployeeManager.Instance.HireEmployee(applicant));
            }
        }
    }

    /// <summary>
    /// '고용된 직원' 카드(HiredEmployeeCard) 한 개의 내용을 채우고 버튼 기능을 연결합니다.
    /// </summary>
    private void UpdateHiredCardUI(GameObject card, EmployeeInstance employee)
    {
        Image portraitImage = card.transform.Find("PortraitImage")?.GetComponent<Image>();
        TextMeshProUGUI nameText = card.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI statsText = card.transform.Find("StatsText")?.GetComponent<TextMeshProUGUI>();
        Button cookUpBtn = card.transform.Find("CookUpgradeButton")?.GetComponent<Button>();
        Button serveUpBtn = card.transform.Find("ServeUpgradeButton")?.GetComponent<Button>();
        Button cleanUpBtn = card.transform.Find("CleanUpgradeButton")?.GetComponent<Button>();

        // 해고 버튼 가져오기
        Button dismissBtn = card.transform.Find("DismissButton")?.GetComponent<Button>();

        if (portraitImage != null) portraitImage.sprite = employee.BaseData.portrait;
        if (nameText != null)
        {
            nameText.text = $"{employee.firstName}\n<size=24>[Lv. {employee.currentLevel}]<color=yellow>({employee.skillPoints})</color></size>\n<size=20>({employee.BaseData.speciesName})</size>";
        }
        if (statsText != null)
        {
            var statsBuilder = new System.Text.StringBuilder();
            statsBuilder.AppendLine($"요리: {employee.currentCookingStat}");
            statsBuilder.AppendLine($"서빙: {employee.currentServingStat}");
            statsBuilder.AppendLine($"정리: {employee.currentCleaningStat}");
            if (employee.currentTraits.Any())
            {
                statsBuilder.AppendLine($"\n특성: <color=yellow>{employee.currentTraits[0].traitName}</color>");
            }
            statsText.text = statsBuilder.ToString();
            statsText.lineSpacing = 5f;
        }

        // 스킬 업그레이드 및 해고 버튼 기능 연결 (EmployeeManager.Instance가 존재한다고 가정)
        if (EmployeeManager.Instance != null)
        {
            // 1. 요리 버튼 설정
            if (cookUpBtn != null)
            {
                cookUpBtn.interactable = employee.skillPoints > 0;
                cookUpBtn.onClick.RemoveAllListeners();
                cookUpBtn.onClick.AddListener(() => { if (employee.SpendSkillPointOnCooking()) UpdateHiredEmployeeListUI(); });
            }

            // 2. 서빙 버튼 설정
            if (serveUpBtn != null)
            {
                serveUpBtn.interactable = employee.skillPoints > 0;
                serveUpBtn.onClick.RemoveAllListeners();
                serveUpBtn.onClick.AddListener(() => { if (employee.SpendSkillPointOnServing()) UpdateHiredEmployeeListUI(); });
            }

            // 3. 정리 버튼 설정 (디버그 코드 포함)
            if (cleanUpBtn == null)
            {
                Debug.LogError($"ERROR! CleanUpgradeButton (정리 버튼)을 찾지 못했습니다! 프리팹 '{card.name}' 내부의 이름 철자를 확인하세요.", card);
            }
            else
            {
                bool isInteractable = employee.skillPoints > 0;
                cleanUpBtn.interactable = isInteractable;

                Debug.Log($"정리 버튼 ({cleanUpBtn.gameObject.name}) | 스킬 포인트: {employee.skillPoints} | Interactable 설정 상태: {isInteractable}");

                cleanUpBtn.onClick.RemoveAllListeners();
                cleanUpBtn.onClick.AddListener(() => { if (employee.SpendSkillPointOnCleaning()) UpdateHiredEmployeeListUI(); });
            }

            // 4. 해고 버튼 기능 연결 (★★★ 확인 창 및 주인공 해고 불가 로직 적용 - 수정됨 ★★★)
            if (dismissBtn != null)
            {
                dismissBtn.onClick.RemoveAllListeners();

                // 주인공 식별 플래그와 이름
                bool isProtagonistFlag = employee.isProtagonist;
                string employeeName = employee.firstName;

                // ★★★ [강력한 디버그] 현재 직원의 상태를 로그에 출력합니다. ★★★
                Debug.Log($"[해고 확인] 직원 이름: {employeeName}, isProtagonist 플래그: {isProtagonistFlag}, BaseData.speciesName: {employee.BaseData.speciesName}");

                // 1. isProtagonist 플래그가 true인 경우 (가장 확실한 방법)
                if (isProtagonistFlag)
                {
                    dismissBtn.interactable = false;
                    Debug.LogError($"주인공 '{employeeName}'({employee.BaseData.speciesName})입니다. isProtagonist=TRUE이므로 해고 버튼 비활성화.");
                }
                // 2. isProtagonist 플래그가 false라도 이름이 "Goblin Chef"인 경우 (비상 방어 로직)
                else if (employeeName.Equals("Goblin Chef", System.StringComparison.OrdinalIgnoreCase)) // 대소문자 구분 없이 "Goblin Chef" 확인
                {
                    dismissBtn.interactable = false;
                    Debug.LogError($"[비상 방어] isProtagonist는 FALSE이지만 이름이 '{employeeName}'입니다. 해고 버튼을 강제로 비활성화합니다.");
                }
                else
                {
                    // 일반 직원
                    dismissBtn.interactable = true;
                    // 확인 창을 띄우는 함수에 이 직원 인스턴스를 전달합니다.
                    dismissBtn.onClick.AddListener(() => ShowDismissalConfirmation(employee));
                }
            }
        }
    }
}