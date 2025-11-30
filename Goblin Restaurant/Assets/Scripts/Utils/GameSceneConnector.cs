using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameSceneConnector : MonoBehaviour
{
    [Header("1. 텍스트 UI (Texts)")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI totalGold;
    public TextMeshProUGUI todaysGoldText;
    public TextMeshProUGUI totalGoldText;
    public TextMeshProUGUI customerCountText;
    public TextMeshProUGUI TimeScaleButtonText;

    [Header("2. 메인 버튼 UI (Main Buttons)")]
    public Button OpenButton;
    public Button TimeScaleButton;
    public List<GameObject> upgradeTableButtons;

    [Header("3. 주요 패널 (Panels)")]
    public GameObject PreparePanel;
    public GameObject settlementPanel;
    public GameObject menuPlanner;
    public GameObject RecipeSelection;
    public GameObject recipeIngredientsPanel;
    public GameObject shopPanel;
    public GameObject recipeShopPanel;
    public GameObject ingredientShopPanel;
    public GameObject RecipeBook;
    public GameObject UpgradeTablePanel;
    public GameObject employeeSubMenuPanel;
    public GameObject PanelBlocker;

    [Header("4. 기타 오브젝트 (Objects)")]
    public GameObject NextDayButton;
    public GameObject CheckButton;
    public GameObject UpgradeTableButton;
    public GameObject PopupManager;

    [Header("5. 컨트롤러 & 매니저 (Controllers)")]
    public RestaurantManager restaurantManager;
    public MenuPlannerUI_Controller menuPlannerUI;
    public InventoryUIController inventoryUI;
    public ClosePopupInput closePopupInput;
    public PauseMenuController pauseMenuController;

    // ▼▼▼ [추가] 사이드 메뉴 버튼들 ▼▼▼
    [Header("6. 사이드 메뉴 버튼 (Sidebar Buttons)")]
    public Button btnRecipeBook;    // 레시피 도감 버튼
    public Button btnMenuPlanner;   // 메뉴 편성 버튼
    public Button btnShop;          // 상점 버튼
    public Button btnEmployee;      // 직원 관리 버튼


    void Start()
    {
        if (GameManager.instance == null) return;

        // --- 1. 변수 재연결 ---
        GameManager.instance.timeText = timeText;
        GameManager.instance.dayText = dayText;
        GameManager.instance.totalGold = totalGold;
        GameManager.instance.todaysGoldText = todaysGoldText;
        GameManager.instance.totalGoldText = totalGoldText;
        GameManager.instance.customerCountText = customerCountText;
        GameManager.instance.TimeScaleButtonText = TimeScaleButtonText;

        GameManager.instance.OpenButton = OpenButton;
        GameManager.instance.TimeScaleButton = TimeScaleButton;
        GameManager.instance.upgradeTableButtons = upgradeTableButtons;

        GameManager.instance.PreparePanel = PreparePanel;
        GameManager.instance.settlementPanel = settlementPanel;
        GameManager.instance.menuPlanner = menuPlanner;
        GameManager.instance.RecipeSelection = RecipeSelection;
        GameManager.instance.recipeIngredientsPanel = recipeIngredientsPanel;
        GameManager.instance.shopPanel = shopPanel;
        GameManager.instance.recipeShopPanel = recipeShopPanel;
        GameManager.instance.ingredientShopPanel = ingredientShopPanel;
        GameManager.instance.RecipeBook = RecipeBook;
        GameManager.instance.UpgradeTablePanel = UpgradeTablePanel;
        GameManager.instance.employeeSubMenuPanel = employeeSubMenuPanel;
        GameManager.instance.panelBlocker = PanelBlocker;

        GameManager.instance.NextDayButton = NextDayButton;
        GameManager.instance.CheckButton = CheckButton;
        GameManager.instance.UpgradeTableButton = UpgradeTableButton;
        GameManager.instance.PopupManager = PopupManager;

        GameManager.instance.restaurantManager = restaurantManager;
        GameManager.instance.menuPlannerUI = menuPlannerUI;
        GameManager.instance.inventoryUI = inventoryUI;
        GameManager.instance.closePopupInput = closePopupInput;
        GameManager.instance.pauseMenuController = pauseMenuController;
        GameManager.instance.recipeIngredientsPanel = recipeIngredientsPanel;



        // --- UI 갱신 및 초기화 ---
        GameManager.instance.AddGold(0);
        GameManager.instance.InitializeScene(); // (이전 턴에서 만든 초기화 함수 호출)

        // =================================================================
        // ▼▼▼ [핵심] 모든 버튼 이벤트 코드 재연결 (Inspector Missing 해결) ▼▼▼
        // =================================================================

        // 1. 메인 기능 버튼
        if (OpenButton != null)
        {
            OpenButton.onClick.RemoveAllListeners();
            OpenButton.onClick.AddListener(GameManager.instance.OpenTheStore);
        }
        if (TimeScaleButton != null)
        {
            TimeScaleButton.onClick.RemoveAllListeners();
            TimeScaleButton.onClick.AddListener(GameManager.instance.ChangeTimeScale);
        }
        if (NextDayButton != null)
        {
            Button btn = NextDayButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(GameManager.instance.MoveToNextDay);
            }
        }
        if (CheckButton != null)
        {
            Button btn = CheckButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(GameManager.instance.closeSettlementPanal);
            }
        }

        // 2. 사이드 메뉴 버튼 (여기에 추가된 버튼들을 연결합니다)
        if (btnRecipeBook != null)
        {
            btnRecipeBook.onClick.RemoveAllListeners();
            btnRecipeBook.onClick.AddListener(GameManager.instance.OpenRecipeBook);
        }
        if (btnMenuPlanner != null)
        {
            btnMenuPlanner.onClick.RemoveAllListeners();
            btnMenuPlanner.onClick.AddListener(GameManager.instance.OpenMenuPlanner);
        }
        if (btnShop != null)
        {
            btnShop.onClick.RemoveAllListeners();
            btnShop.onClick.AddListener(GameManager.instance.OpenShopPanel);
        }
        if (btnEmployee != null)
        {
            btnEmployee.onClick.RemoveAllListeners();
            btnEmployee.onClick.AddListener(GameManager.instance.OpenEmployeeSubMenu);
        }

        Debug.Log("✅ [GameSceneConnector] 모든 UI 연결 및 버튼 이벤트 복구 완료.");
    }
}