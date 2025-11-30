using UnityEngine;
using UnityEngine.UI;

public class GlobalButton : MonoBehaviour
{
    public enum ButtonType
    {
        // --- 메인 기능 ---
        OpenStore,      // 영업 시작
        NextDay,        // 다음 날
        CheckSettlement,// 정산 확인
        TimeScale,      // 시간 배속
        
        // --- 사이드 메뉴 (1단계: 서브 패널 열기) ---
        Side_Recipe_Menu,   // 레시피 서브 메뉴 열기 (도감/재료 선택창)
        Side_Employee_Menu, // 직원 서브 메뉴 열기 (고용/관리 선택창)
        
        Side_MenuPlan,  // 메뉴 편성 (바로 열림)
        Side_Shop,      // 상점 (바로 열림)
        Side_Interior,  // 인테리어 (바로 열림)
        
        // --- 서브 메뉴 내부 버튼 (2단계: 실제 기능 열기) ---
        Sub_OpenRecipeBook, // 레시피 도감 열기
        Sub_OpenInventory,  // 재료 인벤토리 열기
        // (직원 쪽은 EmployeeUI_Controller가 내부 탭으로 관리하므로 별도 버튼 필요 없음)

        // --- 일시정지 메뉴 ---
        Pause_Resume,
        Pause_ToTitle,
        Pause_Quit
    }

    public ButtonType buttonType;
    private Button myButton;

    void Start()
    {
        myButton = GetComponent<Button>();
        if (myButton == null) return;

        myButton.onClick.RemoveAllListeners();

        if (GameManager.instance != null)
        {
            switch (buttonType)
            {
                case ButtonType.OpenStore:
                    myButton.onClick.AddListener(GameManager.instance.OpenTheStore);
                    GameManager.instance.OpenButton = myButton; 
                    break;
                case ButtonType.NextDay:
                    myButton.onClick.AddListener(GameManager.instance.MoveToNextDay);
                    break;
                case ButtonType.CheckSettlement:
                    myButton.onClick.AddListener(GameManager.instance.closeSettlementPanal);
                    break;
                case ButtonType.TimeScale:
                    myButton.onClick.AddListener(GameManager.instance.ChangeTimeScale);
                    break;
                    
                // --- 사이드 메뉴 (서브 패널 열기) ---
                case ButtonType.Side_Recipe_Menu:
                    myButton.onClick.AddListener(GameManager.instance.OpenRecipeIngredientsPanel); // [신규]
                    break;
                case ButtonType.Side_Employee_Menu:
                    myButton.onClick.AddListener(GameManager.instance.OpenEmployeeSubMenu);
                    break;
                
                // --- 기타 사이드 메뉴 ---
                case ButtonType.Side_MenuPlan:
                    myButton.onClick.AddListener(GameManager.instance.OpenMenuPlanner);
                    break;
                case ButtonType.Side_Shop:
                    myButton.onClick.AddListener(GameManager.instance.OpenShopPanel);
                    break;
                case ButtonType.Side_Interior:
                    myButton.onClick.AddListener(GameManager.instance.OpenUpgradeTablePanel);
                    break;

                // --- 서브 메뉴 내부 버튼 (실제 기능) ---
                case ButtonType.Sub_OpenRecipeBook:
                    myButton.onClick.AddListener(GameManager.instance.OpenRecipeBook);
                    break;
                case ButtonType.Sub_OpenInventory:
                    myButton.onClick.AddListener(GameManager.instance.OpenInventoryPanel);
                    break;

                // --- 일시정지 ---
                case ButtonType.Pause_Resume:
                    myButton.onClick.AddListener(GameManager.instance.ClosePauseMenu);
                    break;
                case ButtonType.Pause_ToTitle:
                    if(GameManager.instance.pauseMenuController != null)
                        myButton.onClick.AddListener(GameManager.instance.pauseMenuController.GoToTitle);
                    break;
                case ButtonType.Pause_Quit:
                    if (GameManager.instance.pauseMenuController != null)
                        myButton.onClick.AddListener(GameManager.instance.pauseMenuController.QuitGame);
                    break;
            }
        }
    }
}