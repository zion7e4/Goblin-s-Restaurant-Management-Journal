using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;

// 역할: 게임의 시간, 명성, 돈, 게임 상태 등 전반적인 상태를 관리합니다.
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Restaurant Management")]
    [Tooltip("Restaurant Manager 스크립트를 가진 오브젝트를 연결하세요.")]
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
    [Tooltip("주인공으로 사용할 직원의 설계도(EmployeeData 에셋)")]
    public EmployeeData mainCharacterTemplate;

    // [삭제됨] 테스트용 프리팹 변수들 (Green, Red, Blue, TestTemplate) 제거 완료

    [Header("UI 및 프리팹")]
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
    public GameObject panelBlocker;
    public GameObject PopupManager;
    public GameObject UpgradeTablePanel;

    [Header("직원 UI")]
    [Tooltip("PreparePanel에서 열릴 '직원 서브 메뉴' 패널을 연결하세요.")]
    public GameObject employeeSubMenuPanel;

    private InputSystem_Actions inputActions;

    [Header("Pause Menu")]
    public PauseMenuController pauseMenuController; // 인스펙터에서 연결
    [Header("UI Controllers (Auto Assigned)")]
    public ClosePopupInput closePopupInput;        

    public bool IsPauseMenuOpen 
    { 
        get { return pauseMenuController != null && pauseMenuController.gameObject.activeSelf; } 
    }

    private void Awake()
    {
        inputActions = new InputSystem_Actions();

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        if(inputActions != null) inputActions.Enable();
    }
    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        if (inputActions != null) inputActions.Disable();
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // 게임 씬(MainScene)으로 돌아왔을 때만 초기화
        if (scene.name == "MainScene") 
        {
            InitializeScene();
        }
    }
    void CreateMainCharacter()
    {
        if (mainCharacterTemplate != null && EmployeeManager.Instance != null)
        {
            if (!EmployeeManager.Instance.hiredEmployees.Any(e => e.isProtagonist))
            {
                EmployeeInstance mainCharacter = new EmployeeInstance(mainCharacterTemplate);
                // 주의: EmployeeInstance에 isProtagonist가 public이어야 에러가 안 납니다.
                // 만약 에러가 나면 EmployeeInstance.cs에서 { get; private set; }을 지우세요.
                // mainCharacter.isProtagonist = true; 
                EmployeeManager.Instance.hiredEmployees.Add(mainCharacter);
                Debug.Log($"주인공 '{mainCharacter.firstName}'이(가) 식당에 합류했습니다!");
            }
        }
    }

    void Start()
    {
        
    }

    public void InitializeScene()
    {
        Debug.Log(">>> GameManager: 씬 초기화 시작 (InitializeScene)");

        // 1. 상태 강제 리셋
        currentState = GameState.Preparing;
        Time.timeScale = 1f; // 시간 정지 해제

        // 2. UI 강제 갱신
        // (GameSceneConnector나 GlobalButton이 Awake/Start에서 연결되기를 잠시 기다린 후 UI 갱신)
        StartCoroutine(LateUIUpdate());

        // ... (기존 직원 스폰 로직 유지) ...
        if (restaurantManager == null) restaurantManager = FindFirstObjectByType<RestaurantManager>();
        // (직원 스폰 코드...)
    }

    IEnumerator LateUIUpdate()
    {
        yield return new WaitForEndOfFrame(); // 프레임 끝까지 대기 (모든 UI의 Start가 끝난 뒤)
        
        // 패널 강제 끄기
        if (menuPlanner != null) menuPlanner.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (PreparePanel != null) PreparePanel.SetActive(true); // 준비 패널은 켜기

        UpdateButtonUI(); // 버튼 상태 업데이트
        Debug.Log(">>> GameManager: UI 강제 업데이트 완료");
    }
    void Update()
    {
        if (currentState == GameState.Open)
        {
            currentTimeOfDay += Time.deltaTime * timeScale;

            int hours = (int)(currentTimeOfDay / 3600);
            int minutes = (int)((currentTimeOfDay % 3600) / 60);
            timeText.text = string.Format("{0:D2}:{1:D2}", hours, minutes);
            dayText.text = "Day " + DayCount;

            if (MenuPlanner.instance != null && MenuPlanner.instance.isSoldOut)
            {
                bool noCustomers = (restaurantManager.customers.Count == 0);
                bool anyDirtyTables = restaurantManager.tables.Any(t => t.isDirty);

                if (noCustomers && !anyDirtyTables)
                {
                    Debug.Log("완판 후 영업 종료.");
                    currentState = GameState.Closing;
                }
            }

            if (currentTimeOfDay >= 18 * 3600)
            {
                currentState = GameState.Closing;
                Debug.Log("영업 시간 종료!");
            }
        }

        if (currentState == GameState.Closing)
        {
            bool hasCustomers = restaurantManager.customers.Count > 0;
            bool hasDirtyTables = restaurantManager.tables.Any(t => t.isDirty);

            if (!hasCustomers && !hasDirtyTables)
            {
                currentState = GameState.Settlement;
                Debug.Log("정산을 시작합니다.");
            }
        }
    }

    public void MoveToNextDay()
    {
        if (currentState == GameState.Settlement)
        {
            if (EmployeeManager.Instance != null && InventoryManager.instance != null)
            {
                // 식탐 로직 공간
            }

            timeText.text = "09:00";
            todaysGold = 0;
            todaysCustomers = 0;
            if (MenuPlanner.instance != null)
            {
                MenuPlanner.instance.isSoldOut = false;
                MenuPlanner.instance.ClearDailyMenu();
            }

            if (menuPlannerUI != null)
            {
                menuPlannerUI.UpdateTodayMenuUI();
            }

            currentState = GameState.Preparing;
            currentTimeOfDay = 9 * 3600;
            DayCount += 1;
            dayText.text = "Day " + DayCount;
            Debug.Log("다음 날 준비.");

            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.GenerateTodayItems(FameManager.instance.CurrentFamePoints);
            }

            if (DayCount >= 1)
            {
                float currentFamePoints = FameManager.instance.CurrentFamePoints;
                EmployeeManager.Instance.GenerateApplicants((int)currentFamePoints);

                if (EmployeeUI_Controller.Instance != null)
                {
                    EmployeeUI_Controller.Instance.OpenPanel();
                }
            }

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveGame();
            }
        }
    }

    private void UpdateButtonUI()
    {
        if (PreparePanel == null) return; 

        PreparePanel.SetActive(currentState == GameState.Preparing);

        PreparePanel.SetActive(currentState == GameState.Preparing);
        NextDayButton.SetActive(currentState == GameState.Settlement);

        if (OpenButton != null)
        {
            OpenButton.gameObject.SetActive(currentState == GameState.Preparing);
        }

        bool isPreparing = (currentState == GameState.Preparing);

        if (TimeScaleButton != null)
        {
            TimeScaleButton.gameObject.SetActive(currentState == GameState.Open);
        }

        foreach (GameObject button in upgradeTableButtons)
        {
            if (button != null)
            {
                bool canShowButton = isPreparing && totalGoldAmount >= tablePrice;
                button.SetActive(canShowButton);
            }
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
            Debug.Log("영업 시작");
        }
    }

    public void SetStartButtonInteractable(bool isInteractable)
    {
        if (OpenButton != null)
        {
            OpenButton.interactable = isInteractable;
        }
    }

    public void closeSettlementPanal()
    {
        settlementPanel.SetActive(false);
        CheckButton.SetActive(false);
    }

    private void ShowSettlementPanal()
    {
        todaysGoldText.text = $"오늘 확득한 골드량: {todaysGold}";
        totalGoldText.text = $"총 보유 골드: {totalGoldAmount}";
        customerCountText.text = $"금일 방문객 수: {todaysCustomers}";

        settlementPanel.SetActive(true);
        CheckButton.SetActive(true);
    }

    public void AddGold(int amount)
    {
        totalGoldAmount += amount;
        todaysGold += amount;
        if (totalGold != null)
        {
            totalGold.text = totalGoldAmount.ToString();
        }
    }

    public void SpendGold(int amount)
    {
        totalGoldAmount -= amount;
        if (totalGold != null)
        {
            totalGold.text = totalGoldAmount.ToString();
        }
    }

    public void RefundIngredients(RecipeData recipe)
    {
        if (recipe == null || recipe.requiredIngredients == null || !recipe.requiredIngredients.Any())
            return;

        var ingredientsList = recipe.requiredIngredients;
        IngredientRequirement itemToRefund = ingredientsList[UnityEngine.Random.Range(0, ingredientsList.Count)];

        if (itemToRefund == null || string.IsNullOrEmpty(itemToRefund.ingredientID))
            return;

        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.AddIngredient(itemToRefund.ingredientID, 1);
            Debug.Log($"[재료 반환!] {itemToRefund.ingredientID} 1개를 돌려받았습니다!");
        }
    }

    public void AddCustomerCount()
    {
        todaysCustomers += 1;
    }

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

    public void OpenRecipeBook()
    {
        if (RecipeBook != null) RecipeBook.SetActive(true);
        if (panelBlocker != null) panelBlocker.SetActive(true);
        if (PopupManager != null) PopupManager.SetActive(true);
        CloseRecipeIngredientsPanel();
    }

    public void CloseRecipeBook()
    {
        if (RecipeBook != null) RecipeBook.SetActive(false);
        if (panelBlocker != null) panelBlocker.SetActive(false);
        if (PopupManager != null) PopupManager.SetActive(false);
    }

    public void OpenUpgradeTablePanel()
    {
        if (UpgradeTablePanel != null) UpgradeTablePanel.SetActive(true);
        if (panelBlocker != null) panelBlocker.SetActive(true);
        if (PopupManager != null) PopupManager.SetActive(true);
    }

    public void OpenMenuPlanner()
    {
        if (menuPlanner != null) menuPlanner.SetActive(true);
        if (RecipeSelection != null) RecipeSelection.SetActive(true);
        if (panelBlocker != null) panelBlocker.SetActive(true);
        if (PopupManager != null) PopupManager.SetActive(true);
    }

    public void CloseMenuPlanner()
    {
        if (menuPlanner != null) menuPlanner.SetActive(false);
        if (RecipeSelection != null) RecipeSelection.SetActive(false);
        if (panelBlocker != null) panelBlocker.SetActive(false);
        if (PopupManager != null) PopupManager.SetActive(false);
    }

    public void OpenRecipeSelection()
    {
        if (RecipeSelection != null) RecipeSelection.SetActive(true);
        if (panelBlocker != null) panelBlocker.SetActive(true);
        if (PopupManager != null) PopupManager.SetActive(true);
    }

    public void CloseRecipeSelection()
    {
        if (RecipeSelection != null) RecipeSelection.SetActive(false);
        if (panelBlocker != null) panelBlocker.SetActive(false);
    }

    public void OpenRecipeIngredientsPanel()
    {
        if (recipeIngredientsPanel != null) recipeIngredientsPanel.SetActive(true);
        if (PopupManager != null) PopupManager.SetActive(true);
    }

    public void CloseRecipeIngredientsPanel()
    {
        if (recipeIngredientsPanel != null) recipeIngredientsPanel.SetActive(false);
    }

    public void OpenInventoryPanel()
    {
        if (inventoryUI != null) inventoryUI.OpenInventory();
        CloseRecipeIngredientsPanel();
        if (panelBlocker != null) panelBlocker.SetActive(true);
        if (PopupManager != null) PopupManager.SetActive(true);
    }

    public void CloseInventoryPanel()
    {
        if (inventoryUI != null) inventoryUI.CloseInventory();
        if (panelBlocker != null) panelBlocker.SetActive(false);
        if (PopupManager != null) PopupManager.SetActive(false);
    }

    public void OpenShopPanel()
    {
        if (shopPanel != null) shopPanel.SetActive(true);
        if (panelBlocker != null) panelBlocker.SetActive(true);
        if (PopupManager != null) PopupManager.SetActive(true);
    }

    public void CloseShopPanel()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        if (panelBlocker != null) panelBlocker.SetActive(false);
        if (PopupManager != null) PopupManager.SetActive(false);
    }

    public void OpenRecipeShopPanel()
    {
        if (recipeShopPanel != null) recipeShopPanel.SetActive(true);
        if (ingredientShopPanel != null) ingredientShopPanel.SetActive(false);
    }

    public void OpenIngredientShopPanel()
    {
        if (ingredientShopPanel != null) ingredientShopPanel.SetActive(true);
        if (recipeShopPanel != null) recipeShopPanel.SetActive(false);
    }

    public void OpenEmployeeSubMenu()
    {
        Debug.Log("GameManager가 EmployeeUI_Controller에게 패널 열기를 요청합니다.");

        if (menuPlanner != null) CloseMenuPlanner();
        if (shopPanel != null) CloseShopPanel();

        if (PopupManager != null) PopupManager.SetActive(true);

        if (EmployeeUI_Controller.Instance != null)
        {
            EmployeeUI_Controller.Instance.OpenPanel();
        }

        StartCoroutine(ClearSelectedObjectDeferred());
    }

    public void CloseEmployeeSubMenu()
    {
        if (EmployeeUI_Controller.Instance != null)
        {
            EmployeeUI_Controller.Instance.ClosePanel();
        }

        if (PopupManager != null) PopupManager.SetActive(false);
    }

    public void OpenPauseMenu()
    {
        if (pauseMenuController != null)
        {
            pauseMenuController.gameObject.SetActive(true);
            PopupManager.SetActive(true);
            panelBlocker.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void ClosePauseMenu()
    {
        if (pauseMenuController != null)
        {
            pauseMenuController.gameObject.SetActive(false);
            PopupManager.SetActive(false);
            panelBlocker.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    private IEnumerator ClearSelectedObjectDeferred()
    {
        yield return null;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void AddTable(Transform buttonTransform)
    {
        if (TablePrefab == null)
        {
            Debug.LogError("GameManager에 Table Prefab이 연결되지 않았습니다!");
            return;
        }

        totalGoldAmount -= tablePrice;
        totalGold.text = totalGoldAmount.ToString();

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(buttonTransform.position);
        worldPosition.z = 0f;
        GameObject newTableObject = Instantiate(TablePrefab, worldPosition, Quaternion.identity);

        Table newTableComponent = newTableObject.GetComponent<Table>();
        if (newTableComponent != null && restaurantManager != null)
        {
            restaurantManager.tables.Add(newTableComponent);
        }

        Debug.Log($"테이블을 {worldPosition} 위치에 생성했습니다.");
    }

    public void HireAndSpawnEmployee(EmployeeData dataTemplate, GameObject prefabToSpawn)
    {
        EmployeeInstance newEmployee = new EmployeeInstance(dataTemplate);

        EmployeeManager.Instance.hiredEmployees.Add(newEmployee);
        Debug.Log($"{newEmployee.firstName}이(가) 고용되어 리스트에 추가되었습니다.");

        if (restaurantManager != null)
        {
            restaurantManager.SpawnSingleWorker(newEmployee, prefabToSpawn);
        }
        else
        {
            Debug.LogError("GameManager: Restaurant Manager가 연결되지 않았습니다!");
        }
    }
}