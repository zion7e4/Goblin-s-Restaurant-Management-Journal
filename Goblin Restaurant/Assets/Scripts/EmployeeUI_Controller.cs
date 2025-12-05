using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public class EmployeeUI_Controller : MonoBehaviour
{
    public static EmployeeUI_Controller Instance { get; private set; }

    [Header("1. 메인 패널")]
    public GameObject employeePanel;
    public Button btn_CloseMenu;

    [Header("2. 메인 버튼 그룹")]
    public GameObject mainButtonsGroup;
    public Button btn_Hire;
    public Button btn_Manage;
    public Button btn_Assignment;

    [Header("3. 서브 패널 - 고용")]
    public GameObject recruitmentPanel;
    public Button btn_BackFromRecruitment;
    public GameObject applicantCardPrefab;
    public Transform applicantCardParent;

    [Header("4. 서브 패널 - 관리")]
    public GameObject manageEmployeePanel;
    public Button btn_BackFromManage;
    public GameObject hiredCardPrefab;
    public Transform hiredCardParent;

    [Header("5. 서브 패널 - 배치")]
    public GameObject assignmentPanel;
    public Button btn_BackFromAssignment;

    [Header("배치 구역")]
    public Transform kitchenListParent;
    public Transform hallListParent;
    public Transform allRounderListParent;
    public Transform waitingListParent;
    public Transform synergyListParent;

    [Header("배치용 프리팹")]
    public GameObject assignedWorkerCardPrefab;
    public GameObject workerNameTagPrefab;
    public GameObject synergyTextPrefab;

    [Header("★ 역할 선택 팝업")]
    public GameObject roleSelectPopup;
    public Button btn_ToKitchen;
    public Button btn_ToHall;
    public Button btn_ToAllRounder;
    public Button btn_ToWait;
    public Button btn_ClosePopup;

    [Header("★ 툴팁 (Hover 기능)")]
    [Tooltip("마우스 오버 시 띄울 상세 정보 카드 (씬에 있는 오브젝트 연결)")]
    public GameObject tooltipCardObject;

    [Header("6. 팝업 (해고 확인)")]
    public GameObject dismissalConfirmationPanel;
    public TextMeshProUGUI dismissalNameText;
    public Button Button_ConfirmDismiss;
    public Button Button_CancelDismiss;

    // 내부 변수
    private EmployeeInstance employeeToDismiss;
    private EmployeeInstance selectedEmployeeForRole;
    private List<GameObject> spawnedApplicantCards = new List<GameObject>();
    private List<GameObject> spawnedHiredCards = new List<GameObject>();
    private List<GameObject> spawnedAssignedCards = new List<GameObject>();
    private List<GameObject> spawnedSynergyTexts = new List<GameObject>();

    void Awake() { if (Instance == null) { Instance = this; } else { Destroy(gameObject); } }

    void Start()
    {
        if (btn_Hire) btn_Hire.onClick.AddListener(() => OpenSubPanel(recruitmentPanel));
        if (btn_Manage) btn_Manage.onClick.AddListener(() => OpenSubPanel(manageEmployeePanel));
        if (btn_Assignment) btn_Assignment.onClick.AddListener(() => OpenSubPanel(assignmentPanel));
        if (btn_CloseMenu) btn_CloseMenu.onClick.AddListener(ClosePanel);

        if (btn_BackFromRecruitment) btn_BackFromRecruitment.onClick.AddListener(BackToHub);
        if (btn_BackFromManage) btn_BackFromManage.onClick.AddListener(BackToHub);
        if (btn_BackFromAssignment) btn_BackFromAssignment.onClick.AddListener(BackToHub);

        if (Button_ConfirmDismiss) Button_ConfirmDismiss.onClick.AddListener(ConfirmDismissal);
        if (Button_CancelDismiss) Button_CancelDismiss.onClick.AddListener(HideDismissalConfirmation);

        if (btn_ToKitchen) btn_ToKitchen.onClick.AddListener(() => ChangeRole(EmployeeRole.Kitchen));
        if (btn_ToHall) btn_ToHall.onClick.AddListener(() => ChangeRole(EmployeeRole.Hall));
        if (btn_ToAllRounder) btn_ToAllRounder.onClick.AddListener(() => ChangeRole(EmployeeRole.AllRounder));
        if (btn_ToWait) btn_ToWait.onClick.AddListener(() => ChangeRole(EmployeeRole.Unassigned));
        if (btn_ClosePopup) btn_ClosePopup.onClick.AddListener(CloseRolePopup);

        if (tooltipCardObject != null) tooltipCardObject.SetActive(false);

        ClosePanel();
    }

    void Update()
    {
        if (employeePanel != null && employeePanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            if (dismissalConfirmationPanel.activeSelf) { HideDismissalConfirmation(); return; }
            if (roleSelectPopup != null && roleSelectPopup.activeSelf) { CloseRolePopup(); return; }

            if (recruitmentPanel.activeSelf || manageEmployeePanel.activeSelf || assignmentPanel.activeSelf) BackToHub();
            else ClosePanel();
        }

        // 툴팁 마우스 따라다니기
        if (tooltipCardObject != null && tooltipCardObject.activeSelf)
        {
            tooltipCardObject.transform.position = Input.mousePosition + new Vector3(20, -20, 0);
        }
    }

    public void OpenPanel() { if (employeePanel) employeePanel.SetActive(true); BackToHub(); }
    public void ClosePanel()
    {
        if (employeePanel) employeePanel.SetActive(false);
        if (dismissalConfirmationPanel) dismissalConfirmationPanel.SetActive(false);
        CloseRolePopup();
        HideWorkerTooltip();
    }
    private void BackToHub()
    {
        if (recruitmentPanel) recruitmentPanel.SetActive(false);
        if (manageEmployeePanel) manageEmployeePanel.SetActive(false);
        if (assignmentPanel) assignmentPanel.SetActive(false);
        if (mainButtonsGroup) mainButtonsGroup.SetActive(true);
        CloseRolePopup();
        HideWorkerTooltip();
    }
    private void OpenSubPanel(GameObject panel)
    {
        if (mainButtonsGroup) mainButtonsGroup.SetActive(false);
        if (recruitmentPanel) recruitmentPanel.SetActive(false);
        if (manageEmployeePanel) manageEmployeePanel.SetActive(false);
        if (assignmentPanel) assignmentPanel.SetActive(false);

        if (panel)
        {
            panel.SetActive(true);
            if (panel == recruitmentPanel) UpdateApplicantListUI(EmployeeManager.Instance.applicants);
            else if (panel == manageEmployeePanel) UpdateHiredEmployeeListUI();
            else if (panel == assignmentPanel) UpdateAssignmentUI();
        }
    }

    // --- 툴팁 관련 함수 (복구됨) ---
    public void ShowWorkerTooltip(EmployeeInstance employee)
    {
        if (tooltipCardObject != null)
        {
            tooltipCardObject.SetActive(true);
            UpdateHiredCardUI(tooltipCardObject, employee); // 정보 채우기 재활용

            // 툴팁에선 버튼들 끄기
            Transform dismissBtn = tooltipCardObject.transform.Find("DismissButton");
            if (dismissBtn) dismissBtn.gameObject.SetActive(false);
            Transform roleDrop = tooltipCardObject.transform.Find("RoleDropdown");
            if (roleDrop) roleDrop.gameObject.SetActive(false);
        }
    }

    public void HideWorkerTooltip()
    {
        if (tooltipCardObject != null) tooltipCardObject.SetActive(false);
    }

    // --- 배치 화면 ---
    public void UpdateAssignmentUI()
    {
        foreach (var obj in spawnedAssignedCards) Destroy(obj);
        spawnedAssignedCards.Clear();
        foreach (var obj in spawnedSynergyTexts) Destroy(obj);
        spawnedSynergyTexts.Clear();

        if (EmployeeManager.Instance == null) return;

        foreach (EmployeeInstance emp in EmployeeManager.Instance.hiredEmployees)
        {
            if (emp.assignedRole == EmployeeRole.Unassigned)
            {
                if (waitingListParent && assignedWorkerCardPrefab)
                {
                    GameObject card = Instantiate(assignedWorkerCardPrefab, waitingListParent);
                    UpdateHiredCardUI(card, emp); // 정보 채우기

                    // 클릭 이벤트 (팝업)
                    Button btn = card.GetComponent<Button>();
                    if (!btn) btn = card.AddComponent<Button>();
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OpenRolePopup(emp));

                    // 툴팁 이벤트 (새 스크립트용)
                    WorkerTooltipTrigger tooltip = card.GetComponent<WorkerTooltipTrigger>();
                    if (!tooltip) tooltip = card.AddComponent<WorkerTooltipTrigger>();
                    tooltip.employeeData = emp;

                    spawnedAssignedCards.Add(card);
                }
            }
            else
            {
                Transform targetParent = null;
                switch (emp.assignedRole)
                {
                    case EmployeeRole.Kitchen: targetParent = kitchenListParent; break;
                    case EmployeeRole.Hall: targetParent = hallListParent; break;
                    case EmployeeRole.AllRounder: targetParent = allRounderListParent; break;
                }
                if (targetParent && workerNameTagPrefab)
                {
                    GameObject tag = Instantiate(workerNameTagPrefab, targetParent);
                    TextMeshProUGUI txt = tag.GetComponentInChildren<TextMeshProUGUI>();
                    if (txt) txt.text = $"{emp.firstName} (Lv.{emp.currentLevel})";

                    Button btn = tag.GetComponent<Button>();
                    if (btn)
                    {
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => OpenRolePopup(emp));
                    }

                    // 툴팁 이벤트
                    WorkerTooltipTrigger tooltip = tag.GetComponent<WorkerTooltipTrigger>();
                    if (!tooltip) tooltip = tag.AddComponent<WorkerTooltipTrigger>();
                    tooltip.employeeData = emp;

                    spawnedAssignedCards.Add(tag);
                }
            }
        }

        if (synergyListParent && synergyTextPrefab && SynergyManager.Instance)
        {
            List<string> synergies = SynergyManager.Instance.GetActiveSynergyNames();
            if (synergies.Count == 0) CreateSynergyText("발동된 시너지가 없습니다.");
            else foreach (string s in synergies) CreateSynergyText(s);
        }
    }

    private void CreateSynergyText(string content)
    {
        GameObject obj = Instantiate(synergyTextPrefab, synergyListParent);
        TextMeshProUGUI tmp = obj.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp) tmp.text = content;
        spawnedSynergyTexts.Add(obj);
    }

    // --- 팝업 로직 ---
    public void OpenRolePopup(EmployeeInstance employee)
    {
        selectedEmployeeForRole = employee;
        if (roleSelectPopup) roleSelectPopup.SetActive(true);
    }
    public void CloseRolePopup()
    {
        selectedEmployeeForRole = null;
        if (roleSelectPopup) roleSelectPopup.SetActive(false);
    }
    public void ChangeRole(EmployeeRole newRole)
    {
        if (selectedEmployeeForRole != null)
        {
            selectedEmployeeForRole.assignedRole = newRole;
            if (SynergyManager.Instance) SynergyManager.Instance.UpdateActiveSynergies(EmployeeManager.Instance.hiredEmployees);
            UpdateAssignmentUI();
            CloseRolePopup();
        }
    }

    // --- UI 업데이트 함수들 (Applicant, Hired) ---
    public void UpdateApplicantListUI(List<GeneratedApplicant> applicants)
    {
        foreach (GameObject c in spawnedApplicantCards) Destroy(c);
        spawnedApplicantCards.Clear();
        if (!applicantCardPrefab || !applicantCardParent) return;
        foreach (GeneratedApplicant app in applicants)
        {
            if (app == null) continue;
            GameObject newCard = Instantiate(applicantCardPrefab, applicantCardParent);
            UpdateApplicantCardUI(newCard, app);
            spawnedApplicantCards.Add(newCard);
        }
    }

    private void UpdateApplicantCardUI(GameObject card, GeneratedApplicant applicant)
    {
        Image portrait = card.transform.Find("PortraitImage")?.GetComponent<Image>();
        TextMeshProUGUI name = card.transform.Find("Text/Name/NameText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI species = card.transform.Find("Text/Species/SpeciesText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI grade = card.transform.Find("Text/Grade/GradeText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI trait = card.transform.Find("Text/Trait/TraitText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI cook = card.transform.Find("CookStatText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI serve = card.transform.Find("ServeStatText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI charm = card.transform.Find("CharmStatText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI cost = card.transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
        if (!cost) cost = card.transform.Find("Text/Salary/SalaryText")?.GetComponent<TextMeshProUGUI>();
        Button hireBtn = card.transform.Find("HireButton")?.GetComponent<Button>();

        if (portrait && applicant.BaseSpeciesData.portrait) portrait.sprite = applicant.BaseSpeciesData.portrait;
        if (name) name.text = applicant.GeneratedFirstName;
        if (species) species.text = GetKoreanSpeciesName(applicant.BaseSpeciesData.speciesName);
        if (grade) grade.text = $"<color={GetGradeColorHex(applicant.grade)}>{applicant.grade}</color>";
        if (cook) cook.text = $"{applicant.GeneratedCookingStat}";
        if (serve) serve.text = $"{applicant.GeneratedServingStat}";
        if (charm) charm.text = $"{applicant.GeneratedCharmStat}";
        if (trait) trait.text = (applicant.GeneratedTraits.Count > 0) ? string.Join(", ", applicant.GeneratedTraits.Select(t => t.traitName)) : "-";

        int price = applicant.BaseSpeciesData.salary;
        if (cost) { cost.text = $"{price} G"; cost.color = (GameManager.instance.totalGoldAmount >= price) ? Color.white : Color.red; }
        if (hireBtn)
        {
            hireBtn.interactable = (GameManager.instance.totalGoldAmount >= price);
            hireBtn.onClick.RemoveAllListeners();
            hireBtn.onClick.AddListener(() => {
                if (EmployeeManager.Instance)
                {
                    EmployeeManager.Instance.HireEmployee(applicant);
                    UpdateApplicantListUI(EmployeeManager.Instance.applicants);
                }
            });
        }
    }

    public void UpdateHiredEmployeeListUI()
    {
        foreach (GameObject c in spawnedHiredCards) Destroy(c);
        spawnedHiredCards.Clear();
        if (!hiredCardPrefab || !hiredCardParent || !EmployeeManager.Instance) return;
        foreach (EmployeeInstance emp in EmployeeManager.Instance.hiredEmployees)
        {
            if (emp == null) continue;
            GameObject newCard = Instantiate(hiredCardPrefab, hiredCardParent);
            UpdateHiredCardUI(newCard, emp);
            spawnedHiredCards.Add(newCard);
        }
    }

    private void UpdateHiredCardUI(GameObject card, EmployeeInstance employee)
    {
        Image portrait = card.transform.Find("PortraitImage")?.GetComponent<Image>();
        TextMeshProUGUI name = card.transform.Find("Text/Name/NameText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI species = card.transform.Find("Text/Species/SpeciesText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI grade = card.transform.Find("Text/Grade/GradeText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI trait = card.transform.Find("Text/Trait/TraitText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI cook = card.transform.Find("CookStatText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI serve = card.transform.Find("ServeStatText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI charm = card.transform.Find("CharmStatText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI level = card.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI sp = card.transform.Find("SPText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI salary = card.transform.Find("Text/Salary/SalaryText")?.GetComponent<TextMeshProUGUI>();

        Button cookBtn = card.transform.Find("CookUpButton")?.GetComponent<Button>();
        Button serveBtn = card.transform.Find("ServeUpButton")?.GetComponent<Button>();
        Button charmBtn = card.transform.Find("CharmUpButton")?.GetComponent<Button>();
        Button levelBtn = card.transform.Find("LevelUpButton")?.GetComponent<Button>();
        Button dismissBtn = card.transform.Find("DismissButton")?.GetComponent<Button>();

        if (portrait && employee.BaseData.portrait) portrait.sprite = employee.BaseData.portrait;
        if (name) name.text = employee.firstName;
        if (species) species.text = GetKoreanSpeciesName(employee.BaseData.speciesName);
        if (level) level.text = $"Lv.{employee.currentLevel}";
        else if (name) name.text += $" (Lv.{employee.currentLevel})";
        if (sp) sp.text = (employee.skillPoints > 0) ? $"SP: <color=yellow>{employee.skillPoints}</color>" : "SP: 0";
        if (grade) grade.text = $"<color={GetGradeColorHex(employee.grade)}>{employee.grade}</color>";
        if (cook) cook.text = $"{employee.currentCookingStat}";
        if (serve) serve.text = $"{employee.currentServingStat}";
        if (charm) charm.text = $"{employee.currentCharmStat}";
        if (trait) trait.text = (employee.currentTraits.Count > 0) ? string.Join(", ", employee.currentTraits.Select(t => t.traitName)) : "-";
        if (salary) { salary.text = $"{employee.currentSalary} G"; salary.color = Color.white; }

        bool hasSP = employee.skillPoints > 0;
        if (cookBtn) { cookBtn.interactable = hasSP; cookBtn.onClick.RemoveAllListeners(); cookBtn.onClick.AddListener(() => { if (employee.SpendSkillPointOnCooking()) UpdateHiredEmployeeListUI(); }); }
        if (serveBtn) { serveBtn.interactable = hasSP; serveBtn.onClick.RemoveAllListeners(); serveBtn.onClick.AddListener(() => { if (employee.SpendSkillPointOnServing()) UpdateHiredEmployeeListUI(); }); }
        if (charmBtn) { charmBtn.interactable = hasSP; charmBtn.onClick.RemoveAllListeners(); charmBtn.onClick.AddListener(() => { if (employee.SpendSkillPointOnCharm()) UpdateHiredEmployeeListUI(); }); }

        if (levelBtn)
        {
            int maxLv = 20; // 등급별 로직 생략
            int cost = (int)(100 * Mathf.Pow(1.1f, employee.currentLevel - 1));
            TextMeshProUGUI txt = levelBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (employee.currentLevel >= maxLv) { levelBtn.interactable = false; if (txt) txt.text = "MAX"; }
            else
            {
                levelBtn.interactable = (GameManager.instance.totalGoldAmount >= cost);
                if (txt) txt.text = $"UP {cost}G";
                levelBtn.onClick.RemoveAllListeners();
                levelBtn.onClick.AddListener(() => { if (employee.TryLevelUp()) UpdateHiredEmployeeListUI(); });
            }
        }

        if (dismissBtn)
        {
            dismissBtn.onClick.RemoveAllListeners();
            if (employee.isProtagonist) dismissBtn.interactable = false;
            else { dismissBtn.interactable = true; dismissBtn.onClick.AddListener(() => ShowDismissalConfirmation(employee)); }
        }
    }

    private string GetKoreanSpeciesName(string s) { return (s == "Elf") ? "엘프" : (s == "Dwarf") ? "드워프" : (s == "Goblin") ? "고블린" : s; }
    private string GetGradeColorHex(EmployeeGrade g) { return (g == EmployeeGrade.S) ? "#FFD700" : (g == EmployeeGrade.A) ? "#9370DB" : (g == EmployeeGrade.B) ? "#1E90FF" : "#FFFFFF"; }

    public void ShowDismissalConfirmation(EmployeeInstance e)
    {
        employeeToDismiss = e;
        if (dismissalConfirmationPanel) { dismissalConfirmationPanel.SetActive(true); if (dismissalNameText) dismissalNameText.text = $"{e.firstName} 해고?"; }
    }
    public void ConfirmDismissal()
    {
        if (employeeToDismiss != null) EmployeeManager.Instance.DismissEmployee(employeeToDismiss);
        HideDismissalConfirmation(); UpdateHiredEmployeeListUI();
    }
    public void HideDismissalConfirmation() { employeeToDismiss = null; if (dismissalConfirmationPanel) dismissalConfirmationPanel.SetActive(false); }
}
