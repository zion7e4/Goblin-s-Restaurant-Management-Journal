using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeListButton : MonoBehaviour
{
    // 1. 인스펙터에서 연결할 UI
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private Button thisButton;

    // 2. 내부 저장용
    private PlayerRecipe myRecipe;
    private RecipeBook_UI bookController;

    // 3. RecipeBook_UI가 호출할 초기화 함수
    public void Setup(PlayerRecipe playerRecipe, RecipeBook_UI book_UI)
    {
        myRecipe = playerRecipe;
        bookController = book_UI;

        // 4. 버튼의 텍스트를 레시피 이름으로 설정
        recipeNameText.text = playerRecipe.data.recipeName;

        // 5. 버튼 클릭 리스너 연결
        thisButton.onClick.RemoveAllListeners();
        thisButton.onClick.AddListener(OnShowDetailsClick);
    }

    // 6. 클릭 시 RecipeBook_UI에게 알림
    void OnShowDetailsClick()
    {
        bookController.ShowRecipeDetails(myRecipe.data.id);
    }
}