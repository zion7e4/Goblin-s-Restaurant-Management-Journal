using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 레시피 도감 목록에 표시될 각 '텍스트 버튼'의 UI를 제어합니다.
/// </summary>
public class RecipeListButton : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("레시피 이름이 표시될 텍스트")]
    [SerializeField] private TextMeshProUGUI recipeNameText;

    [Tooltip("클릭 이벤트를 감지할 이 프리팹의 버튼")]
    [SerializeField] private Button thisButton;

    // 내부 저장용
    private PlayerRecipe myRecipe;
    private RecipeBook_UI bookController; // 메인 UI 컨트롤러

    /// <summary>
    /// RecipeBook_UI가 이 아이템을 초기화할 때 호출합니다.
    /// </summary>
    public void Setup(PlayerRecipe playerRecipe, RecipeBook_UI book_UI)
    {
        myRecipe = playerRecipe;
        bookController = book_UI;

        // 1. 버튼의 텍스트를 레시피 이름으로 설정
        recipeNameText.text = playerRecipe.data.recipeName;

        // 2. 버튼 클릭 리스너 연결
        thisButton.onClick.RemoveAllListeners();
        thisButton.onClick.AddListener(OnShowDetailsClick);
    }

    /// <summary>
    /// '상세보기' 버튼을 클릭했을 때 호출
    /// </summary>
    void OnShowDetailsClick()
    {
        // RecipeBook_UI에게 "이 레시피의 상세정보를 보여줘"라고 알림
        bookController.ShowRecipeDetails(myRecipe.data.id);
    }
}