using UnityEngine;
using UnityEngine.UI; // Image 사용
using TMPro; // TextMeshProUGUI 사용
using System.Linq; // .Count() 사용
using System.Collections.Generic; // Dictionary 사용

/// <summary>
/// 레시피 도감 UI의 모든 기능을 관리합니다.
/// (수집 현황 표시, 보유 레시피 목록 표시, 상세 정보 패널 표시, 열기/닫기)
/// </summary>
public class RecipeBook_UI : MonoBehaviour
{
    [Header("List Panel (왼쪽 목록)")]
    [Tooltip("수집 현황 텍스트 (예: 4 / 10 개)")]
    [SerializeField] private TextMeshProUGUI collectionStatusText;

    [Tooltip("레시피 버튼 프리팹이 생성될 스크롤 뷰의 Content")]
    [SerializeField] private Transform recipeContentParent;

    [Tooltip("목록에 사용할 '텍스트 버튼' 프리팹 (RecipeListButton 스크립트가 붙어있어야 함)")]
    [SerializeField] private GameObject recipeListButtonPrefab; // ★ 1순위 연결 대상

    [Header("Detail Panel (오른쪽 상세창)")]
    [Tooltip("상세 정보를 표시할 패널 (오른쪽)")]
    [SerializeField] private GameObject detailPanel; // ★ 2순위 연결 대상
    [SerializeField] private Image detailRecipeImage;
    [SerializeField] private TextMeshProUGUI detailRecipeNameText;
    [SerializeField] private TextMeshProUGUI detailRecipeDescriptionText;
    [SerializeField] private TextMeshProUGUI detailRecipeLevelText;
    // (TODO: 재료 목록, 강화 버튼 등)


    /// <summary>
    /// 이 UI 패널이 활성화될 때마다 도감 내용을 새로고침합니다.
    /// </summary>
    void OnEnable()
    {
        // 도감을 열 때마다 상세 패널은 숨김
        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }
        RefreshBook();
    }

    // --- ▼▼▼ 기능 추가 ▼▼▼ ---

    /// <summary>
    /// ESC 키 입력을 감지하기 위해 Update 함수를 사용합니다.
    /// </summary>
    void Update()
    {
        // 만약 ESC 키가 눌렸다면
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 닫기 함수를 호출합니다.
            ClosePanel();
        }
    }

    /// <summary>
    /// (외부의 '도감 열기' 버튼이 호출할 함수)
    /// 이 패널을 활성화합니다.
    /// </summary>
    public void OpenPanel()
    {
        gameObject.SetActive(true);
        // OnEnable()이 자동으로 호출되면서 RefreshBook()도 실행됩니다.
    }

    /// <summary>
    /// (ESC 키 또는 '닫기' 버튼이 호출할 함수)
    /// 이 패널을 비활성화합니다.
    /// </summary>
    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    // --- ▲▲▲ 기능 추가 완료 ▲▲▲ ---


    /// <summary>
    /// 레시피 도감의 모든 내용을 새로고침합니다.
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
            // 5. 새 프리팹(recipeListButtonPrefab)을 Content 자식으로 생성
            GameObject itemGO = Instantiate(recipeListButtonPrefab, recipeContentParent);

            // 6. 새 스크립트(RecipeListButton)를 찾음
            RecipeListButton itemUI = itemGO.GetComponent<RecipeListButton>();

            if (itemUI != null)
            {
                // 7. 새 스크립트의 Setup 함수 호출
                itemUI.Setup(recipe, this);
            }
            else
            {
                Debug.LogError($"'{recipeListButtonPrefab.name}' 프리팹에 RecipeListButton 스크립트가 없습니다!", itemGO);
            }
        }
    }

    /// <summary>
    /// (RecipeListButton이 호출) 레시피 아이템 클릭 시 상세 정보 패널을 채우고 켭니다.
    /// </summary>
    public void ShowRecipeDetails(int recipeID)
    {
        // 1. 매니저에서 ID에 해당하는 레시피 데이터 가져오기
        if (!RecipeManager.instance.playerRecipes.TryGetValue(recipeID, out PlayerRecipe playerRecipe))
        {
            Debug.LogError($"ID: {recipeID} 에 해당하는 레시피를 찾을 수 없습니다.");
            return;
        }

        RecipeData data = playerRecipe.data; // 원본 데이터

        // 2. 1단계에서 만든 상세창 UI 컴포넌트에 데이터 채우기
        if (detailPanel != null)
        {
            detailRecipeImage.sprite = data.icon;
            detailRecipeNameText.text = data.recipeName;
            detailRecipeDescriptionText.text = data.description;
            detailRecipeLevelText.text = $"현재 레벨: {playerRecipe.currentLevel}";

            // (TODO: 재료 목록 UI 채우기)

            // 3. 상세창 패널 활성화
            detailPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 수집 현황 텍스트를 업데이트합니다.
    /// </summary>
    private void UpdateCollectionStatus(int acquiredCount)
    {
        // GameDataManager에서 전체 레시피 개수를 가져옵니다.
        int totalCount = GameDataManager.instance.GetAllRecipeData().Count();

        // 텍스트 UI에 반영
        collectionStatusText.text = $"수집 현황: {acquiredCount} / {totalCount} 개";
    }
}