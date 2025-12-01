using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq; // 리스트 및 LINQ 기능을 위해 필요
using UnityEngine.EventSystems;

/// <summary>
/// 직원 관리 UI(태블릿 허브, 고용, 배치, 해고 등)를 제어하는 메인 컨트롤러입니다.
/// </summary>
public class EmployeeUI_Controller : MonoBehaviour
{
    // 싱글톤 패턴: 어디서든 접근 가능하도록 설정
    public static EmployeeUI_Controller Instance { get; private set; }

    [Header("1. 메인 패널 (Employee_Panel)")]
    [Tooltip("전체 배경 이미지와 닫기 버튼이 있는 최상위 부모 패널입니다.")]
    public GameObject employeePanel;

    [Tooltip("메뉴 전체를 닫는 X 버튼입니다.")]
    public Button btn_CloseMenu;

    [Header("2. 메인 버튼 그룹 (허브 화면)")]
    [Tooltip("고용/관리 버튼을 묶어둔 빈 오브젝트입니다. 서브 패널이 열리면 숨겨집니다.")]
    public GameObject mainButtonsGroup;

    [Tooltip("직원 고용 화면으로 이동하는 버튼")]
    public Button btn_Hire;

    [Tooltip("직원 관리(배치) 화면으로 이동하는 버튼")]
    public Button btn_Manage;

    [Header("3. 서브 패널 - 고용 (RecruitmentPanel)")]
    [Tooltip("지원자 목록이 표시되는 패널입니다.")]
    public GameObject recruitmentPanel;
    [Tooltip("고용 화면 뒤로가기 버튼")]
    public Button btn_BackFromRecruitment;

    [Tooltip("지원자 카드 프리팹 (ApplicantSlot)")]
    public GameObject applicantCardPrefab;
    [Tooltip("지원자 카드가 생성될 위치 (Scroll View의 Content)")]
    public Transform applicantCardParent;

    [Header("4. 서브 패널 - 관리 (ManageEmployeePanel)")]
    [Tooltip("보유 중인 직원 목록이 표시되는 패널입니다.")]
    public GameObject manageEmployeePanel;
    [Tooltip("관리 화면 뒤로가기 버튼")]
    public Button btn_BackFromManage;

    [Tooltip("직원 카드 프리팹 (HiredEmployeeCard)")]
    public GameObject hiredCardPrefab;
    [Tooltip("직원 카드가 생성될 위치 (Scroll View의 Content)")]
    public Transform hiredCardParent;

    [Header("5. 팝업 (해고 확인)")]
    public GameObject dismissalConfirmationPanel;
    public TextMeshProUGUI dismissalNameText;
    public Button Button_ConfirmDismiss;
    public Button Button_CancelDismiss;

    // --- 내부 변수 (스크립트가 자동으로 관리) ---
    private EmployeeInstance employeeToDismiss; // 해고 대기 중인 직원 데이터
    private List<GameObject> spawnedApplicantCards = new List<GameObject>(); // 생성된 지원자 카드 리스트
    private List<GameObject> spawnedHiredCards = new List<GameObject>();     // 생성된 직원 카드 리스트

    void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }
    }

    void Start()
    {
        // --- 버튼 클릭 이벤트 연결 ---

        // 메인 메뉴 버튼
        if (btn_Hire != null) btn_Hire.onClick.AddListener(() => OpenSubPanel(recruitmentPanel));
        if (btn_Manage != null) btn_Manage.onClick.AddListener(() => OpenSubPanel(manageEmployeePanel));

        // 닫기 버튼
        if (btn_CloseMenu != null) btn_CloseMenu.onClick.AddListener(ClosePanel);

        // 뒤로가기 버튼
        if (btn_BackFromRecruitment != null) btn_BackFromRecruitment.onClick.AddListener(BackToHub);
        if (btn_BackFromManage != null) btn_BackFromManage.onClick.AddListener(BackToHub);

        // 해고 팝업 버튼
        if (Button_ConfirmDismiss != null) Button_ConfirmDismiss.onClick.AddListener(ConfirmDismissal);
        if (Button_CancelDismiss != null) Button_CancelDismiss.onClick.AddListener(HideDismissalConfirmation);

        // 게임 시작 시 패널 닫기
        ClosePanel();
    }

    void Update()
    {
        // ESC 키 기능
        if (employeePanel != null && employeePanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            if (dismissalConfirmationPanel != null && dismissalConfirmationPanel.activeSelf)
            {
                HideDismissalConfirmation();
                return;
            }

            if ((recruitmentPanel != null && recruitmentPanel.activeSelf) ||
                (manageEmployeePanel != null && manageEmployeePanel.activeSelf))
            {
                BackToHub();
            }
            else
            {
                ClosePanel();
            }
        }
    }

    // ==================================================================================
    // 1. 패널 열기 / 닫기 로직
    // ==================================================================================

    public void OpenPanel()
    {
        if (employeePanel != null) employeePanel.SetActive(true);
        BackToHub();
    }

    public void ClosePanel()
    {
        if (employeePanel != null) employeePanel.SetActive(false);
        if (dismissalConfirmationPanel != null) dismissalConfirmationPanel.SetActive(false);
    }

    private void BackToHub()
    {
        if (recruitmentPanel != null) recruitmentPanel.SetActive(false);
        if (manageEmployeePanel != null) manageEmployeePanel.SetActive(false);

        if (mainButtonsGroup != null) mainButtonsGroup.SetActive(true);

        if (dismissalConfirmationPanel != null) dismissalConfirmationPanel.SetActive(false);
    }

    private void OpenSubPanel(GameObject panelToShow)
    {
        if (mainButtonsGroup != null) mainButtonsGroup.SetActive(false);

        if (recruitmentPanel != null) recruitmentPanel.SetActive(false);
        if (manageEmployeePanel != null) manageEmployeePanel.SetActive(false);

        if (panelToShow != null)
        {
            panelToShow.SetActive(true);

            if (panelToShow == recruitmentPanel && EmployeeManager.Instance != null)
            {
                UpdateApplicantListUI(EmployeeManager.Instance.applicants);
            }
            else if (panelToShow == manageEmployeePanel)
            {
                UpdateHiredEmployeeListUI();
            }
        }
    }

    // ==================================================================================
    // 2. 고용 화면 (ApplicantSlot) UI 업데이트
    // ==================================================================================

    public void UpdateApplicantListUI(List<GeneratedApplicant> applicants)
    {
        foreach (GameObject card in spawnedApplicantCards) Destroy(card);
        spawnedApplicantCards.Clear();

        if (applicantCardPrefab == null || applicantCardParent == null) return;

        foreach (GeneratedApplicant applicant in applicants)
        {
            if (applicant == null) continue;

            GameObject newCard = Instantiate(applicantCardPrefab, applicantCardParent);
            UpdateApplicantCardUI(newCard, applicant);
            spawnedApplicantCards.Add(newCard);
        }
    }

    private void UpdateApplicantCardUI(GameObject card, GeneratedApplicant applicant)
    {
        // UI 요소 찾기
        Image portraitImage = card.transform.Find("PortraitImage")?.GetComponent<Image>();
        TextMeshProUGUI cookText = card.transform.Find("CookStatText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI serveText = card.transform.Find("ServeStatText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI charmText = card.transform.Find("CharmStatText")?.GetComponent<TextMeshProUGUI>();
        Button hireButton = card.transform.Find("HireButton")?.GetComponent<Button>();

        TextMeshProUGUI costText = card.transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
        if (costText == null) costText = card.transform.Find("Text/Salary/SalaryText")?.GetComponent<TextMeshProUGUI>();

        // Text 폴더 내부 요소
        TextMeshProUGUI nameText = card.transform.Find("Text/Name/NameText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI speciesText = card.transform.Find("Text/Species/SpeciesText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI gradeText = card.transform.Find("Text/Grade/GradeText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI traitText = card.transform.Find("Text/Trait/TraitText")?.GetComponent<TextMeshProUGUI>();

        // 데이터 적용
        if (portraitImage != null && applicant.BaseSpeciesData.portrait != null)
            portraitImage.sprite = applicant.BaseSpeciesData.portrait;

        if (nameText != null)
            nameText.text = applicant.GeneratedFirstName;

        // ★ [수정됨] 종족 이름을 한글로 변환하여 표시
        if (speciesText != null)
            speciesText.text = GetKoreanSpeciesName(applicant.BaseSpeciesData.speciesName);

        if (gradeText != null)
        {
            string colorHex = GetGradeColorHex(applicant.grade);
            gradeText.text = $"<color={colorHex}>{applicant.grade}</color>";
        }

        if (cookText != null) cookText.text = $"{applicant.GeneratedCookingStat}";
        if (serveText != null) serveText.text = $"{applicant.GeneratedServingStat}";
        if (charmText != null) charmText.text = $"{applicant.GeneratedCharmStat}";

        if (traitText != null)
        {
            if (applicant.GeneratedTraits != null && applicant.GeneratedTraits.Count > 0)
                traitText.text = string.Join(", ", applicant.GeneratedTraits.Select(t => t.traitName));
            else
                traitText.text = "-";
        }

        int cost = applicant.BaseSpeciesData.salary;
        bool canAfford = GameManager.instance.totalGoldAmount >= cost;

        if (costText != null)
        {
            costText.text = $"{cost} G";
            costText.color = canAfford ? Color.white : Color.red;
        }

        if (hireButton != null)
        {
            hireButton.interactable = canAfford;
            hireButton.onClick.RemoveAllListeners();
            hireButton.onClick.AddListener(() => {
                if (EmployeeManager.Instance != null)
                {
                    EmployeeManager.Instance.HireEmployee(applicant);
                    UpdateApplicantListUI(EmployeeManager.Instance.applicants);
                }
            });
        }
    }

    /// <summary>
    /// 영어로 된 종족 이름을 한글로 변환합니다. (데이터가 추가되면 여기에도 추가해주세요)
    /// </summary>
    private string GetKoreanSpeciesName(string englishName)
    {
        switch (englishName)
        {
            case "Elf": return "엘프";
            case "Dwarf": return "드워프";
            case "Goblin": return "고블린";
            case "Orc": return "오크";
            case "Human": return "인간";
            // 여기에 새로운 종족을 계속 추가하면 됩니다.
            default: return englishName; // 목록에 없으면 원래 영어 이름 출력
        }
    }

    private string GetGradeColorHex(EmployeeGrade grade)
    {
        switch (grade)
        {
            case EmployeeGrade.S: return "#FFD700"; // Gold
            case EmployeeGrade.A: return "#9370DB"; // Purple
            case EmployeeGrade.B: return "#1E90FF"; // Blue
            default: return "#FFFFFF"; // White
        }
    }

    // ==================================================================================
    // 3. 관리 화면 (HiredEmployeeCard) UI 업데이트
    // ==================================================================================

    public void UpdateHiredEmployeeListUI()
    {
        foreach (GameObject card in spawnedHiredCards) Destroy(card);
        spawnedHiredCards.Clear();

        if (hiredCardPrefab == null || hiredCardParent == null || EmployeeManager.Instance == null) return;

        foreach (EmployeeInstance employee in EmployeeManager.Instance.hiredEmployees)
        {
            if (employee == null) continue;
            GameObject newCard = Instantiate(hiredCardPrefab, hiredCardParent);
            UpdateHiredCardUI(newCard, employee);
            spawnedHiredCards.Add(newCard);
        }
    }

    private void UpdateHiredCardUI(GameObject card, EmployeeInstance employee)
    {
        // UI 요소 찾기
        Image portraitImage = card.transform.Find("PortraitImage")?.GetComponent<Image>();
        TextMeshProUGUI cookText = card.transform.Find("CookStatText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI serveText = card.transform.Find("ServeStatText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI charmText = card.transform.Find("CharmStatText")?.GetComponent<TextMeshProUGUI>();

        // 레벨 & SP & 강화 버튼
        TextMeshProUGUI levelText = card.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI spText = card.transform.Find("SPText")?.GetComponent<TextMeshProUGUI>();
        Button cookUpBtn = card.transform.Find("CookUpButton")?.GetComponent<Button>();
        Button serveUpBtn = card.transform.Find("ServeUpButton")?.GetComponent<Button>();
        Button charmUpBtn = card.transform.Find("CharmUpButton")?.GetComponent<Button>();

        // Text 폴더 내부
        TextMeshProUGUI nameText = card.transform.Find("Text/Name/NameText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI speciesText = card.transform.Find("Text/Species/SpeciesText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI gradeText = card.transform.Find("Text/Grade/GradeText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI traitText = card.transform.Find("Text/Trait/TraitText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI salaryText = card.transform.Find("Text/Salary/SalaryText")?.GetComponent<TextMeshProUGUI>();

        // 기능 UI
        TMP_Dropdown roleDropdown = card.transform.Find("RoleDropdown")?.GetComponent<TMP_Dropdown>();
        Button dismissBtn = card.transform.Find("DismissButton")?.GetComponent<Button>();

        // 데이터 적용
        if (portraitImage != null && employee.BaseData.portrait != null)
            portraitImage.sprite = employee.BaseData.portrait;

        if (nameText != null) nameText.text = employee.firstName;

        // ★ [수정됨] 종족 이름을 한글로 변환하여 표시
        if (speciesText != null)
            speciesText.text = GetKoreanSpeciesName(employee.BaseData.speciesName);

        // 레벨
        if (levelText != null)
            levelText.text = $"Lv.{employee.currentLevel}";
        else if (nameText != null)
            nameText.text = $"{employee.firstName} <size=80%>(Lv.{employee.currentLevel})</size>";

        // SP
        if (spText != null)
        {
            if (employee.skillPoints > 0) spText.text = $"SP: <color=yellow>{employee.skillPoints}</color>";
            else spText.text = "SP: 0";
        }

        // 등급
        if (gradeText != null)
        {
            string colorHex = GetGradeColorHex(employee.grade);
            gradeText.text = $"<color={colorHex}>{employee.grade}</color>";
        }

        if (cookText != null) cookText.text = $"{employee.currentCookingStat}";
        if (serveText != null) serveText.text = $"{employee.currentServingStat}";
        if (charmText != null) charmText.text = $"{employee.currentCharmStat}";

        if (traitText != null)
        {
            if (employee.currentTraits != null && employee.currentTraits.Count > 0)
                traitText.text = string.Join(", ", employee.currentTraits.Select(t => t.traitName));
            else
                traitText.text = "-";
        }

        if (salaryText != null)
        {
            salaryText.text = $"{employee.currentSalary} G";
            salaryText.color = Color.white;
        }

        // 스탯 강화 버튼 로직
        bool hasSP = employee.skillPoints > 0;

        if (cookUpBtn != null)
        {
            cookUpBtn.interactable = hasSP;
            cookUpBtn.onClick.RemoveAllListeners();
            cookUpBtn.onClick.AddListener(() => {
                if (employee.SpendSkillPointOnCooking()) UpdateHiredEmployeeListUI();
            });
        }

        if (serveUpBtn != null)
        {
            serveUpBtn.interactable = hasSP;
            serveUpBtn.onClick.RemoveAllListeners();
            serveUpBtn.onClick.AddListener(() => {
                if (employee.SpendSkillPointOnServing()) UpdateHiredEmployeeListUI();
            });
        }

        if (charmUpBtn != null)
        {
            charmUpBtn.interactable = hasSP;
            charmUpBtn.onClick.RemoveAllListeners();
            charmUpBtn.onClick.AddListener(() => {
                if (employee.SpendSkillPointOnCharm()) UpdateHiredEmployeeListUI();
            });
        }

        // 해고 버튼
        if (dismissBtn != null)
        {
            dismissBtn.onClick.RemoveAllListeners();
            if (employee.isProtagonist) dismissBtn.interactable = false;
            else
            {
                dismissBtn.interactable = true;
                dismissBtn.onClick.AddListener(() => ShowDismissalConfirmation(employee));
            }
        }

        // 역할 드롭다운
        if (roleDropdown != null)
        {
            if (EmployeeManager.Instance.hiredEmployees.Count <= 1)
            {
                roleDropdown.value = (int)EmployeeRole.Unassigned;
                roleDropdown.interactable = false;
            }
            else
            {
                roleDropdown.interactable = true;
                roleDropdown.ClearOptions();
                roleDropdown.AddOptions(new List<string> { "미지정", "주방", "홀" });

                roleDropdown.onValueChanged.RemoveAllListeners();
                roleDropdown.value = (int)employee.assignedRole;

                roleDropdown.onValueChanged.AddListener((newRoleIndex) => {
                    employee.assignedRole = (EmployeeRole)newRoleIndex;
                    if (SynergyManager.Instance != null)
                        SynergyManager.Instance.UpdateActiveSynergies(EmployeeManager.Instance.hiredEmployees);
                });
            }
        }
    }

    // ==================================================================================
    // 4. 해고 확인 팝업 로직
    // ==================================================================================

    public void ShowDismissalConfirmation(EmployeeInstance employee)
    {
        employeeToDismiss = employee;
        if (dismissalConfirmationPanel != null)
        {
            dismissalConfirmationPanel.SetActive(true);
            if (dismissalNameText != null)
                dismissalNameText.text = $"'{employee.firstName}' 직원을\n해고하시겠습니까?";
        }
    }

    public void ConfirmDismissal()
    {
        if (employeeToDismiss != null && EmployeeManager.Instance != null)
        {
            EmployeeManager.Instance.DismissEmployee(employeeToDismiss);
        }
        HideDismissalConfirmation();
        UpdateHiredEmployeeListUI();
    }

    public void HideDismissalConfirmation()
    {
        employeeToDismiss = null;
        if (dismissalConfirmationPanel != null) dismissalConfirmationPanel.SetActive(false);
    }
}
