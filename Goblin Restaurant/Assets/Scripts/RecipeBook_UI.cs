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

    private int currentRecipeID;

    void Start()
    {
        if (enhanceButton != null)
            enhanceButton.onClick.AddListener(OnEnhanceButtonClick);

        // 레시피 획득 또는 강화 시 목록 갱신 이벤트 연결
        if (RecipeManager.instance != null)
        {
            RecipeManager.instance.onRecipeUpdated += RefreshBook;
        }

        // 시작할 때는 상세창을 끄고 목록만 갱신
        if (detailPanel != null) detailPanel.SetActive(false);
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

        // 도감을 열 때 상세창은 일단 숨김
        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }

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

        // 버튼을 눌렀을 때만 상세창 활성화
        if (detailPanel != null) detailPanel.SetActive(true);

        r_DetailImage.sprite = data.fullImage != null ? data.fullImage : data.icon;
        r_Name.text = data.recipeName;
        r_Desc.text = data.description;

        // 정보 텍스트 줄바꿈 적용
        r_Info.text = $"음식 레벨 : LV.{playerRecipe.currentLevel}\n" +
                      $"판매 가격 : {data.basePrice} 골드\n" +
                      $"요리 시간 : {data.baseCookTime} 초";

        // 레벨에 따른 별점 계산 (1~9: 1개, 10~19: 2개, 40이상: 5개)
        int level = playerRecipe.currentLevel;
        int starCount = 1;

        if (level >= 40) starCount = 5;
        else if (level >= 30) starCount = 4;
        else if (level >= 20) starCount = 3;
        else if (level >= 10) starCount = 2;

        // 별 스프라이트 교체
        for (int i = 0; i < r_StarImages.Length; i++)
        {
            if (i < starCount)
                r_StarImages[i].sprite = starFullSprite;
            else
                r_StarImages[i].sprite = starEmptySprite;
        }

        // 필요 재료 아이콘 표시
        foreach (Transform child in r_NeedIngredientGrid) Destroy(child.gameObject);

        foreach (var req in data.requiredIngredients)
        {
            GameObject iconObj = Instantiate(simpleIconPrefab, r_NeedIngredientGrid);
            IngredientData ingData = GameDataManager.instance.GetIngredientDataById(req.ingredientID);
            if (ingData != null)
            {
                iconObj.GetComponent<Image>().sprite = ingData.icon;
                iconObj.GetComponentInChildren<TextMeshProUGUI>().text = $"x{req.amount}";
            }
        }

        UpdateNextUpgradeInfo(playerRecipe);
    }

    void OnEnhanceButtonClick()
    {
        if (RecipeManager.instance.UpgradeRecipe(currentRecipeID))
        {
            ShowRecipeDetails(currentRecipeID);
            RefreshBook();
        }
    }

    void UpdateNextUpgradeInfo(PlayerRecipe recipe)
    {
        int nextLv = recipe.currentLevel + 1;
        RecipeLevelEntry entry = GameDataManager.instance.GetRecipeLevelData(nextLv);

        if (entry == null)
        {
            nextUpgradeInfoText.text = "최대 레벨 (Max)";
            enhanceButton.interactable = false;
        }
        else
        {
            nextUpgradeInfoText.text = $"[다음 강화 비용]\n골드: {entry.Required_Gold}G\n재료 배수: x{entry.Required_Item_Count}";
            enhanceButton.interactable = true;
        }
    }
}