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
    public Button hireTabButton;
    public Button manageTabButton;
    public Button recipeTabButton; // [핵심 추가 1] 레시피 탭 버튼을 위한 변수

    [Header("컨텐츠 패널")]
    public GameObject applicantListPanel;
    public GameObject manageEmployeePanel;
    public GameObject recipeBookPanel; // [핵심 추가 2] 레시피 패널을 위한 변수

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
    private bool isUIVisible = false;

    void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }
    }

    void Start()
    {
        if (hireTabButton != null) hireTabButton.onClick.AddListener(() => OpenTab(applicantListPanel, hireTabButton));
        if (manageTabButton != null) manageTabButton.onClick.AddListener(() => OpenTab(manageEmployeePanel, manageTabButton));
        if (recipeTabButton != null) recipeTabButton.onClick.AddListener(() => OpenTab(recipeBookPanel, recipeTabButton)); // [핵심 추가 3] 레시피 버튼 기능 연결

        isUIVisible = false;
        if (managementUIParent != null) managementUIParent.SetActive(isUIVisible);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            isUIVisible = !isUIVisible;
            if (managementUIParent != null) managementUIParent.SetActive(isUIVisible);
            if (isUIVisible)
            {
                if (applicantListPanel != null) applicantListPanel.SetActive(false);
                if (manageEmployeePanel != null) manageEmployeePanel.SetActive(false);
                if (recipeBookPanel != null) recipeBookPanel.SetActive(false); // [핵심 추가 4] UI 켤 때 레시피 패널도 끄기

                Image hireBtnImage = hireTabButton?.GetComponent<Image>();
                if (hireBtnImage != null) hireBtnImage.color = normalTabColor;
                Image manageBtnImage = manageTabButton?.GetComponent<Image>();
                if (manageBtnImage != null) manageBtnImage.color = normalTabColor;
                Image recipeBtnImage = recipeTabButton?.GetComponent<Image>(); // [핵심 추가 5] 레시피 버튼 색상 초기화
                if (recipeBtnImage != null) recipeBtnImage.color = normalTabColor;
            }
        }
    }

    /// <summary>
    /// 지정된 컨텐츠 패널을 열고, 클릭된 탭 버튼의 색상을 변경합니다.
    /// </summary>
    void OpenTab(GameObject panelToShow, Button clickedButton)
    {
        if (applicantListPanel != null) applicantListPanel.SetActive(false);
        if (manageEmployeePanel != null) manageEmployeePanel.SetActive(false);
        if (recipeBookPanel != null) recipeBookPanel.SetActive(false); // [핵심 추가 6] 탭 열 때 레시피 패널도 끄기

        if (panelToShow != null) panelToShow.SetActive(true);

        Image hireBtnImage = hireTabButton?.GetComponent<Image>();
        if (hireBtnImage != null) hireBtnImage.color = normalTabColor;
        Image manageBtnImage = manageTabButton?.GetComponent<Image>();
        if (manageBtnImage != null) manageBtnImage.color = normalTabColor;
        Image recipeBtnImage = recipeTabButton?.GetComponent<Image>(); // [핵심 추가 7] 레시피 버튼 색상 초기화
        if (recipeBtnImage != null) recipeBtnImage.color = normalTabColor;

        Image clickedBtnImage = clickedButton?.GetComponent<Image>();
        if (clickedBtnImage != null) clickedBtnImage.color = activeTabColor;

        if (panelToShow == manageEmployeePanel)
        {
            UpdateHiredEmployeeListUI();
        }
        // '레시피' 탭을 열 때는 RecipeBookUI가 OnEnable에서 스스로 새로고침하므로 별도 호출이 필요 없습니다.
    }

    // --- 이하 모든 함수는 보내주신 코드와 동일하게 유지됩니다 ---

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

