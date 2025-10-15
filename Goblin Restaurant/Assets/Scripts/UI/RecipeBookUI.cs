using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class RecipeBookUI : MonoBehaviour
{
    [Header("상단 UI")]
    public TextMeshProUGUI collectionStatusText;
    public TMP_Dropdown sortDropdown;
    public TMP_Dropdown filterDropdown;

    [Header("좌측 목록 패널")]
    public Transform recipeListContent;
    public GameObject recipeListItemPrefab;
    public Sprite filledStarSprite;
    public Sprite emptyStarSprite;

    [Header("우측 상세 정보 패널")]
    public GameObject detailPanelParent;
    public Image detail_RecipeIcon;
    public TextMeshProUGUI detail_RecipeName;
    public Transform detail_GradeStarsParent;
    public TextMeshProUGUI detail_RecipeInfoText;
    public Transform detail_IngredientsParent;
    public GameObject ingredientIconPrefab;
    public TextMeshProUGUI detail_Description;
    public Button upgradeButton;

    private RecipeInstance selectedRecipe;
    private List<GameObject> spawnedListItems = new List<GameObject>();

    // [핵심 수정] UI가 활성화될 때마다 목록을 새로고침하도록 OnEnable 함수를 사용합니다.
    void OnEnable()
    {
        RefreshRecipeList();
        // UI가 켜질 때는 항상 상세 정보창을 숨깁니다.
        if (detailPanelParent != null) detailPanelParent.SetActive(false);
    }

    void Start()
    {
        if (upgradeButton != null) upgradeButton.onClick.AddListener(UpgradeSelectedRecipe);
    }

    public void OpenRecipeBook()
    {
        gameObject.SetActive(true);
    }

    public void CloseRecipeBook()
    {
        gameObject.SetActive(false);
    }

    void RefreshRecipeList()
    {
        // 기존 목록 아이템들을 모두 삭제합니다.
        foreach (GameObject item in spawnedListItems)
        {
            Destroy(item);
        }
        spawnedListItems.Clear();

        // RecipeManager가 준비되었는지 확인합니다.
        if (RecipeManager.Instance == null) return;

        List<RecipeInstance> currentRecipes = RecipeManager.Instance.ownedRecipes;

        // 목록의 각 레시피에 대해 UI 아이템을 생성하고 내용을 채웁니다.
        foreach (RecipeInstance recipe in currentRecipes)
        {
            GameObject listItem = Instantiate(recipeListItemPrefab, recipeListContent);

            Image icon = listItem.transform.Find("Icon")?.GetComponent<Image>();
            TextMeshProUGUI nameText = listItem.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            Transform starsParent = listItem.transform.Find("GradeStarsContainer");
            Button itemButton = listItem.GetComponent<Button>();

            if (icon != null) icon.sprite = recipe.BaseData.icon;
            if (nameText != null) nameText.text = recipe.BaseData.recipeName;

            if (starsParent != null) UpdateGradeStars(starsParent, recipe.BaseData.GetGrade(recipe.currentLevel));

            // 각 목록 아이템을 클릭하면 ShowRecipeDetails 함수가 실행되도록 연결합니다.
            if (itemButton != null)
            {
                itemButton.onClick.RemoveAllListeners();
                itemButton.onClick.AddListener(() => ShowRecipeDetails(recipe));
            }

            spawnedListItems.Add(listItem);
        }

        // 수집 현황 텍스트를 업데이트합니다.
        if (collectionStatusText != null)
            collectionStatusText.text = $"수집 현황: {currentRecipes.Count}/{RecipeManager.Instance.allRecipesInGame.Count}";
    }

    void ShowRecipeDetails(RecipeInstance recipe)
    {
        selectedRecipe = recipe;
        if (detailPanelParent == null) return;

        detailPanelParent.SetActive(true);

        if (detail_RecipeIcon != null) detail_RecipeIcon.sprite = recipe.BaseData.icon;
        if (detail_RecipeName != null) detail_RecipeName.text = recipe.BaseData.recipeName;
        if (detail_Description != null) detail_Description.text = recipe.BaseData.description;

        if (detail_RecipeInfoText != null)
            detail_RecipeInfoText.text = $"음식 레벨: Lv.{recipe.currentLevel}\n판매 가격: {recipe.BaseData.basePrice}골드\n요리 시간: {recipe.BaseData.baseCookTime}초";

        if (detail_GradeStarsParent != null)
            UpdateGradeStars(detail_GradeStarsParent, recipe.BaseData.GetGrade(recipe.currentLevel));

        // TODO: 필요 재료 아이콘들을 생성하고 표시하는 로직
    }

    void UpdateGradeStars(Transform starsParent, RecipeGrade grade)
    {
        int gradeNumber = (int)grade + 1;
        for (int i = 0; i < starsParent.childCount; i++)
        {
            Image starImage = starsParent.GetChild(i).GetComponent<Image>();
            if (starImage == null) continue;

            if (i < gradeNumber) { starImage.sprite = filledStarSprite; }
            else { starImage.sprite = emptyStarSprite; }
        }
    }

    void UpgradeSelectedRecipe()
    {
        if (selectedRecipe != null)
        {
            // TODO: 강화 재료/골드 확인 로직
            selectedRecipe.Upgrade();
            RefreshRecipeList();
            ShowRecipeDetails(selectedRecipe);
        }
    }
}