using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("메인 UI 패널")]
    public GameObject managementUIParent;

    [Header("탭 UI 요소")]
    public Button hireTabButton;
    public Button manageTabButton;

    [Header("컨텐츠 패널")]
    public GameObject applicantListPanel;
    public GameObject manageEmployeePanel;

    [Header("지원자 목록 UI")]
    public Transform applicantCardParent;
    public GameObject applicantCardPrefab;

    [Header("레이아웃 조정")]
    [Tooltip("지원자 목록의 기본 왼쪽 여백")]
    public int normalLeftPadding = 20;
    [Tooltip("지원자가 18명일 때 적용될 좁은 왼쪽 여백")]
    public int narrowLeftPadding = 5;
    [Tooltip("화면에 표시할 최대 지원자 수")]
    public int maxApplicantsToShow = 18;

    [Header("탭 시각 효과")]
    public Color normalTabColor = Color.white;
    public Color activeTabColor = new Color(0.8f, 0.9f, 1f);

    private List<GameObject> spawnedApplicantCards = new List<GameObject>();
    private bool isUIVisible = false;
    private GridLayoutGroup applicantGrid; // 그리드 레이아웃 컴포넌트를 저장할 변수

    void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }

        // applicantCardParent에서 GridLayoutGroup 컴포넌트를 미리 찾아둡니다.
        if (applicantCardParent != null)
        {
            applicantGrid = applicantCardParent.GetComponent<GridLayoutGroup>();
        }
    }

    void Start()
    {
        if (hireTabButton != null) hireTabButton.onClick.AddListener(() => OpenTab(applicantListPanel, hireTabButton));
        if (manageTabButton != null) manageTabButton.onClick.AddListener(() => OpenTab(manageEmployeePanel, manageTabButton));

        // 시작 시 UI를 완전히 끈 상태로 시작합니다.
        isUIVisible = false;
        if (managementUIParent != null) managementUIParent.SetActive(isUIVisible);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            isUIVisible = !isUIVisible;
            if (managementUIParent != null) managementUIParent.SetActive(isUIVisible);

            // UI를 켤 때, 모든 컨텐츠 패널을 끄고 탭 색상을 초기화합니다.
            if (isUIVisible)
            {
                if (applicantListPanel != null) applicantListPanel.SetActive(false);
                if (manageEmployeePanel != null) manageEmployeePanel.SetActive(false);
                if (hireTabButton != null) hireTabButton.GetComponent<Image>().color = normalTabColor;
                if (manageTabButton != null) manageTabButton.GetComponent<Image>().color = normalTabColor;
            }

            Debug.Log($"Tab 키 눌림. UI 표시 상태: {isUIVisible}");
        }
    }

    void OpenTab(GameObject panelToShow, Button clickedButton)
    {
        if (applicantListPanel != null) applicantListPanel.SetActive(false);
        if (manageEmployeePanel != null) manageEmployeePanel.SetActive(false);
        if (panelToShow != null) panelToShow.SetActive(true);

        if (hireTabButton != null) hireTabButton.GetComponent<Image>().color = normalTabColor;
        if (manageTabButton != null) manageTabButton.GetComponent<Image>().color = normalTabColor;
        if (clickedButton != null) clickedButton.GetComponent<Image>().color = activeTabColor;
    }

    public void UpdateApplicantListUI(List<GeneratedApplicant> applicants)
    {
        // 지원자 수에 따라 왼쪽 여백을 조정합니다.
        if (applicantGrid != null)
        {
            // Take(maxApplicantsToShow).Count()를 사용해 실제 표시될 지원자 수를 기준으로 판단합니다.
            if (applicants.Take(maxApplicantsToShow).Count() >= maxApplicantsToShow)
            {
                applicantGrid.padding.left = narrowLeftPadding;
            }
            else
            {
                applicantGrid.padding.left = normalLeftPadding;
            }
        }

        foreach (GameObject card in spawnedApplicantCards) { Destroy(card); }
        spawnedApplicantCards.Clear();

        // applicants 리스트에서 최대 maxApplicantsToShow 개수까지만 가져와서 UI를 생성합니다.
        foreach (GeneratedApplicant applicant in applicants.Take(maxApplicantsToShow))
        {
            GameObject newCard = Instantiate(applicantCardPrefab, applicantCardParent);
            UpdateCardUI(newCard, applicant);
            spawnedApplicantCards.Add(newCard);
        }
    }

    private void UpdateCardUI(GameObject card, GeneratedApplicant applicant)
    {
        Image portraitImage = card.transform.Find("PortraitImage")?.GetComponent<Image>();
        TextMeshProUGUI nameText = card.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI statsText = card.transform.Find("StatsText")?.GetComponent<TextMeshProUGUI>();
        Button hireButton = card.transform.Find("HireButton")?.GetComponent<Button>();

        if (portraitImage != null) portraitImage.sprite = applicant.BaseSpeciesData.portrait;
        if (nameText != null) nameText.text = $"{applicant.GeneratedFirstName}\n<size=20>({applicant.BaseSpeciesData.speciesName})</size>";

        if (statsText != null)
        {
            var statsBuilder = new System.Text.StringBuilder();
            statsBuilder.AppendLine($"요리: {applicant.GeneratedCookingStat}");
            statsBuilder.AppendLine($"서빙: {applicant.GeneratedServingStat}");
            statsBuilder.AppendLine($"정리: {applicant.GeneratedCleaningStat}");
            if (applicant.GeneratedTraits.Any()) { statsBuilder.AppendLine($"\n특성: <color=yellow>{applicant.GeneratedTraits[0].traitName}</color>"); }
            statsText.text = statsBuilder.ToString();
        }
        if (hireButton != null)
        {
            hireButton.onClick.RemoveAllListeners();
            hireButton.onClick.AddListener(() => EmployeeManager.Instance.HireEmployee(applicant));
        }
    }
}
