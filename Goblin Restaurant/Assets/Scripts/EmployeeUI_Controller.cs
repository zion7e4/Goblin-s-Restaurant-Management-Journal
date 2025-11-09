using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// '직원 관리' 관련 모든 UI(패널, 탭, 카드, 팝업)를 제어하는 중앙 매니저입니다.
/// (이 스크립트는 'Managers' 같은 중앙 오브젝트에 붙입니다.)
/// </summary>
public class EmployeeUI_Controller : MonoBehaviour
{
    public static EmployeeUI_Controller Instance { get; private set; }

    [Header("메인 패널 (필수 연결)")]
    [Tooltip("이 스크립트가 켜고 끌 직원 서브 메뉴 패널")]
    public GameObject employeeSubMenuPanel; // 이 스크립트가 제어할 메인 패널

    [Header("핵심 컨텐츠 패널 (필수)")]
    [Tooltip("서브 메뉴 안의 탭별 컨텐츠")]
    public GameObject applicantListPanel;  // '지원자' 탭의 컨텐츠
    public GameObject manageEmployeePanel; // '직원 관리' 탭의 컨텐츠

    [Header("탭 UI 요소 (필수)")]
    [Tooltip("'직원 서브 메뉴' 패널 내부에 있는 버튼들")]
    public Button Button_OpenHirePanel;
    public Button Button_OpenManagePanel;

    [Header("카드 프리팹 및 위치 (필수)")]
    public GameObject applicantCardPrefab;
    public GameObject hiredCardPrefab;
    public Transform applicantCardParent;
    public Transform hiredCardParent;

    [Header("탭 시각 효과")]
    public Color normalTabColor = Color.white;
    public Color activeTabColor = new Color(0.8f, 0.9f, 1f);

    [Header("디버그 테스트용 버튼")]
    public Button Button_DebugLevelUp;

    [Header("해고 확인 팝업 (필수)")]
    public GameObject dismissalConfirmationPanel;
    public TextMeshProUGUI dismissalNameText;
    public Button Button_ConfirmDismiss;
    public Button Button_CancelDismiss;

    // --- 내부 변수 ---
    private EmployeeInstance employeeToDismiss;
    private List<GameObject> spawnedApplicantCards = new List<GameObject>();
    private List<GameObject> spawnedHiredCards = new List<GameObject>();

    void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }
    }

    void Start()
    {
        // Start에서는 버튼 리스너를 한 번만 등록합니다.
        if (Button_OpenHirePanel != null) Button_OpenHirePanel.onClick.AddListener(() => OpenTab(applicantListPanel, Button_OpenHirePanel));
        if (Button_OpenManagePanel != null) Button_OpenManagePanel.onClick.AddListener(() => OpenTab(manageEmployeePanel, Button_OpenManagePanel));
        if (Button_DebugLevelUp != null) Button_DebugLevelUp.onClick.AddListener(Debug_LevelUpAllEmployees);
        if (Button_ConfirmDismiss != null) Button_ConfirmDismiss.onClick.AddListener(ConfirmDismissal);
        if (Button_CancelDismiss != null) Button_CancelDismiss.onClick.AddListener(HideDismissalConfirmation);

        // 게임 시작 시 모든 관련 UI를 확실히 끕니다.
        if (dismissalConfirmationPanel != null) dismissalConfirmationPanel.SetActive(false);
        if (employeeSubMenuPanel != null) employeeSubMenuPanel.SetActive(false);
        if (applicantListPanel != null) applicantListPanel.SetActive(false);
        if (manageEmployeePanel != null) manageEmployeePanel.SetActive(false);
    }

    // ESC 키 닫기 기능을 위한 Update 함수 추가
    void Update()
    {
        // 메인 패널이 켜져 있고, ESC 키가 눌렸을 때만 작동
        if (employeeSubMenuPanel != null && employeeSubMenuPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            // 해고 확인 팝업이 켜져 있으면 팝업만 닫고 종료
            if (dismissalConfirmationPanel != null && dismissalConfirmationPanel.activeSelf)
            {
                HideDismissalConfirmation();
                return;
            }

            // 팝업이 닫혀 있으면 메인 패널 닫기
            ClosePanel();
        }
    }

    /// <summary>
    /// GameManager가 이 함수를 호출해서 '직원 서브 메뉴' 패널을 웁니다.
    /// </summary>
    public void OpenPanel()
    {
        Debug.Log("1. OpenPanel() 호출됨 (GameManager가 부름)");

        if (employeeSubMenuPanel == null)
        {
            Debug.LogError("OpenPanel 오류: 'employeeSubMenuPanel' 변수가 null입니다! 인스펙터를 확인하세요!");
            return;
        }

        // 메인 패널 켜기
        employeeSubMenuPanel.SetActive(true);

        // 하위 컨텐츠 패널 초기 상태 설정 (안전성 보장)
        if (applicantListPanel != null) applicantListPanel.SetActive(false);
        if (manageEmployeePanel != null) manageEmployeePanel.SetActive(false);

        // 기본 탭 (지원자 목록) 열기 시도
        if (applicantListPanel != null && Button_OpenHirePanel != null)
        {
            OpenTab(applicantListPanel, Button_OpenHirePanel);
        }
    }

    /// <summary>
    /// '뒤로가기' 또는 ESC 키가 이 함수를 호출해서 '직원 서브 메뉴' 패널을 끕니다.
    /// </summary>
    public void ClosePanel()
    {
        if (employeeSubMenuPanel != null)
        {
            employeeSubMenuPanel.SetActive(false);
        }

        // 하위 컨텐츠 패널도 명시적으로 모두 닫아 상태를 초기화합니다.
        if (applicantListPanel != null) applicantListPanel.SetActive(false);
        if (manageEmployeePanel != null) manageEmployeePanel.SetActive(false);

        // 팝업도 혹시 모를 경우를 대비해 닫습니다.
        if (dismissalConfirmationPanel != null) dismissalConfirmationPanel.SetActive(false);
    }

    /// <summary>
    /// 패널 *내부*의 탭(컨텐츠)을 전환합니다.
    /// </summary>
    void OpenTab(GameObject panelToShow, Button clickedButton)
    {
        // 탭 전환 시작: 모든 컨텐츠 패널을 끕니다.
        if (applicantListPanel != null) applicantListPanel.SetActive(false);
        if (manageEmployeePanel != null) manageEmployeePanel.SetActive(false);

        if (panelToShow != null)
        {
            panelToShow.SetActive(true);
        }

        // 탭 색상 변경
        Image[] tabImages = new Image[] { Button_OpenHirePanel?.GetComponent<Image>(), Button_OpenManagePanel?.GetComponent<Image>() };
        foreach (Image img in tabImages) { if (img != null) img.color = normalTabColor; }
        Image clickedBtnImage = clickedButton?.GetComponent<Image>();
        if (clickedBtnImage != null) clickedBtnImage.color = activeTabColor;

        // 탭에 따라 목록 갱신 (지원자를 생성하지 않고, 저장된 목록을 갱신합니다.)
        if (panelToShow == applicantListPanel)
        {
            if (EmployeeManager.Instance != null)
            {
                // 저장된 지원자 목록을 가져와서 UI를 그립니다.
                UpdateApplicantListUI(EmployeeManager.Instance.applicants);
            }
        }
        else if (panelToShow == manageEmployeePanel)
        {
            // 고용된 직원 목록을 갱신합니다.
            UpdateHiredEmployeeListUI();
        }
    }

    // --- (이하 원본 기능들) ---

    public void Debug_LevelUpAllEmployees()
    {
        if (EmployeeManager.Instance == null)
        {
            Debug.LogError("EmployeeManager.Instance가 존재하지 않습니다.");
            return;
        }

        if (EmployeeManager.Instance.hiredEmployees.Count > 0)
        {
            foreach (EmployeeInstance employee in EmployeeManager.Instance.hiredEmployees)
            {
                employee.currentLevel += 1;
                employee.skillPoints += 1; // 기획서대로 1점 지급
                Debug.Log($"{employee.firstName}의 레벨이 {employee.currentLevel}로 증가했고, 스킬 포인트 1점을 얻었습니다.");
            }
            UpdateHiredEmployeeListUI();
        }
        else
        {
            Debug.LogWarning("현재 고용된 직원이 없습니다. 먼저 직원을 고용하세요.");
        }
    }

    public void ShowDismissalConfirmation(EmployeeInstance employee)
    {
        if (dismissalConfirmationPanel != null && employee != null)
        {
            employeeToDismiss = employee;
            if (dismissalNameText != null)
            {
                dismissalNameText.text = $"'{employee.firstName}'을(를) 정말로 해고 하시겠습니까?";
            }
            dismissalConfirmationPanel.SetActive(true);
        }
    }

    public void ConfirmDismissal()
    {
        if (employeeToDismiss != null && EmployeeManager.Instance != null)
        {
            // 주인공 또는 특정 직원(Goblin Chef)은 해고 불가능하도록 EmployeeManager에서 처리해야 함
            EmployeeManager.Instance.DismissEmployee(employeeToDismiss);
            employeeToDismiss = null;
        }
        UpdateHiredEmployeeListUI(); // 해고 후 목록 갱신
        HideDismissalConfirmation();
    }

    public void HideDismissalConfirmation()
    {
        if (dismissalConfirmationPanel != null)
        {
            dismissalConfirmationPanel.SetActive(false);
        }
        employeeToDismiss = null;
    }

    /// <summary>
    /// 지원자 목록 UI를 최신 정보로 새로고침합니다.
    /// </summary>
    public void UpdateApplicantListUI(List<GeneratedApplicant> applicants)
    {
        // 기존 카드 제거
        foreach (GameObject card in spawnedApplicantCards) { Destroy(card); }
        spawnedApplicantCards.Clear();

        if (applicantCardPrefab == null || applicantCardParent == null)
        {
            Debug.LogWarning("Applicant UI 업데이트 실패: 프리팹 또는 부모 오브젝트가 연결되지 않았습니다.");
            return;
        }

        foreach (GeneratedApplicant applicant in applicants)
        {
            // Null 참조 방어: 유효하지 않은 지원자 데이터는 건너뜁니다.
            if (applicant == null || applicant.BaseSpeciesData == null)
            {
                Debug.LogWarning("Null 또는 유효하지 않은 지원자 데이터가 감지되어 건너뜁니다.");
                continue;
            }

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
        // 기존 카드 제거
        foreach (GameObject card in spawnedHiredCards) { Destroy(card); }
        spawnedHiredCards.Clear();

        if (hiredCardPrefab == null || hiredCardParent == null || EmployeeManager.Instance == null)
        {
            Debug.LogWarning("Hired Employee UI 업데이트 실패: 프리팹, 부모 오브젝트, 또는 EmployeeManager가 Null입니다.");
            return;
        }

        foreach (EmployeeInstance employee in EmployeeManager.Instance.hiredEmployees)
        {
            // Null 참조 방어: 유효하지 않은 직원 데이터는 건너뜁니다.
            if (employee == null || employee.BaseData == null)
            {
                Debug.LogWarning("Null 또는 유효하지 않은 직원 데이터가 감지되어 건너뜁니다.");
                continue;
            }

            GameObject newCard = Instantiate(hiredCardPrefab, hiredCardParent);
            UpdateHiredCardUI(newCard, employee);
            spawnedHiredCards.Add(newCard);
        }
    }

    /// <summary>
    /// '지원자' 카드(ApplicantSlot) 한 개의 내용을 채웁니다. (널 참조 방어 로직 강화)
    /// </summary>
    private void UpdateApplicantCardUI(GameObject card, GeneratedApplicant applicant)
    {
        // Null 체크는 이미 UpdateApplicantListUI에서 했지만, 여기서도 다시 한 번 방어합니다.
        if (applicant == null || applicant.BaseSpeciesData == null) return;

        Image portraitImage = card.transform.Find("PortraitImage")?.GetComponent<Image>();
        TextMeshProUGUI nameText = card.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI statsText = card.transform.Find("StatsText")?.GetComponent<TextMeshProUGUI>();
        Button hireButton = card.transform.Find("HireButton")?.GetComponent<Button>();

        TextMeshProUGUI gradeText = card.transform.Find("GradeText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI salaryText = card.transform.Find("SalaryText")?.GetComponent<TextMeshProUGUI>(); // 'SalaryText' 찾기

        // Null 체크 강화
        if (portraitImage != null && applicant.BaseSpeciesData.portrait != null)
            portraitImage.sprite = applicant.BaseSpeciesData.portrait;

        if (nameText != null)
        {
            nameText.text = $"{applicant.GeneratedFirstName}\n<size=20>({applicant.BaseSpeciesData.speciesName})</size>";
        }

        // GradeText에 등급 표시 (예: "<color=yellow>S</color>등급")
        if (gradeText != null)
        {
            gradeText.text = $"<color=yellow>{applicant.grade.ToString()}</color>등급";
        }

        // SalaryText에 급여 표시
        if (salaryText != null)
        {
            salaryText.text = $"급여: {applicant.BaseSpeciesData.salary}G";
        }

        if (statsText != null)
        {
            var statsBuilder = new System.Text.StringBuilder();
            statsBuilder.AppendLine($"요리: {applicant.GeneratedCookingStat}");
            statsBuilder.AppendLine($"서빙: {applicant.GeneratedServingStat}");
            statsBuilder.AppendLine($"매력: {applicant.GeneratedCharmStat}");

            // 특성 리스트와 특성 객체 자체에 대한 널 체크 강화
            if (applicant.GeneratedTraits != null && applicant.GeneratedTraits.Any())
            {
                Trait firstTrait = applicant.GeneratedTraits[0];
                if (firstTrait != null) // 특성 객체 자체가 null이 아닌지 체크
                {
                    statsBuilder.AppendLine($"\n특성: <color=yellow>{firstTrait.traitName}</color>");
                }
            }
            statsText.text = statsBuilder.ToString();
            statsText.lineSpacing = 5f;
        }

        if (hireButton != null && EmployeeManager.Instance != null)
        {
            hireButton.onClick.RemoveAllListeners();
            hireButton.onClick.AddListener(() => EmployeeManager.Instance.HireEmployee(applicant));
        }
    }

    /// <summary>
    /// '고용된 직원' 카드(HiredEmployeeCard) 한 개의 내용을 채우고 버튼 기능을 연결합니다.
    /// </summary>
    private void UpdateHiredCardUI(GameObject card, EmployeeInstance employee)
    {
        // Null 체크는 이미 UpdateHiredEmployeeListUI에서 했지만, 여기서도 다시 한 번 방어합니다.
        if (employee == null || employee.BaseData == null) return;

        // --- 1. 기본 UI 요소 찾기 ---
        Image portraitImage = card.transform.Find("PortraitImage")?.GetComponent<Image>();
        TextMeshProUGUI nameText = card.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI statsText = card.transform.Find("StatsText")?.GetComponent<TextMeshProUGUI>();

        TextMeshProUGUI gradeText = card.transform.Find("GradeText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI salaryText = card.transform.Find("SalaryText")?.GetComponent<TextMeshProUGUI>(); // 'SalaryText' 찾기

        // --- 2. 버튼 찾기 ---
        Button levelUpBtn = card.transform.Find("LevelUpButton")?.GetComponent<Button>();
        Button cookUpBtn = card.transform.Find("CookUpgradeButton")?.GetComponent<Button>();
        Button serveUpBtn = card.transform.Find("ServeUpgradeButton")?.GetComponent<Button>();
        Button charmUpBtn = card.transform.Find("CharmUpgradeButton")?.GetComponent<Button>();
        Button dismissBtn = card.transform.Find("DismissButton")?.GetComponent<Button>();

        // --- 3. 역할(Role) 드롭다운 찾기 ---
        TMP_Dropdown roleDropdown = card.transform.Find("RoleDropdown")?.GetComponent<TMP_Dropdown>();

        // Null 체크 강화
        if (portraitImage != null && employee.BaseData.portrait != null)
            portraitImage.sprite = employee.BaseData.portrait;

        if (nameText != null)
        {
            nameText.text = $"{employee.firstName}\n<size=24>[Lv. {employee.currentLevel}]<color=yellow>({employee.skillPoints})</color></size>\n<size=20>({employee.BaseData.speciesName})</size>";
        }

        // GradeText에 등급 표시 (예: "<color=yellow>S</color>등급")
        if (gradeText != null)
        {
            gradeText.text = $"<color=yellow>{employee.grade.ToString()}</color>등급";
        }

        // SalaryText에 급여 표시
        if (salaryText != null)
        {
            salaryText.text = $"급여: {employee.currentSalary}G";
        }

        if (statsText != null)
        {
            var statsBuilder = new System.Text.StringBuilder();
            statsBuilder.AppendLine($"요리: {employee.currentCookingStat}");
            statsBuilder.AppendLine($"서빙: {employee.currentServingStat}");
            statsBuilder.AppendLine($"매력: {employee.currentCharmStat}");

            // 특성 리스트와 특성 객체 자체에 대한 널 체크 강화
            if (employee.currentTraits != null && employee.currentTraits.Any())
            {
                Trait firstTrait = employee.currentTraits[0];
                if (firstTrait != null)
                {
                    statsBuilder.AppendLine($"\n특성: <color=yellow>{firstTrait.traitName}</color>");
                }
            }
            statsText.text = statsBuilder.ToString();
            statsText.lineSpacing = 5f;
        }

        // =======================================================
        // ★★★ 버튼 및 드롭다운 기능 연결 ★★★
        // =======================================================
        if (EmployeeManager.Instance != null)
        {
            // --- 레벨업 버튼 (SP 획득) ---
            if (levelUpBtn != null)
            {
                // (참고: TryLevelUp 함수 내부에서 골드/최대레벨을 체크합니다)
                levelUpBtn.onClick.RemoveAllListeners();
                // 레벨업 성공 시(SP 1 획득) UI 전체 갱신
                levelUpBtn.onClick.AddListener(() => {
                    if (employee.TryLevelUp())
                    {
                        UpdateHiredEmployeeListUI();
                    }
                });
            }

            // --- 스탯 분배 버튼 (SP 소모) ---
            if (cookUpBtn != null)
            {
                cookUpBtn.interactable = employee.skillPoints > 0;
                cookUpBtn.onClick.RemoveAllListeners();
                cookUpBtn.onClick.AddListener(() => { if (employee.SpendSkillPointOnCooking()) UpdateHiredEmployeeListUI(); });
            }

            if (serveUpBtn != null)
            {
                serveUpBtn.interactable = employee.skillPoints > 0;
                serveUpBtn.onClick.RemoveAllListeners();
                serveUpBtn.onClick.AddListener(() => { if (employee.SpendSkillPointOnServing()) UpdateHiredEmployeeListUI(); });
            }

            if (charmUpBtn != null)
            {
                charmUpBtn.interactable = employee.skillPoints > 0;
                charmUpBtn.onClick.RemoveAllListeners();
                charmUpBtn.onClick.AddListener(() => { if (employee.SpendSkillPointOnCharm()) UpdateHiredEmployeeListUI(); });
            }

            // --- 해고 버튼 ---
            if (dismissBtn != null)
            {
                dismissBtn.onClick.RemoveAllListeners();
                bool isProtagonistFlag = employee.isProtagonist;
                string employeeName = employee.firstName;
                if (isProtagonistFlag || employeeName.Equals("Goblin Chef", System.StringComparison.OrdinalIgnoreCase))
                {
                    dismissBtn.interactable = false;
                }
                else
                {
                    dismissBtn.interactable = true;
                    dismissBtn.onClick.AddListener(() => ShowDismissalConfirmation(employee));
                }
            }

            // --- 역할(Role) 드롭다운 리스너 연결 ---
            if (roleDropdown != null)
            {
                // 직원이 1명일 때(주인공 혼자)는 드롭다운을 비활성화하고 '미지정'으로 강제
                if (EmployeeManager.Instance.hiredEmployees.Count == 1)
                {
                    employee.assignedRole = EmployeeRole.Unassigned; // 데이터 강제
                    roleDropdown.value = (int)EmployeeRole.Unassigned; // UI 강제
                    roleDropdown.interactable = false; // 비활성화
                }
                else
                {
                    // 직원이 2명 이상일 때만 드롭다운을 활성화하고 리스너 연결
                    roleDropdown.interactable = true;
                    roleDropdown.onValueChanged.RemoveAllListeners();
                    roleDropdown.value = (int)employee.assignedRole;
                    roleDropdown.onValueChanged.AddListener((newRoleIndex) => {
                        OnRoleChanged(employee, (EmployeeRole)newRoleIndex);
                    });
                }
            }
        }
    }

    /// <summary>
    /// 직원의 역할(Role) 드롭다운 값이 변경되었을 때 호출되는 함수입니다.
    /// </summary>
    /// <param name="employee">역할이 변경된 직원 데이터</param>
    /// <param name="newRole">새롭게 선택된 역할 (Unassigned, Kitchen, Hall)</param>
    private void OnRoleChanged(EmployeeInstance employee, EmployeeRole newRole)
    {
        // 1. 직원의 데이터(EmployeeInstance)에 새 역할을 저장합니다.
        employee.assignedRole = newRole;
        Debug.Log($"[역할 변경] {employee.firstName}의 역할이 {newRole.ToString()}(으)로 지정되었습니다.");

        // 2. 시너지 매니저를 호출하여 시너지를 즉시 새로고침합니다.
        if (SynergyManager.Instance != null && EmployeeManager.Instance != null)
        {
            SynergyManager.Instance.UpdateActiveSynergies(EmployeeManager.Instance.hiredEmployees);
        }

        // 3. (선택 사항) UI를 새로고침하여 이름 옆에 (Kitchen) 등이 표시되게 할 수 있습니다.
        // UpdateHiredEmployeeListUI();
    }
}