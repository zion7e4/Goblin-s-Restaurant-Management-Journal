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

    [Header("메인 UI 패널")]
    public GameObject managementUIParent;
    [Header("탭 UI 요소")]
    public Button Button_OpenHirePanel;
    public Button Button_OpenManagePanel;
    public Button recipeTabButton;

    [Header("컨텐츠 패널")]
    public GameObject applicantListPanel;
    public GameObject manageEmployeePanel;
    public GameObject recipeBookPanel;

    [Header("카드 프리팹")]
    public GameObject applicantCardPrefab;
    public GameObject hiredCardPrefab;
    [Header("카드 생성 위치")]
    public Transform applicantCardParent;
    public Transform hiredCardParent;
    [Header("탭 시각 효과")]
    public Color normalTabColor = Color.white;
    public Color activeTabColor = new Color(0.8f, 0.9f, 1f);

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
        // 각 버튼에 OpenTab 기능 연결
        if (Button_OpenHirePanel != null) Button_OpenHirePanel.onClick.AddListener(() => OpenTab(applicantListPanel, Button_OpenHirePanel));
        if (Button_OpenManagePanel != null) Button_OpenManagePanel.onClick.AddListener(() => OpenTab(manageEmployeePanel, Button_OpenManagePanel));
        if (recipeTabButton != null) recipeTabButton.onClick.AddListener(() => OpenTab(recipeBookPanel, recipeTabButton));

        // 게임 시작 시 UI 끄기
        isUIVisible = false;
        if (managementUIParent != null) managementUIParent.SetActive(isUIVisible);
    }

    // [수정 1] Update 함수의 Tab키 로직을 Esc키 로직으로 변경
    void Update()
    {
        // Tab 키 관련 로직을 모두 제거합니다.

        // 대신, UI가 켜져 있을 때 'Escape' 키를 누르면 UI가 닫히도록 변경합니다. (선택 사항)
        if (isUIVisible && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            isUIVisible = false;
            if (managementUIParent != null) managementUIParent.SetActive(isUIVisible);
        }
    }

    /// <summary>
    /// [수정 2] OpenTab 함수가 메인 패널(managementUIParent)의 활성화/비활성화를 직접 제어합니다.
    /// </summary>
    void OpenTab(GameObject panelToShow, Button clickedButton)
    {
        // --- 1. 닫기 로직: UI가 켜져있고, 이미 활성화된 탭을 다시 눌렀는지 확인 ---
        if (isUIVisible && panelToShow != null && panelToShow.activeSelf)
        {
            // 이미 열린 탭을 다시 눌렀으므로, 메인 UI 전체를 닫습니다.
            isUIVisible = false;
            if (managementUIParent != null) managementUIParent.SetActive(false);
            return; // 함수 종료
        }

        // --- 2. 열기 또는 탭 전환 로직 ---

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

        // 2d. 모든 탭 버튼 색상을 '일반'으로 초기화합니다.
        Image hireBtnImage = Button_OpenHirePanel?.GetComponent<Image>();
        if (hireBtnImage != null) hireBtnImage.color = normalTabColor;
        Image manageBtnImage = Button_OpenManagePanel?.GetComponent<Image>();
        if (manageBtnImage != null) manageBtnImage.color = normalTabColor;
        Image recipeBtnImage = recipeTabButton?.GetComponent<Image>();
        if (recipeBtnImage != null) recipeBtnImage.color = normalTabColor;

        // 2e. 클릭된 탭 버튼만 '활성' 색상으로 변경합니다.
        Image clickedBtnImage = clickedButton?.GetComponent<Image>();
        if (clickedBtnImage != null) clickedBtnImage.color = activeTabColor;

        // 2f. '직원 관리' 탭을 열었다면 목록을 새로고침합니다.
        if (panelToShow == manageEmployeePanel)
        {
            UpdateHiredEmployeeListUI();
        }
    }

    // --- 이하 함수들은 수정할 필요 없이 동일하게 유지됩니다 ---

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
        foreach (EmployeeInstance employee in EmployeeManager.Instance.hiredEmployees)
        {
            if (employee == null) continue;
            GameObject newCard = Instantiate(hiredCardPrefab, hiredCardParent);
            UpdateHiredCardUI(newCard, employee);
            spawnedHiredCards.Add(newCard);
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
            hireButton.onClick.AddListener(() => EmployeeManager.Instance.HireEmployee(applicant));
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
        if (cleanUpBtn != null)
        {
            cleanUpBtn.interactable = employee.skillPoints > 0;
            cleanUpBtn.onClick.RemoveAllListeners();
            cleanUpBtn.onClick.AddListener(() => { if (employee.SpendSkillPointOnCleaning()) UpdateHiredEmployeeListUI(); });
        }
    }
}