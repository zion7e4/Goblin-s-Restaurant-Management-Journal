using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeListButton : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Button thisButton;
    [SerializeField] private Image recipeIcon;
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("별점 설정")]
    [SerializeField] private Image[] starImages;
    [SerializeField] private Sprite starEmptySprite;
    [SerializeField] private Sprite starFullSprite;

    private int recipeID;
    private RecipeBook_UI bookController;
    private bool isOwned; // 보유 여부

    // Setup 함수 변경 (PlayerRecipe 대신 RecipeData와 보유 여부를 받음)
    public void Setup(RecipeData data, bool _isOwned, RecipeBook_UI book_UI)
    {
        recipeID = data.id;
        isOwned = _isOwned;
        bookController = book_UI;

        // 1. 기본 정보 (이름, 아이콘)
        if (nameText != null) nameText.text = data.recipeName;
        if (recipeIcon != null) recipeIcon.sprite = data.icon;

        // 2. 보유 여부에 따른 시각적 처리 (핵심!)
        if (isOwned)
        {
            // [보유함] 밝게 표시, 별점 표시
            if (recipeIcon != null) recipeIcon.color = Color.white;
            if (nameText != null) nameText.color = Color.white; // 텍스트도 밝게

            // 현재 레벨 가져오기
            int currentLevel = 1;
            if (RecipeManager.instance.playerRecipes.TryGetValue(recipeID, out PlayerRecipe pRecipe))
            {
                currentLevel = pRecipe.currentLevel;
            }
            UpdateStars(currentLevel);
        }
        else
        {
            // [미보유] 어둡게 표시, 별점 숨기기
            if (recipeIcon != null) recipeIcon.color = Color.gray; // 회색으로 어둡게
            if (nameText != null) nameText.color = Color.gray;

            // 별점 싹 다 끄기 (또는 물음표 처리)
            foreach (var star in starImages) star.gameObject.SetActive(false);
        }

        // 3. 클릭 이벤트
        thisButton.onClick.RemoveAllListeners();
        thisButton.onClick.AddListener(OnShowDetailsClick);
    }

    void UpdateStars(int level)
    {
        // 별점 UI 켜기
        foreach (var star in starImages) star.gameObject.SetActive(true);

        int starCount = 1;
        if (level >= 40) starCount = 5;
        else if (level >= 30) starCount = 4;
        else if (level >= 20) starCount = 3;
        else if (level >= 10) starCount = 2;

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) continue;
            starImages[i].sprite = (i < starCount) ? starFullSprite : starEmptySprite;
        }
    }

    void OnShowDetailsClick()
    {
        // 미보유 상태일 때도 클릭은 되게 할 건지, 아예 안 되게 할 건지 결정
        // (여기서는 클릭하면 상세창에 "미획득" 정보를 띄우도록 넘겨줍니다.)
        bookController.ShowRecipeDetails(recipeID);
    }
}