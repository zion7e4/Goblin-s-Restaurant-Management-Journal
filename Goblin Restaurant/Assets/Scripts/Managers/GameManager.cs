using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem; 
using TMPro;
using UnityEngine.EventSystems; 
using System.Collections; 
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Restaurant Management")]
    public RestaurantManager restaurantManager;

    public enum GameState { Preparing, Open, Closing, Settlement }
    public GameState _currentState;
    public GameState currentState
    {
        get { return _currentState; }
        set
        {
            _currentState = value;
            UpdateButtonUI();

            if (_currentState == GameState.Settlement)
            {
                ShowSettlementPanal();
            }
        }
    }

    public float dayDurationInSeconds = 600f;
    private float currentTimeOfDay;
    private float timeScale;
    private int speedState = 0;

    [Header("게임 상태 변수")]
    public int totalGoldAmount = 0;
    private int todaysGold = 0;
    private int todaysCustomers = 0;
    public int DayCount = 1;
    private Camera mainCamera;

    [Header("주인공 설정")]
    public EmployeeData mainCharacterTemplate;

    [Header("테스트용 프리팹")]
    public GameObject greenPrefab;
    
    [Header("UI 및 프리팹 (인스펙터에서 직접 연결하세요)")]
    public List<GameObject> upgradeTableButtons;
    public int tablePrice = 100;

    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI totalGold;

    public Button OpenButton;
    public GameObject PreparePanel;
    public GameObject NextDayButton;
    public GameObject TablePrefab;
    public GameObject settlementPanel;
    public GameObject CheckButton;
    public TextMeshProUGUI todaysGoldText;
    public TextMeshProUGUI totalGoldText;
    public TextMeshProUGUI customerCountText;
    
    // 패널들
    public GameObject menuPlanner;
    public GameObject RecipeSelection;
    public GameObject UpgradeTableButton;
    public GameObject recipeIngredientsPanel;
    public TextMeshProUGUI TimeScaleButtonText;
    public MenuPlannerUI_Controller menuPlannerUI;
    public InventoryUIController inventoryUI;
    public GameObject shopPanel;
    public GameObject recipeShopPanel;
    public GameObject ingredientShopPanel;
    public GameObject RecipeBook;
    public Button TimeScaleButton; 
    public GameObject PanelBlocker; 
    public GameObject PopupManager; 
    public GameObject UpgradeTablePanel; 
    public GameObject employeeSubMenuPanel;
    public GameObject recipeSubMenuPanel; // 레시피 서브 메뉴

    // [추가] 팝업/일시정지 컨트롤러 직접 참조
    public ClosePopupInput closePopupInput;
    public PauseMenuController pauseMenuController;

    private InputSystem_Actions inputActions;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        inputActions = new InputSystem_Actions(); 
    }

    private void OnEnable() { inputActions.Enable(); }
    private void OnDisable() { inputActions.Disable(); }

    void Start()
    {
        // 1. 게임 초기 상태 설정
        currentState = GameState.Preparing;
        timeScale = (9 * 60 * 60) / dayDurationInSeconds;
        currentTimeOfDay = 9 * 3600;
        
        if(timeText) timeText.text = "09:00";
        if(dayText) dayText.text = "Day " + DayCount;
        if(totalGold) totalGold.text = totalGoldAmount.ToString(); 
        if(TimeScaleButtonText) TimeScaleButtonText.text = "X1";

        Time.timeScale = 1;
        mainCamera = Camera.main;
        
        // 2. 주인공 생성
        CreateMainCharacter();
        
        // 3. 직원 스폰
        if (restaurantManager != null)
        {
            List<(EmployeeInstance data, GameObject prefab)> workersToSpawn = new List<(EmployeeInstance, GameObject)>();
            
            if (EmployeeManager.Instance != null)
            {
                var mainWorker = EmployeeManager.Instance.hiredEmployees.FirstOrDefault(e => e.isProtagonist);
                if (mainWorker != null && greenPrefab != null)
                {
                    workersToSpawn.Add((mainWorker, greenPrefab));
                }
            }
            restaurantManager.SpawnWorkersWithPrefabs(workersToSpawn);
        }

        // 4. (테스트) 상점 초기화
        if (ShopManager.Instance != null && FameManager.instance != null)
        {
             ShopManager.Instance.GenerateTodayItems((int)FameManager.instance.CurrentFamePoints); 
        }
        
        UpdateButtonUI();
    }

    void CreateMainCharacter()
    {
        if (mainCharacterTemplate != null && EmployeeManager.Instance != null)
        {
            if (!EmployeeManager.Instance.hiredEmployees.Any(e => e.isProtagonist))
            {
                EmployeeInstance mainCharacter = new EmployeeInstance(mainCharacterTemplate);
                EmployeeManager.Instance.hiredEmployees.Add(mainCharacter);
            }
        }
    }
    
    void Update()
    {
        // ESC 키 입력 감지 (직접 처리)
        if (inputActions.UI.ClosePopup.WasPerformedThisFrame())
        {
            HandleEscInput();
        }

        if (currentState == GameState.Open)
        {
            currentTimeOfDay += Time.deltaTime * timeScale;

            int hours = (int)(currentTimeOfDay / 3600);
            int minutes = (int)((currentTimeOfDay % 3600) / 60);
            if(timeText) timeText.text = string.Format("{0:D2}:{1:D2}", hours, minutes);
            
            // 완판 체크
            if (MenuPlanner.instance != null && MenuPlanner.instance.isSoldOut)
            {
                bool noCustomers = (restaurantManager.customers.Count == 0);
                bool anyDirtyTables = restaurantManager.tables.Any(t => t.isDirty);
                if (noCustomers && !anyDirtyTables) currentState = GameState.Closing; 
            }
            
            // 마감 시간 체크
            if (currentTimeOfDay >= 18 * 3600)
            {
                currentState = GameState.Closing;
            }
        }

        if (currentState == GameState.Closing)
        {
            bool hasCustomers = restaurantManager.customers.Count > 0;
            bool hasDirtyTables = restaurantManager.tables.Any(t => t.isDirty);
            
            if (!hasCustomers && !hasDirtyTables)
            {
                currentState = GameState.Settlement;
            }
        }
    }

    // ESC 처리 로직
    void HandleEscInput()
    {
        // 1. 일시정지 메뉴가 켜져있으면 닫기
        if (pauseMenuController != null && pauseMenuController.gameObject.activeSelf)
        {
            ClosePauseMenu();
            return;
        }

        // 2. 팝업 닫기 시도
        if (closePopupInput != null && closePopupInput.TryCloseTopPopup())
        {
            return;
        }

        // 3. 일시정지 열기
        OpenPauseMenu();
    }
    
    // --- 기능 함수들 ---

    public void MoveToNextDay()
    {
        if (currentState == GameState.Settlement) 
        {
            // 리셋 로직
            timeText.text = "09:00";
            todaysGold = 0;
            todaysCustomers = 0;
            if (MenuPlanner.instance != null)
            {
                MenuPlanner.instance.isSoldOut = false; 
                MenuPlanner.instance.ClearDailyMenu();
            }
            if (menuPlannerUI != null) menuPlannerUI.UpdateTodayMenuUI();
            
            currentState = GameState.Preparing;
            currentTimeOfDay = 9 * 3600;
            DayCount += 1;
            if(dayText) dayText.text = "Day " + DayCount;
            
            // 지원자 생성
            if ((DayCount - 1) % 7 == 0 && DayCount > 1)
            {
                EmployeeManager.Instance.GenerateApplicants((int)FameManager.instance.CurrentFamePoints);
            }
            
            // 상점 갱신 (6시가 아니라 아침에 갱신하고 싶다면 여기서 호출)
            // if (ShopManager.Instance != null) ShopManager.Instance.GenerateTodayItems(...);
        }
    }

    private void UpdateButtonUI()
    {
        if (PreparePanel) PreparePanel.SetActive(currentState == GameState.Preparing);
        if (NextDayButton) NextDayButton.SetActive(currentState == GameState.Settlement); 
        if (OpenButton != null) OpenButton.gameObject.SetActive(currentState == GameState.Preparing);

        bool isPreparing = (currentState == GameState.Preparing);
        if (TimeScaleButton != null) TimeScaleButton.gameObject.SetActive(currentState == GameState.Open);

        foreach (GameObject button in upgradeTableButtons)
        {
            if (button != null) button.SetActive(isPreparing && totalGoldAmount >= tablePrice);
        }
    }

    public void OpenTheStore()
    {
        if (currentState == GameState.Preparing)
        {
            if (currentTimeOfDay > 9 * 3600) 
            {
                currentTimeOfDay = 9 * 3600;
                timeText.text = "09:00";
            }

            currentState = GameState.Open;
            if (menuPlanner != null) menuPlanner.SetActive(false);

            if (MenuPlanner.instance != null)
            {
                MenuPlanner.instance.ConsumeIngredientsForToday(); 
                MenuPlanner.instance.StartDaySales(); 
            }
        }
    }

    // 패널 열기/닫기 함수들
    public void OpenPauseMenu()
    {
        if (pauseMenuController != null)
        {
            pauseMenuController.OpenPauseMenu();
        }
    }
    public void ClosePauseMenu()
    {
        if (pauseMenuController != null)
        {
            pauseMenuController.ClosePauseMenu();
        }
    }

    // ... (나머지 Open/Close 함수들, AddTable, AddGold 등은 기존 로직 유지) ...
    
    public void SetStartButtonInteractable(bool isInteractable)
    {
        if (OpenButton != null) OpenButton.interactable = isInteractable;
    }
    
    public void closeSettlementPanal()
    {
        settlementPanel.SetActive(false);
        CheckButton.SetActive(false);
    }
    
    private void ShowSettlementPanal()
    {
        todaysGoldText.text = $"오늘 획득한 골드: {todaysGold}";
        totalGoldText.text = $"총 보유 골드: {totalGoldAmount}";
        customerCountText.text = $"방문객 수: {todaysCustomers}";
        settlementPanel.SetActive(true);
        CheckButton.SetActive(true);
    }

    public void AddGold(int amount)
    {
        totalGoldAmount += amount;
        todaysGold += amount;
        if(totalGold) totalGold.text = totalGoldAmount.ToString();
    }

    public void SpendGold(int amount)
    {
        totalGoldAmount -= amount;
        if(totalGold) totalGold.text = totalGoldAmount.ToString();
    }

    public void AddCustomerCount() { todaysCustomers++; }
    
    public void ChangeTimeScale()
    {
        speedState = (speedState + 1) % 3;
        switch (speedState)
        {
            case 0: Time.timeScale = 1; TimeScaleButtonText.text = "X1"; break;
            case 1: Time.timeScale = 2; TimeScaleButtonText.text = "X2"; break;
            case 2: Time.timeScale = 0; TimeScaleButtonText.text = "||"; break;
        }
    }
    
    // UI 열기 함수들 (GlobalButton에서 호출용)
    public void OpenRecipeBook() { RecipeBook.SetActive(true); PanelBlocker.SetActive(true); if(recipeSubMenuPanel) recipeSubMenuPanel.SetActive(false); }
    public void OpenMenuPlanner() { menuPlanner.SetActive(true); }
    public void OpenShopPanel() { shopPanel.SetActive(true); PanelBlocker.SetActive(true); }
    public void OpenInventoryPanel() { inventoryUI.OpenInventory(); PanelBlocker.SetActive(true); if(recipeSubMenuPanel) recipeSubMenuPanel.SetActive(false); }
    public void OpenUpgradeTablePanel() { UpgradeTablePanel.SetActive(true); PanelBlocker.SetActive(true); }
    public void OpenEmployeeSubMenu() { employeeSubMenuPanel.SetActive(true); }
    
    public void OpenRecipeSubMenu() 
    { 
        if(recipeSubMenuPanel) recipeSubMenuPanel.SetActive(true); 
        if(PopupManager) PopupManager.SetActive(true); // 팝업 매니저(블로커) 켜기
    }

    public void CloseRecipeSubMenu()
    {
        if(recipeSubMenuPanel) recipeSubMenuPanel.SetActive(false);
        if(PopupManager) PopupManager.SetActive(false);
    }

    public void AddTable(Transform buttonTransform)
    {
        // (기존 테이블 설치 로직 유지)
    }
}