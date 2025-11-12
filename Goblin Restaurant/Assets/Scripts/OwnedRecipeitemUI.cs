using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OwnedRecipeItemUI : MonoBehaviour
{
    public Button selectButton;
    public TextMeshProUGUI recipeNameText;
    // (추가) public Image recipeIconImage;

    private PlayerRecipe myRecipe;
    private MenuPlannerUI_Controller controller; // 컨트롤러의 정보를 저장할 변수

    // 컨트롤러로부터 정보를 받아 자신을 초기화하는 함수
    public void Setup(PlayerRecipe recipe, bool canSelect, MenuPlannerUI_Controller uiController)
    {
        myRecipe = recipe;
        controller = uiController; // 컨트롤러 정보 저장

        recipeNameText.text = myRecipe.data.recipeName;
        // if (recipeIconImage != null) recipeIconImage.sprite = myRecipe.data.icon;

        // 선택 가능한 상태일 때만 버튼을 활성화
        selectButton.interactable = canSelect;

        // 버튼 클릭 시 OnSelectButtonClick 함수를 호출하도록 설정
        selectButton.onClick.AddListener(OnSelectButtonClick);
    }

    void OnSelectButtonClick()
    {
        // 저장해둔 컨트롤러에게 "내가 눌렸어!" 라고 직접 알림
        if (controller != null)
        {
            controller.OnRecipeSelectedFromPopup(myRecipe);
        }
    }
}