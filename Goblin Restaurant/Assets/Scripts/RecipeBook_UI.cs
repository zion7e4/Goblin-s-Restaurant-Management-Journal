using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class RecipeBook_UI : MonoBehaviour
{
    [Header("Left ������ ��� UI")]
    [SerializeField] private TextMeshProUGUI collectionStatusText;
    [SerializeField] private Transform recipeContentParent;
    [SerializeField] private GameObject recipeListButtonPrefab;

    [Header("Right ������ �� UI")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private Image r_DetailImage;
    [SerializeField] private TextMeshProUGUI r_Name;
    [SerializeField] private TextMeshProUGUI r_Desc;

    [Header("�� ���� �ؽ�Ʈ �� ����")]
    [SerializeField] private TextMeshProUGUI r_Info;
    [SerializeField] private Image[] r_StarImages;
    [SerializeField] private Sprite starEmptySprite;
    [SerializeField] private Sprite starFullSprite;

    [Header("��� ������ �� ��ȭ")]
    [SerializeField] private Transform r_NeedIngredientGrid;
    [SerializeField] private GameObject simpleIconPrefab;
    [SerializeField] private Button enhanceButton;
    [SerializeField] private TextMeshProUGUI nextUpgradeInfoText;

    // ����
    [Header("��ȭ ���� ����")]
    [SerializeField] private GameObject upgradeInfoPanel;
    [SerializeField] private TextMeshProUGUI tooltipText; // (Inspector ���� Ȯ��!)

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

    // ���� [������] ���� ����� �ƴ϶� '��ü ���'�� ��ȸ�� ����
    public void RefreshBook()
    {
        foreach (Transform child in recipeContentParent) Destroy(child.gameObject);

        // 1. ��ü ������ ��������
        List<RecipeData> allRecipes = GameDataManager.instance.GetAllRecipeData();
        // 2. ���� ���� ������ ��� �������� (ID�� ����)
        var ownedRecipeIDs = RecipeManager.instance.playerRecipes.Keys;

        // ���� ��Ȳ ������Ʈ
        int totalCount = allRecipes.Count;
        int ownedCount = ownedRecipeIDs.Count;
        collectionStatusText.text = $"���� ��Ȳ: {ownedCount} / {totalCount}";

        // 3. ��ü ��� ����
        foreach (var data in allRecipes)
        {
            GameObject go = Instantiate(recipeListButtonPrefab, recipeContentParent);
            RecipeListButton btn = go.GetComponent<RecipeListButton>();

            // ���� ���� üũ
            bool isOwned = RecipeManager.instance.playerRecipes.ContainsKey(data.id);

            // ��ư ���� (������, ��������, ��Ʈ�ѷ�)
            btn.Setup(data, isOwned, this);
        }
    }
    // ��������������������������������������

    public void ShowRecipeDetails(int recipeID)
    {
        currentRecipeID = recipeID;

        // ������ ��������
        RecipeData data = GameDataManager.instance.GetRecipeDataById(recipeID);
        if (data == null) return;

        // ���� ���� Ȯ��
        bool isOwned = RecipeManager.instance.playerRecipes.TryGetValue(recipeID, out PlayerRecipe playerRecipe);

        if (detailPanel != null) detailPanel.SetActive(true);

        // 1. �̹���/�̸�/���� (�̺����� ������)
        r_DetailImage.sprite = data.fullImage != null ? data.fullImage : data.icon;
        r_Name.text = data.recipeName;

        // 2. ���� ���ο� ���� �б� ó��
        if (isOwned)
        {
            r_DetailImage.color = Color.white; // ���
            r_Desc.text = data.description;

            // ����/���� ���
            int currentLevel = playerRecipe.currentLevel;
            int currentPrice = RecipeManager.instance.GetRecipeSellingPrice(recipeID);

            r_Info.text = $"���� ���� : LV.{currentLevel}\n" +
                          $"�Ǹ� ���� : {currentPrice} ���\n" +
                          $"�丮 �ð� : {data.baseCookTime} ��";

            // ���� ǥ��
            ShowStars(currentLevel);

            // ��ȭ ��ư Ȱ��ȭ
            enhanceButton.interactable = true;
            enhanceButton.GetComponentInChildren<TextMeshProUGUI>().text = "�� ȭ";

            // ���� ���
            CalculateNextUpgradeInfo(playerRecipe);
        }
        else
        {
            // [�̺��� ����]
            r_DetailImage.color = Color.black; // �Ƿ翧 ó�� (����)
            r_Desc.text = "���� ȹ������ ���� �������Դϴ�.";
            r_Info.text = "???";

            // ���� ����
            foreach (var star in r_StarImages) star.gameObject.SetActive(false);

            // ��ȭ ��ư ��Ȱ��ȭ �� �ؽ�Ʈ ����
            enhanceButton.interactable = false;
            enhanceButton.GetComponentInChildren<TextMeshProUGUI>().text = "��ȹ��";

            currentTooltipMessage = "�����̳� ����Ʈ�� ���� ȹ���ϼ���.";
        }

        // 3. ��� ������ (�̺����� �ʿ� ���� ������ - ��Ʈ��)
        // (���� �̺��� �� ����� �ʹٸ� if(isOwned) ������ ��������)
        foreach (Transform child in r_NeedIngredientGrid) Destroy(child.gameObject);
        foreach (var req in data.requiredIngredients)
        {
            GameObject iconObj = Instantiate(simpleIconPrefab, r_NeedIngredientGrid);
            IngredientData ingData = GameDataManager.instance.GetIngredientDataById(req.ingredientID);
            if (ingData != null)
            {
                iconObj.GetComponent<Image>().sprite = ingData.icon;
                // �̺��� �ÿ� �⺻ ������ ǥ��
                int amount = req.amount;
                if (isOwned)
                {
                    RecipeLevelEntry entry = GameDataManager.instance.GetRecipeLevelData(playerRecipe.currentLevel);
                    // (���⼭ ���� ���� ��� ���� ����)
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
        // ���� ���� ���� ��ȭ �õ�
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
            currentTooltipMessage = "�ִ� ���� (Max)";
            enhanceButton.interactable = false;
        }
        else
        {
            currentTooltipMessage = $"�丮 ���� {currentLv} -> {nextLv}\n" +
                                    $"�䱸 ��� : {entry.Required_Gold} G\n" +
                                    $"���� Ȯ�� : {entry.SuccessRate}%";
            enhanceButton.interactable = true;
        }
    }
}