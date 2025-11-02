using UnityEngine;
using UnityEngine.UI; // Button, Image 사용
using TMPro; // TextMeshProUGUI 사용
using System.Linq; // .Count() 사용
using System.Collections.Generic; // Dictionary 사용

/// <summary>
/// 레시피 도감 UI의 모든 기능을 관리합니다.
/// (수집 현황 표시, 보유 레시피 목록 표시, 상세 정보 패널 표시, 열기/닫기, 강화)
/// </summary>
public class RecipeBook_UI : MonoBehaviour
{
    [Header("List Panel (왼쪽 목록)")]
    [Tooltip("수집 현황 텍스트 (예: 4 / 10 개)")]
    [SerializeField] private TextMeshProUGUI collectionStatusText;

    [Tooltip("레시피 버튼 프리팹이 생성될 스크롤 뷰의 Content")]
    [SerializeField] private Transform recipeContentParent;

    [Tooltip("목록에 사용할 '텍스트 버튼' 프리팹 (RecipeListButton 스크립트가 붙어있어야 함)")]
    [SerializeField] private GameObject recipeListButtonPrefab;

    [Header("Detail Panel (오른쪽 상세창)")]
    [Tooltip("상세 정보를 표시할 패널 (오른쪽)")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private Image detailRecipeImage;
    [SerializeField] private TextMeshProUGUI detailRecipeNameText;
    [SerializeField] private TextMeshProUGUI detailRecipeDescriptionText;
    [SerializeField] private TextMeshProUGUI detailRecipeLevelText;
    [SerializeField] private Button enhanceButton; // 강화 버튼

    /// <summary>
    /// 현재 상세창에서 보고 있는 레시피의 ID
    /// </summary>
    private int currentRecipeID;

    /// <summary>
    /// UI가 켜질 때마다 목록을 새로고침하고 상세창을 숨깁니다.
    /// </summary>
    void OnEnable()
    {
        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }
        RefreshBook();
    }


    /// <summary>
    /// 게임 시작 시 강화 버튼에 클릭 이벤트를 연결합니다.
    /// </summary>
    void Start()
    {
        if (enhanceButton != null)
        {
            enhanceButton.onClick.AddListener(OnEnhanceButtonClick);
        }
    }

    /// <summary>
    /// ESC 키 입력을 감지하여 패널을 닫습니다.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePanel();
        }
    }

    /// <summary>
    /// (외부 '도감 열기' 버튼이 호출) 패널을 활성화합니다.
    /// </summary>
    public void OpenPanel()
    {
        gameObject.SetActive(true);
        // OnEnable()이 자동으로 호출됩니다.
    }

    /// <summary>
    /// (ESC 키 또는 '닫기' 버튼이 호출) 패널을 비활성화합니다.
    /// </summary>
    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// '강화' 버튼을 클릭했을 때 호출됩니다.
    /// </summary>
    void OnEnhanceButtonClick()
    {
        // ▼▼▼ 진단용 로그 1 ▼▼▼
        Debug.Log("--- [RecipeBook_UI] 강화 버튼 클릭됨 ---");

        // RecipeManager에게 현재 ID의 레시피 강화를 요청합니다.
        bool success = RecipeManager.instance.UpgradeRecipe(currentRecipeID);

        // ▼▼▼ 진단용 로그 2 ▼▼▼
        Debug.Log($"--- [RecipeBook_UI] 강화 시도 결과: {success} ---");

        // 강화에 성공했다면
        if (success)
        {
            // 상세 정보 UI를 새로고침 (레벨 표시 등)
            ShowRecipeDetails(currentRecipeID);

            // 목록 UI도 새로고침 (목록의 레벨 텍스트도 바뀔 수 있으므로)
            RefreshBook();
        }
    }

    /// <summary>
    /// 레시피 목록(왼쪽)을 새로고침합니다.
    /// </summary>
    public void RefreshBook()
    {
        // 1. 기존 리스트 삭제
        foreach (Transform child in recipeContentParent)
        {
            Destroy(child.gameObject);
        }

        // 2. 보유한 레시피 목록 가져오기
        var ownedRecipes = RecipeManager.instance.playerRecipes;
        if (ownedRecipes == null) return;

        // 3. 수집 현황 텍스트 업데이트
        UpdateCollectionStatus(ownedRecipes.Count);

        // 4. 레시피 목록 채우기
        foreach (PlayerRecipe recipe in ownedRecipes.Values)
        {
            GameObject itemGO = Instantiate(recipeListButtonPrefab, recipeContentParent);
            RecipeListButton itemUI = itemGO.GetComponent<RecipeListButton>();

            if (itemUI != null)
            {
                itemUI.Setup(recipe, this);
            }
            else
            {
                Debug.LogError($"'{recipeListButtonPrefab.name}' 프리팹에 RecipeListButton 스크립트가 없습니다!", itemGO);
            }
        }
    }

    /// <summary>
    /// (RecipeListButton이 호출) 상세 정보 패널(오른쪽)을 엽니다.
    /// </summary>
    public void ShowRecipeDetails(int recipeID)
    {
        // 1. 강화 버튼이 참조할 수 있도록 현재 ID를 저장
        currentRecipeID = recipeID;

        // 2. 매니저에서 레시피 데이터 가져오기
        if (!RecipeManager.instance.playerRecipes.TryGetValue(recipeID, out PlayerRecipe playerRecipe))
        {
            Debug.LogError($"ID: {recipeID} 에 해당하는 레시피를 찾을 수 없습니다.");
            return;
        }

        RecipeData data = playerRecipe.data; // 원본 데이터

        // 3. 상세창 UI 컴포넌트에 데이터 채우기
        if (detailPanel != null)
        {
            detailRecipeImage.sprite = data.icon;
            detailRecipeNameText.text = data.recipeName;
            detailRecipeDescriptionText.text = data.description;
            detailRecipeLevelText.text = $"현재 레벨: {playerRecipe.currentLevel}";

            // 4. 상세창 패널 활성화
            detailPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 수집 현황 텍스트를 업데이트합니다.
    /// </summary>
    private void UpdateCollectionStatus(int acquiredCount)
    {
        int totalCount = GameDataManager.instance.GetAllRecipeData().Count();
        collectionStatusText.text = $"수집 현황: {acquiredCount} / {totalCount} 개";
    }

}