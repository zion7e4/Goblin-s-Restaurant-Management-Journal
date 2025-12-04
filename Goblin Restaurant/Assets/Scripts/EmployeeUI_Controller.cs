using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

/// <summary>
/// 직원 관리 UI(태블릿 허브, 고용, 배치, 해고 등)를 제어하는 메인 컨트롤러입니다.
/// </summary>
public class EmployeeUI_Controller : MonoBehaviour
{
    public static EmployeeUI_Controller Instance { get; private set; }

    [Header("1. 메인 패널 (Employee_Panel)")]
    public GameObject employeePanel;
    public Button btn_CloseMenu;

    [Header("2. 메인 버튼 그룹 (허브 화면)")]
    public GameObject mainButtonsGroup;
    public Button btn_Hire;
    public Button btn_Manage;
    public Button btn_Assignment; // 배치 버튼

    [Header("3. 서브 패널 - 고용 (RecruitmentPanel)")]
    public GameObject recruitmentPanel;
    public Button btn_BackFromRecruitment;
    public GameObject applicantCardPrefab;
    public Transform applicantCardParent;

    [Header("4. 서브 패널 - 관리 (ManageEmployeePanel)")]
    public GameObject manageEmployeePanel;
    public Button btn_BackFromManage;
    public GameObject hiredCardPrefab;
    public Transform hiredCardParent;

    [Header("5. 서브 패널 - 배치 (AssignmentPanel)")]
    public GameObject assignmentPanel;
    public Button btn_BackFromAssignment;

    [Tooltip("주방 직원 리스트 위치")]
    public Transform kitchenListParent;
    [Tooltip("홀 직원 리스트 위치")]
    public Transform hallListParent;
    [Tooltip("시너지 리스트 위치")]
    public Transform synergyListParent;

    [Tooltip("배치 화면용 직원 카드 (별도로 만들거나 HiredCard 재사용)")]
    public GameObject assignedWorkerPrefab;
    public GameObject synergyTextPrefab;

    [Header("6. 팝업 (해고 확인)")]
    public GameObject dismissalConfirmationPanel;
    public TextMeshProUGUI dismissalNameText;
    public Button Button_ConfirmDismiss;
    public Button Button_CancelDismiss;

    // --- 내부 변수 ---
    private EmployeeInstance employeeToDismiss;
    private List<GameObject> spawnedApplicantCards = new List<GameObject>();
    private List<GameObject> spawnedHiredCards = new List<GameObject>();
    private List<GameObject> spawnedAssignedCards = new List<GameObject>();
    private List<GameObject> spawnedSynergyTexts = new List<GameObject>();

    void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }
    }

    void Start()
    {
        if (btn_Hire != null) btn_Hire.onClick.AddListener(() => OpenSubPanel(recruitmentPanel));
        if (btn_Manage != null) btn_Manage.onClick.AddListener(() => OpenSubPanel(manageEmployeePanel));
        if (btn_Assignment != null) btn_Assignment.onClick.AddListener(() => OpenSubPanel(assignmentPanel));

        if (btn_CloseMenu != null) btn_CloseMenu.onClick.AddListener(ClosePanel);

        if (btn_BackFromRecruitment != null) btn_BackFromRecruitment.onClick.AddListener(BackToHub);
        if (btn_BackFromManage != null) btn_BackFromManage.onClick.AddListener(BackToHub);
        if (btn_BackFromAssignment != null) btn_BackFromAssignment.onClick.AddListener(BackToHub);

        if (Button_ConfirmDismiss != null) Button_ConfirmDismiss.onClick.AddListener(ConfirmDismissal);
        if (Button_CancelDismiss != null) Button_CancelDismiss.onClick.AddListener(HideDismissalConfirmation);

        ClosePanel();
    }

    void Update()
    {
        if (employeePanel != null && employeePanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            if (dismissalConfirmationPanel != null && dismissalConfirmationPanel.activeSelf)
            {
                HideDismissalConfirmation();
                return;
            }

            if ((recruitmentPanel != null && recruitmentPanel.activeSelf) ||
                (manageEmployeePanel != null && manageEmployeePanel.activeSelf) ||
                (assignmentPanel != null && assignmentPanel.activeSelf))
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
    // 패널 제어
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
        if (assignmentPanel != null) assignmentPanel.SetActive(false);

        if (mainButtonsGroup != null) mainButtonsGroup.SetActive(true);
        if (dismissalConfirmationPanel != null) dismissalConfirmationPanel.SetActive(false);
    }

    private void OpenSubPanel(GameObject panelToShow)
    {
        if (mainButtonsGroup != null) mainButtonsGroup.SetActive(false);

        if (recruitmentPanel != null) recruitmentPanel.SetActive(false);
        if (manageEmployeePanel != null) manageEmployeePanel.SetActive(false);
        if (assignmentPanel != null) assignmentPanel.SetActive(false);

        if (panelToShow != null)
        {
            panelToShow.SetActive(true);

            if (panelToShow == recruitmentPanel && EmployeeManager.Instance != null)
                UpdateApplicantListUI(EmployeeManager.Instance.applicants);
            else if (panelToShow == manageEmployeePanel)
                UpdateHiredEmployeeListUI();
            else if (panelToShow == assignmentPanel)
                UpdateAssignmentUI();
        }
    }

    // ==================================================================================
    // [배치] 화면 UI 업데이트
    // ==================================================================================
    public void UpdateAssignmentUI()
    {
        // 1. 기존 카드/텍스트 싹 지우기 (초기화)
        foreach (var obj in spawnedAssignedCards) Destroy(obj);
        spawnedAssignedCards.Clear();
        foreach (var obj in spawnedSynergyTexts) Destroy(obj);
        spawnedSynergyTexts.Clear();

        if (EmployeeManager.Instance == null) return;

        // 2. 배치용 카드 생성
        // (assignedWorkerPrefab이 없으면 관리용 카드 hiredCardPrefab을 대신 씀)
        GameObject cardTemplate = assignedWorkerPrefab != null ? assignedWorkerPrefab : hiredCardPrefab;

        if (cardTemplate != null)
        {
            foreach (EmployeeInstance emp in EmployeeManager.Instance.hiredEmployees)
            {
                Transform targetParent = null;

                // 직원의 현재 역할에 따라 들어갈 부모(구역) 결정
                if (emp.assignedRole == EmployeeRole.Kitchen)
                    targetParent = kitchenListParent;
                else if (emp.assignedRole == EmployeeRole.Hall)
                    targetParent = hallListParent;

                // (미지정 직원은 '대기실' 구역이 없다면 일단 표시 안 함 or 별도 처리가능)

                if (targetParent != null)
                {
                    GameObject newCard = Instantiate(cardTemplate, targetParent);
                    UpdateAssignedCardUI(newCard, emp); // 데이터 채우기
                    spawnedAssignedCards.Add(newCard);
                }
            }
        }

        // 3. 시너지 텍스트 표시 (SynergyManager 연동)
        if (synergyListParent != null && synergyTextPrefab != null && SynergyManager.Instance != null)
        {
            // SynergyManager에서 활성화된 시너지 이름 리스트를 가져온다고 가정
            List<string> activeSynergies = SynergyManager.Instance.GetActiveSynergyNames();

            if (activeSynergies.Count == 0)
            {
                // 시너지가 없으면 안내 문구 하나 생성
                CreateSynergyText("발동된 시너지가 없습니다.");
            }
            else
            {
                foreach (string synergy in activeSynergies)
                {
                    CreateSynergyText(synergy);
                }
            }
        }
    }

    // 시너지 텍스트 생성 헬퍼 함수
    private void CreateSynergyText(string content)
    {
        GameObject newTextObj = Instantiate(synergyTextPrefab, synergyListParent);
        TextMeshProUGUI tmp = newTextObj.GetComponent<TextMeshProUGUI>();
        // 프리팹이 바로 텍스트일 수도 있고, 자식에 있을 수도 있음
        if (tmp == null) tmp = newTextObj.GetComponentInChildren<TextMeshProUGUI>();

        if (tmp != null) tmp.text = content;
        spawnedSynergyTexts.Add(newTextObj);
    }

    /// <summary>
    /// 배치 화면용 카드 정보 채우기 (드롭다운 포함)
    /// </summary>
    private void UpdateAssignedCardUI(GameObject card, EmployeeInstance employee)
    {
        // 1. UI 찾기 (HiredCard와 유사한 구조라고 가정)
        Image portraitImage = card.transform.Find("PortraitImage")?.GetComponent<Image>();
        TMP_Dropdown roleDropdown = card.transform.Find("RoleDropdown")?.GetComponent<TMP_Dropdown>();

        // 텍스트들 (구조에 따라 경로 수정 필요)
        TextMeshProUGUI nameText = card.transform.Find("Text/Name/NameText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI cookText = card.transform.Find("CookStatText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI serveText = card.transform.Find("ServeStatText")?.GetComponent<TextMeshProUGUI>();

        // 2. 데이터 적용
        if (portraitImage != null && employee.BaseData.portrait != null)
            portraitImage.sprite = employee.BaseData.portrait;

        if (nameText != null)
            nameText.text = $"{employee.firstName} <size=80%>(Lv.{employee.currentLevel})</size>";

        // 스탯 (배치할 때 참고용)
        if (cookText != null) cookText.text = $"{employee.currentCookingStat}";
        if (serveText != null) serveText.text = $"{employee.currentServingStat}";

        // ★★★ 3. 역할 변경 드롭다운 설정 (핵심 기능) ★★★
        if (roleDropdown != null)
        {
            roleDropdown.ClearOptions();
            // 옵션 추가 (순서: 0:미지정, 1:주방, 2:홀) -> Enum 순서와 맞춰야 함!
            roleDropdown.AddOptions(new List<string> { "대기", "주방", "홀" });

            // 현재 역할 선택
            roleDropdown.SetValueWithoutNotify((int)employee.assignedRole);

            // 이벤트 연결
            roleDropdown.onValueChanged.RemoveAllListeners();
            roleDropdown.onValueChanged.AddListener((newRoleIndex) => {

                // 1. 데이터 변경
                employee.assignedRole = (EmployeeRole)newRoleIndex;
                Debug.Log($"[직원배치] {employee.firstName} -> {(EmployeeRole)newRoleIndex} 이동");

                // 2. 시너지 재계산 (매니저가 있다면)
                if (SynergyManager.Instance != null)
                    SynergyManager.Instance.UpdateActiveSynergies(EmployeeManager.Instance.hiredEmployees);

                // 3. ★화면 갱신★ (카드가 주방<->홀로 즉시 이동해야 하므로 UI를 다시 그림)
                UpdateAssignmentUI();
            });
        }
    }

    // ==================================================================================
    // [고용] 화면 UI 업데이트
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
        Image portraitImage = card.transform.Find("PortraitImage")?.GetComponent<Image>();
        TextMeshProUGUI cookText = card.transform.Find("CookStatText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI serveText = card.transform.Find("ServeStatText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI charmText = card.transform.Find("CharmStatText")?.GetComponent<TextMeshProUGUI>();
        Button hireButton = card.transform.Find("HireButton")?.GetComponent<Button>();

        TextMeshProUGUI costText = card.transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
        if (costText == null) costText = card.transform.Find("Text/Salary/SalaryText")?.GetComponent<TextMeshProUGUI>();

        TextMeshProUGUI nameText = card.transform.Find("Text/Name/NameText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI speciesText = card.transform.Find("Text/Species/SpeciesText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI gradeText = card.transform.Find("Text/Grade/GradeText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI traitText = card.transform.Find("Text/Trait/TraitText")?.GetComponent<TextMeshProUGUI>();

        if (portraitImage != null && applicant.BaseSpeciesData.portrait != null)
            portraitImage.sprite = applicant.BaseSpeciesData.portrait;
        if (nameText != null) nameText.text = applicant.GeneratedFirstName;
        if (speciesText != null) speciesText.text = GetKoreanSpeciesName(applicant.BaseSpeciesData.speciesName);
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
            else traitText.text = "-";
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

    // ==================================================================================
    // [관리] 화면 UI 업데이트
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
        // 1. UI 요소 찾기
        Image portraitImage = card.transform.Find("PortraitImage")?.GetComponent<Image>();
        TextMeshProUGUI cookText = card.transform.Find("CookStatText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI serveText = card.transform.Find("ServeStatText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI charmText = card.transform.Find("CharmStatText")?.GetComponent<TextMeshProUGUI>();

        TextMeshProUGUI levelText = card.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI spText = card.transform.Find("SPText")?.GetComponent<TextMeshProUGUI>();
        Button cookUpBtn = card.transform.Find("CookUpButton")?.GetComponent<Button>();
        Button serveUpBtn = card.transform.Find("ServeUpButton")?.GetComponent<Button>();
        Button charmUpBtn = card.transform.Find("CharmUpButton")?.GetComponent<Button>();
        Button levelUpBtn = card.transform.Find("LevelUpButton")?.GetComponent<Button>();

        TextMeshProUGUI nameText = card.transform.Find("Text/Name/NameText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI speciesText = card.transform.Find("Text/Species/SpeciesText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI gradeText = card.transform.Find("Text/Grade/GradeText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI traitText = card.transform.Find("Text/Trait/TraitText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI salaryText = card.transform.Find("Text/Salary/SalaryText")?.GetComponent<TextMeshProUGUI>();

        Button dismissBtn = card.transform.Find("DismissButton")?.GetComponent<Button>();

        // ★ [삭제됨] RoleDropdown 관련 코드는 모두 제거했습니다.

        // 2. 데이터 적용
        if (portraitImage != null && employee.BaseData.portrait != null) portraitImage.sprite = employee.BaseData.portrait;
        if (nameText != null) nameText.text = employee.firstName;
        if (speciesText != null) speciesText.text = GetKoreanSpeciesName(employee.BaseData.speciesName);
        if (levelText != null) levelText.text = $"Lv.{employee.currentLevel}";
        else if (nameText != null) nameText.text = $"{employee.firstName} <size=80%>(Lv.{employee.currentLevel})</size>";

        if (spText != null) spText.text = (employee.skillPoints > 0) ? $"SP: <color=yellow>{employee.skillPoints}</color>" : "SP: 0";
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
            else traitText.text = "-";
        }
        if (salaryText != null)
        {
            salaryText.text = $"{employee.currentSalary} G";
            salaryText.color = Color.white;
        }

        // 스탯 강화 버튼
        bool hasSP = employee.skillPoints > 0;
        if (cookUpBtn != null)
        {
            cookUpBtn.interactable = hasSP;
            cookUpBtn.onClick.RemoveAllListeners();
            cookUpBtn.onClick.AddListener(() => { if (employee.SpendSkillPointOnCooking()) UpdateHiredEmployeeListUI(); });
        }
        if (serveUpBtn != null)
        {
            serveUpBtn.interactable = hasSP;
            serveUpBtn.onClick.RemoveAllListeners();
            serveUpBtn.onClick.AddListener(() => { if (employee.SpendSkillPointOnServing()) UpdateHiredEmployeeListUI(); });
        }
        if (charmUpBtn != null)
        {
            charmUpBtn.interactable = hasSP;
            charmUpBtn.onClick.RemoveAllListeners();
            charmUpBtn.onClick.AddListener(() => { if (employee.SpendSkillPointOnCharm()) UpdateHiredEmployeeListUI(); });
        }

        // 레벨업 버튼
        if (levelUpBtn != null)
        {
            int maxLevel = 20;
            int nextLevelCost = (int)(100 * Mathf.Pow(1.1f, employee.currentLevel - 1));
            TextMeshProUGUI btnText = levelUpBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (employee.currentLevel >= maxLevel)
            {
                levelUpBtn.interactable = false;
                if (btnText != null) btnText.text = "MAX";
            }
            else
            {
                bool canAfford = GameManager.instance.totalGoldAmount >= nextLevelCost;
                levelUpBtn.interactable = canAfford;
                if (btnText != null) btnText.text = $"LvUP <size=80%>({nextLevelCost}G)</size>";
                levelUpBtn.onClick.RemoveAllListeners();
                levelUpBtn.onClick.AddListener(() => { if (employee.TryLevelUp()) UpdateHiredEmployeeListUI(); });
            }
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
    }

    // --- 헬퍼 함수 ---
    private string GetKoreanSpeciesName(string englishName)
    {
        switch (englishName)
        {
            case "Elf": return "엘프";
            case "Dwarf": return "드워프";
            case "Goblin": return "고블린";
            case "Orc": return "오크";
            case "Human": return "인간";
            default: return englishName;
        }
    }

    private string GetGradeColorHex(EmployeeGrade grade)
    {
        switch (grade)
        {
            case EmployeeGrade.S: return "#FFD700";
            case EmployeeGrade.A: return "#9370DB";
            case EmployeeGrade.B: return "#1E90FF";
            default: return "#FFFFFF";
        }
    }

    // --- 해고 팝업 ---
    public void ShowDismissalConfirmation(EmployeeInstance employee)
    {
        employeeToDismiss = employee;
        if (dismissalConfirmationPanel != null)
        {
            dismissalConfirmationPanel.SetActive(true);
            if (dismissalNameText != null) dismissalNameText.text = $"'{employee.firstName}' 직원을\n해고하시겠습니까?";
        }
    }

    public void ConfirmDismissal()
    {
        if (employeeToDismiss != null && EmployeeManager.Instance != null)
            EmployeeManager.Instance.DismissEmployee(employeeToDismiss);
        HideDismissalConfirmation();
        UpdateHiredEmployeeListUI();
        if (assignmentPanel.activeSelf) UpdateAssignmentUI();
    }

    public void HideDismissalConfirmation()
    {
        employeeToDismiss = null;
        if (dismissalConfirmationPanel != null) dismissalConfirmationPanel.SetActive(false);
    }
}
