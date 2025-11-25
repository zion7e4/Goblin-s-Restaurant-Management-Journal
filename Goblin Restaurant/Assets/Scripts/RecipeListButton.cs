using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeListButton : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Button thisButton;        // 버튼 본체
    [SerializeField] private Image recipeIcon;         // 아이콘 이미지
    [SerializeField] private TextMeshProUGUI nameText; // 요리 이름

    [Header("별점 설정")]
    [SerializeField] private Image[] starImages;      // 별 이미지 5개
    [SerializeField] private Sprite starEmptySprite;  // 꺼진 별 그림
    [SerializeField] private Sprite starFullSprite;   // 켜진 별 그림

    private PlayerRecipe myRecipe;
    private RecipeBook_UI bookController;

    public void Setup(PlayerRecipe playerRecipe, RecipeBook_UI book_UI)
    {
        myRecipe = playerRecipe;
        bookController = book_UI;

        // 기본 정보 설정
        if (nameText != null) nameText.text = playerRecipe.data.recipeName;
        if (recipeIcon != null) recipeIcon.sprite = playerRecipe.data.icon;

        // 레벨에 따른 별점 이미지 교체 로직
        int level = playerRecipe.currentLevel;
        int starCount = 1; // 기본 1개

        if (level >= 40) starCount = 5;
        else if (level >= 30) starCount = 4;
        else if (level >= 20) starCount = 3;
        else if (level >= 10) starCount = 2;

        // 별 개수만큼은 켜진 별, 나머지는 꺼진 별로 교체
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) continue;

            if (i < starCount)
                starImages[i].sprite = starFullSprite;
            else
                starImages[i].sprite = starEmptySprite;
        }

        // 클릭 이벤트 연결
        thisButton.onClick.RemoveAllListeners();
        thisButton.onClick.AddListener(OnShowDetailsClick);
    }

    void OnShowDetailsClick()
    {
        bookController.ShowRecipeDetails(myRecipe.data.id);
    }
}