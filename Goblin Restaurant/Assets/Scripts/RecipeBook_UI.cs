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

    // [수정됨] 툴팁 관련 변수
    [Header("강화 툴팁 설정")]
    [SerializeField] private GameObject upgradeInfoPanel; // 툴팁 배경 패널 (새로 만드신 UpgradeInfo)
    [SerializeField] private TextMeshProUGUI nextUpgradeInfoText; // 툴팁 안의 텍스트 (기존 변수 유지)

    private string currentTooltipMessage; // 툴팁에 띄울 메시지 저장용

    private int currentRecipeID;

    void Start()
    {
        if (enhanceButton != null)
            enhanceButton.onClick.AddListener(OnEnhanceButtonClick);

        if (RecipeManager.instance != null)
        {
            RecipeManager.instance.onRecipeUpdated += RefreshBook;
        }

        // 시작 시 상세창과 툴팁은 꺼둠
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

    // 도감 열기
    public void OpenPanel()
    {
        gameObject.SetActive(true);

        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }
        // 열 때 툴팁도 확실히 끄기
        if (upgradeInfoPanel != null) upgradeInfoPanel.SetActive(false);

        RefreshBook();
    }

    // 도감 닫기
    public void ClosePanel()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.CloseRecipeBook();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // 목록 갱신
    public void RefreshBook()
    {
        foreach (Transform child in recipeContentParent)
        {
            Destroy(child.gameObject);
        }

        var ownedRecipes = RecipeManager.instance.playerRecipes;
        if (ownedRecipes == null) return;

        int totalCount = GameDataManager.instance.GetAllRecipeData().Count;
        collectionStatusText.text = $"수집 현황: {ownedRecipes.Count} / {totalCount}";

        foreach (var recipe in ownedRecipes.Values)
        {
            GameObject go = Instantiate(recipeListButtonPrefab, recipeContentParent);
            go.GetComponent<RecipeListButton>().Setup(recipe, this);
        }
    }

    // 상세 정보 표시
    public void ShowRecipeDetails(int recipeID)
    {
        currentRecipeID = recipeID;

        if (!RecipeManager.instance.playerRecipes.TryGetValue(recipeID, out PlayerRecipe playerRecipe)) return;

        RecipeData data = playerRecipe.data;

        if (detailPanel != null) detailPanel.SetActive(true);

        r_DetailImage.sprite = data.fullImage != null ? data.fullImage : data.icon;
        r_Name.text = data.recipeName;
        r_Desc.text = data.description;

        // 현재 레벨 정보 표시
        int currentLevel = playerRecipe.currentLevel;
        int currentPrice = data.basePrice;
        int ingredientMultiplier = 1;

        RecipeLevelEntry levelData = GameDataManager.instance.GetRecipeLevelData(currentLevel);
        if (levelData != null)
        {
            currentPrice = (int)(data.basePrice * (1.0f + levelData.Price_Growth_Rate));
            ingredientMultiplier = levelData.Required_Item_Count;
        }

        r_Info.text = $"음식 레벨 : LV.{currentLevel}\n" +
                      $"판매 가격 : {currentPrice} 골드\n" +
                      $"요리 시간 : {data.baseCookTime} 초";

        // 별점 표시
        int starCount = 1;
        if (currentLevel >= 40) starCount = 5;
        else if (currentLevel >= 30) starCount = 4;
        else if (currentLevel >= 20) starCount = 3;
        else if (currentLevel >= 10) starCount = 2;

        for (int i = 0; i < r_StarImages.Length; i++)
        {
            if (i < starCount)
                r_StarImages[i].sprite = starFullSprite;
            else
                r_StarImages[i].sprite = starEmptySprite;
        }

        // 재료 아이콘 표시
        foreach (Transform child in r_NeedIngredientGrid) Destroy(child.gameObject);

        foreach (var req in data.requiredIngredients)
        {
            GameObject iconObj = Instantiate(simpleIconPrefab, r_NeedIngredientGrid);
            IngredientData ingData = GameDataManager.instance.GetIngredientDataById(req.ingredientID);
            if (ingData != null)
            {
                iconObj.GetComponent<Image>().sprite = ingData.icon;
                int finalAmount = req.amount * ingredientMultiplier;
                iconObj.GetComponentInChildren<TextMeshProUGUI>().text = $"x{finalAmount}";
            }
        }

        // 툴팁 내용 미리 계산
        CalculateNextUpgradeInfo(playerRecipe);
    }

    // [툴팁 기능] 외부 UIEventHandler에서 호출
    public void ShowTooltip(bool isShow)
    {
        Debug.Log($"[신호 수신] 툴팁 켜기: {isShow}");
        if (upgradeInfoPanel != null)
        {
            upgradeInfoPanel.SetActive(isShow);

            if (isShow && nextUpgradeInfoText != null)
            {
                nextUpgradeInfoText.text = currentTooltipMessage;
            }
        }
        else
        {
            Debug.LogError("오류: upgradeInfoPanel이 비어있습니다!");
        }
    }

    void OnEnhanceButtonClick()
    {
        if (RecipeManager.instance.UpgradeRecipe(currentRecipeID))
        {
            ShowRecipeDetails(currentRecipeID);
            RefreshBook();

            // 강화 성공 후 마우스가 위에 있다면 툴팁 내용 즉시 갱신
            if (upgradeInfoPanel.activeSelf && nextUpgradeInfoText != null)
            {
                nextUpgradeInfoText.text = currentTooltipMessage;
            }
        }
    }

    // 툴팁에 들어갈 텍스트 계산
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
            // 요청하신 형식: 레벨 변화 및 요구 골드
            currentTooltipMessage = $"요리 레벨 {currentLv} -> {nextLv}\n" +
                                    $"요구 골드 : {entry.Required_Gold} G";

            enhanceButton.interactable = true;
        }
    }
}