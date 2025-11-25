using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class RecipeBook_UI : MonoBehaviour
{
    [Header("Left 레시피 목록 UI")]
    [SerializeField] private TextMeshProUGUI collectionStatusText;
    [SerializeField] private Transform recipeContentParent;
    [SerializeField] private GameObject recipeListButtonPrefab;

    [Header("Right 레시피 상세 UI")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private Image r_DetailImage;
    [SerializeField] private TextMeshProUGUI r_Name;
    [SerializeField] private TextMeshProUGUI r_Desc;

    [Header("상세 정보 텍스트 및 별점")]
    [SerializeField] private TextMeshProUGUI r_Info;
    [SerializeField] private Image[] r_StarImages;
    [SerializeField] private Sprite starEmptySprite;
    [SerializeField] private Sprite starFullSprite;

    [Header("재료 아이콘 및 강화")]
    [SerializeField] private Transform r_NeedIngredientGrid;
    [SerializeField] private GameObject simpleIconPrefab;
    [SerializeField] private Button enhanceButton;
    [SerializeField] private TextMeshProUGUI nextUpgradeInfoText;

    // 툴팁
    [Header("강화 툴팁 설정")]
    [SerializeField] private GameObject upgradeInfoPanel;
    [SerializeField] private TextMeshProUGUI tooltipText; // (Inspector 연결 확인!)

    private string currentTooltipMessage;
    private int currentRecipeID;

    void Start()
    {
        if (enhanceButton != null)
            enhanceButton.onClick.AddListener(OnEnhanceButtonClick);

        if (RecipeManager.instance != null)
        {
            RecipeManager.instance.onRecipeUpdated += RefreshBook;
        }

        if (detailPanel != null) detailPanel.SetActive(false);
        if (upgradeInfoPanel != null) upgradeInfoPanel.SetActive(false);

        RefreshBook();
    }

    void OnDestroy()
    {
        if (RecipeManager.instance != null)
        {
            RecipeManager.instance.onRecipeUpdated -= RefreshBook;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) ClosePanel();
    }

    public void OpenPanel()
    {
        gameObject.SetActive(true);
        if (detailPanel != null) detailPanel.SetActive(false);
        if (upgradeInfoPanel != null) upgradeInfoPanel.SetActive(false);
        RefreshBook();
    }

    public void ClosePanel()
    {
        if (GameManager.instance != null) GameManager.instance.CloseRecipeBook();
        else gameObject.SetActive(false);
    }

    // ▼▼▼ [수정됨] 보유 목록이 아니라 '전체 목록'을 순회함 ▼▼▼
    public void RefreshBook()
    {
        foreach (Transform child in recipeContentParent) Destroy(child.gameObject);

        // 1. 전체 데이터 가져오기
        List<RecipeData> allRecipes = GameDataManager.instance.GetAllRecipeData();
        // 2. 내가 가진 레시피 목록 가져오기 (ID만 추출)
        var ownedRecipeIDs = RecipeManager.instance.playerRecipes.Keys;

        // 수집 현황 업데이트
        int totalCount = allRecipes.Count;
        int ownedCount = ownedRecipeIDs.Count;
        collectionStatusText.text = $"수집 현황: {ownedCount} / {totalCount}";

        // 3. 전체 목록 생성
        foreach (var data in allRecipes)
        {
            GameObject go = Instantiate(recipeListButtonPrefab, recipeContentParent);
            RecipeListButton btn = go.GetComponent<RecipeListButton>();

            // 보유 여부 체크
            bool isOwned = RecipeManager.instance.playerRecipes.ContainsKey(data.id);

            // 버튼 세팅 (데이터, 보유여부, 컨트롤러)
            btn.Setup(data, isOwned, this);
        }
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    public void ShowRecipeDetails(int recipeID)
    {
        currentRecipeID = recipeID;

        // 데이터 가져오기
        RecipeData data = GameDataManager.instance.GetRecipeDataById(recipeID);
        if (data == null) return;

        // 보유 여부 확인
        bool isOwned = RecipeManager.instance.playerRecipes.TryGetValue(recipeID, out PlayerRecipe playerRecipe);

        if (detailPanel != null) detailPanel.SetActive(true);

        // 1. 이미지/이름/설명 (미보유라도 보여줌)
        r_DetailImage.sprite = data.fullImage != null ? data.fullImage : data.icon;
        r_Name.text = data.recipeName;

        // 2. 보유 여부에 따른 분기 처리
        if (isOwned)
        {
            r_DetailImage.color = Color.white; // 밝게
            r_Desc.text = data.description;

            // 레벨/가격 계산
            int currentLevel = playerRecipe.currentLevel;
            int currentPrice = RecipeManager.instance.GetRecipeSellingPrice(recipeID);

            r_Info.text = $"음식 레벨 : LV.{currentLevel}\n" +
                          $"판매 가격 : {currentPrice} 골드\n" +
                          $"요리 시간 : {data.baseCookTime} 초";

            // 별점 표시
            ShowStars(currentLevel);

            // 강화 버튼 활성화
            enhanceButton.interactable = true;
            enhanceButton.GetComponentInChildren<TextMeshProUGUI>().text = "강 화";

            // 툴팁 계산
            CalculateNextUpgradeInfo(playerRecipe);
        }
        else
        {
            // [미보유 상태]
            r_DetailImage.color = Color.black; // 실루엣 처리 (선택)
            r_Desc.text = "아직 획득하지 못한 레시피입니다.";
            r_Info.text = "???";

            // 별점 끄기
            foreach (var star in r_StarImages) star.gameObject.SetActive(false);

            // 강화 버튼 비활성화 및 텍스트 변경
            enhanceButton.interactable = false;
            enhanceButton.GetComponentInChildren<TextMeshProUGUI>().text = "미획득";

            currentTooltipMessage = "상점이나 퀘스트를 통해 획득하세요.";
        }

        // 3. 재료 아이콘 (미보유라도 필요 재료는 보여줌 - 힌트용)
        // (만약 미보유 시 숨기고 싶다면 if(isOwned) 안으로 넣으세요)
        foreach (Transform child in r_NeedIngredientGrid) Destroy(child.gameObject);
        foreach (var req in data.requiredIngredients)
        {
            GameObject iconObj = Instantiate(simpleIconPrefab, r_NeedIngredientGrid);
            IngredientData ingData = GameDataManager.instance.GetIngredientDataById(req.ingredientID);
            if (ingData != null)
            {
                iconObj.GetComponent<Image>().sprite = ingData.icon;
                // 미보유 시엔 기본 수량만 표시
                int amount = req.amount;
                if (isOwned)
                {
                    RecipeLevelEntry entry = GameDataManager.instance.GetRecipeLevelData(playerRecipe.currentLevel);
                    // (여기서 현재 레벨 배수 적용 가능)
                }
                iconObj.GetComponentInChildren<TextMeshProUGUI>().text = $"x{amount}";
            }
        }
    }

    void ShowStars(int level)
    {
        foreach (var star in r_StarImages) star.gameObject.SetActive(true);

        int starCount = 1;
        if (level >= 40) starCount = 5;
        else if (level >= 30) starCount = 4;
        else if (level >= 20) starCount = 3;
        else if (level >= 10) starCount = 2;

        for (int i = 0; i < r_StarImages.Length; i++)
        {
            r_StarImages[i].sprite = (i < starCount) ? starFullSprite : starEmptySprite;
        }
    }

    public void ShowTooltip(bool isShow)
    {
        if (upgradeInfoPanel != null)
        {
            upgradeInfoPanel.SetActive(isShow);
            if (isShow && tooltipText != null)
            {
                tooltipText.text = currentTooltipMessage;
            }
        }
    }

    void OnEnhanceButtonClick()
    {
        // 보유 중일 때만 강화 시도
        if (RecipeManager.instance.playerRecipes.ContainsKey(currentRecipeID))
        {
            if (RecipeManager.instance.UpgradeRecipe(currentRecipeID))
            {
                ShowRecipeDetails(currentRecipeID);
                RefreshBook();
                if (upgradeInfoPanel.activeSelf && tooltipText != null)
                {
                    tooltipText.text = currentTooltipMessage;
                }
            }
        }
    }

    void CalculateNextUpgradeInfo(PlayerRecipe recipe)
    {
        int currentLv = recipe.currentLevel;
        int nextLv = currentLv + 1;
        RecipeLevelEntry entry = GameDataManager.instance.GetRecipeLevelData(nextLv);

        if (entry == null)
        {
            currentTooltipMessage = "최대 레벨 (Max)";
            enhanceButton.interactable = false;
        }
        else
        {
            currentTooltipMessage = $"요리 레벨 {currentLv} -> {nextLv}\n" +
                                    $"요구 골드 : {entry.Required_Gold} G\n" +
                                    $"성공 확률 : {entry.SuccessRate}%";
            enhanceButton.interactable = true;
        }
    }
}