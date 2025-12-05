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
    [SerializeField] private int DayCount = 1;
    private Camera mainCamera;

    [Header("주인공 설정")]
    public EmployeeData mainCharacterTemplate;

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

    // UI 패널들
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
    public GameObject QuestPanel;

    public Button TimeScaleButton;
    public GameObject panelBlocker;
    public GameObject PopupManager;
    public GameObject UpgradeTablePanel;

    // [삭제됨] 예전 employeeSubMenuPanel 변수는 더 이상 사용하지 않음

    [Header("사이드 메뉴 버튼")]
    public Button btnRecipeBook;
    public Button btnEmployee;

    [Header("기능 해금 상태")]
    public bool isRecipeUnlocked = false;
    public bool isEmployeeUnlocked = false;

    private InputSystem_Actions inputActions;

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

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    void CreateMainCharacter()
    {
        if (mainCharacterTemplate != null && EmployeeManager.Instance != null)
        {
            if (!EmployeeManager.Instance.hiredEmployees.Any(e => e.isProtagonist))
            {
                EmployeeInstance mainCharacter = new EmployeeInstance(mainCharacterTemplate);
                EmployeeManager.Instance.hiredEmployees.Add(mainCharacter);
                Debug.Log($"주인공 '{mainCharacter.firstName}'이(가) 식당에 합류했습니다!");
            }
        }
    }

    void Start()
    {
        currentState = GameState.Preparing;
        timeScale = (9 * 60 * 60) / dayDurationInSeconds;
        currentTimeOfDay = 9 * 3600;
        timeText.text = "09:00";
        dayText.text = "Day " + DayCount;
        totalGold.text = totalGoldAmount.ToString();

        Time.timeScale = 1;
        TimeScaleButtonText.text = "X1";
        Debug.Log("오픈 준비 시간입니다.");
        mainCamera = Camera.main;

        CreateMainCharacter();

        if (restaurantManager == null)
        {
            Debug.LogError("Restaurant Manager가 연결되지 않았습니다! 직원 스폰 불가.");
            return;
        }

        List<(EmployeeInstance data, GameObject prefab)> workersToSpawn = new List<(EmployeeInstance, GameObject)>();
        EmployeeInstance mainWorker = EmployeeManager.Instance.hiredEmployees.FirstOrDefault(e => e.isProtagonist);

        if (mainWorker != null && restaurantManager.employeePrefab != null)
        {
            workersToSpawn.Add((mainWorker, restaurantManager.employeePrefab));
        }

        restaurantManager.SpawnWorkersWithPrefabs(workersToSpawn);

        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.GenerateTodayItems(FameManager.instance.CurrentFamePoints);
        }

        UpdateLockedButtons();
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

            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.GenerateTodayItems(FameManager.instance.CurrentFamePoints);
            }

            if (isEmployeeUnlocked && DayCount >= 1)
            {
                EmployeeManager.Instance.GenerateApplicants((int)FameManager.instance.CurrentFamePoints);
                Debug.Log($"[GameManager] {DayCount}일차 아침, 새로운 지원자 생성.");

                if (NotificationController.instance != null)
                    NotificationController.instance.ShowNotification("새로운 지원자가 도착했습니다!");
            }
        }
    }

    private void UpdateButtonUI()
    {
        PreparePanel.SetActive(currentState == GameState.Preparing);
        NextDayButton.SetActive(currentState == GameState.Settlement);

        if (OpenButton != null) OpenButton.gameObject.SetActive(currentState == GameState.Preparing);

        bool isPreparing = (currentState == GameState.Preparing);

        if (TimeScaleButton != null) TimeScaleButton.gameObject.SetActive(currentState == GameState.Open);

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
            // 1. (기존 로직) 시간 초기화
            if (currentTimeOfDay > 9 * 3600) currentTimeOfDay = 9 * 3600;

            // ================================================================
            // ★ [추가됨] 미지정(Unassigned) 직원을 자동으로 올라운더로 변경
            // ================================================================
            if (EmployeeManager.Instance != null)
            {
                bool roleChanged = false; // 변경된 사항이 있는지 체크

                foreach (var emp in EmployeeManager.Instance.hiredEmployees)
                {
                    // 직원이 '대기(미지정)' 상태라면?
                    if (emp.assignedRole == EmployeeRole.Unassigned)
                    {
                        emp.assignedRole = EmployeeRole.AllRounder; // 올라운더로 변경
                        Debug.Log($"[자동배치] {emp.firstName}이(가) 영업 시작과 함께 '올라운더'로 배치되었습니다.");
                        roleChanged = true;
                    }
                }

                // 역할이 바뀌었으니 시너지도 다시 계산해야 함
                if (roleChanged && SynergyManager.Instance != null)
                {
                    SynergyManager.Instance.UpdateActiveSynergies(EmployeeManager.Instance.hiredEmployees);
                }

                // (선택 사항) 만약 배치 UI가 켜져 있다면 갱신 (보통 영업 시작하면 UI 닫히니 필수는 아님)
                if (roleChanged && EmployeeUI_Controller.Instance != null)
                {
                    // 데이터가 바뀌었으니 UI도 갱신해달라고 요청 (혹시 켜져 있을 경우 대비)
                    // EmployeeUI_Controller.Instance.UpdateAssignmentUI(); 
                }
            }
            // ================================================================

            // 2. (기존 로직) 상태 변경 및 영업 시작
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
        if (OpenButton != null) OpenButton.interactable = isInteractable;
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
        totalGold.text = totalGoldAmount.ToString();

        QuestManager.Instance.SetProgress(QuestTargetType.Collect, "골드 보유량", totalGoldAmount);
    }

    public void SpendGold(int amount)
    {
        totalGoldAmount -= amount;
        totalGold.text = totalGoldAmount.ToString();
    }

    public void RefundIngredients(RecipeData recipe)
    {
        if (recipe == null || recipe.requiredIngredients == null) return;
        var ingredientsList = recipe.requiredIngredients;
        if (ingredientsList.Count == 0) return;

        var itemToRefund = ingredientsList[UnityEngine.Random.Range(0, ingredientsList.Count)];
        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.AddIngredient(itemToRefund.ingredientID, 1);
        }
    }

    public void AddCustomerCount() { todaysCustomers += 1; }

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

    // --- 패널 관리 ---

    public void OpenQuestPanel()
    {
        if (QuestPanel != null) QuestPanel.SetActive(true);
        if (panelBlocker != null) panelBlocker.SetActive(true);
        if (PopupManager != null) PopupManager.SetActive(true);
    }

    public void CloseQuestPanel()
    {
        if (QuestPanel != null) QuestPanel.SetActive(false);
        if (panelBlocker != null) panelBlocker.SetActive(false);
        if (PopupManager != null) PopupManager.SetActive(false);
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

    // =========================================================================
    // ★★★ [수정됨] 직원 관리 메뉴 열기 (OpenEmployeeMenu) ★★★
    // =========================================================================
    public void OpenEmployeeMenu()
    {
        Debug.Log("GameManager가 직원 관리(태블릿) 메뉴를 엽니다.");

        // 다른 겹칠 수 있는 패널들 닫기
        if (menuPlanner != null) CloseMenuPlanner();
        if (shopPanel != null) CloseShopPanel();
        if (inventoryUI != null) CloseInventoryPanel();

        if (PopupManager != null) PopupManager.SetActive(true);

        // EmployeeUI_Controller를 통해 허브 화면 열기
        if (EmployeeUI_Controller.Instance != null)
        {
            EmployeeUI_Controller.Instance.OpenPanel();
        }

        StartCoroutine(ClearSelectedObjectDeferred());
    }

    /// <summary>
    /// 직원 메뉴 닫기
    /// </summary>
    public void CloseEmployeeMenu()
    {
        if (EmployeeUI_Controller.Instance != null)
        {
            EmployeeUI_Controller.Instance.ClosePanel();
        }

        if (PopupManager != null) PopupManager.SetActive(false);
    }

    private IEnumerator ClearSelectedObjectDeferred()
    {
        yield return null;
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    public void AddTable(Transform buttonTransform)
    {
        if (TablePrefab == null) return;
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
    }

    public void HireAndSpawnEmployee(EmployeeData dataTemplate, GameObject prefabToSpawn)
    {
        EmployeeInstance newEmployee = new EmployeeInstance(dataTemplate);
        EmployeeManager.Instance.hiredEmployees.Add(newEmployee);
        if (restaurantManager != null)
        {
            restaurantManager.SpawnSingleWorker(newEmployee, prefabToSpawn);
        }
    }

    private void UpdateLockedButtons()
    {
        if (btnRecipeBook != null) btnRecipeBook.interactable = isRecipeUnlocked;
        if (btnEmployee != null) btnEmployee.interactable = isEmployeeUnlocked;
    }

    public void UnlockRecipeSystem()
    {
        isRecipeUnlocked = true;
        UpdateLockedButtons();
        if (NotificationController.instance != null)
            NotificationController.instance.ShowNotification("레시피 도감 기능이 해금되었습니다!");
    }

    public void UnlockEmployeeSystem()
    {
        isEmployeeUnlocked = true;
        UpdateLockedButtons();
        if (NotificationController.instance != null)
            NotificationController.instance.ShowNotification("직원 관리 기능이 해금되었습니다!");
    }
}
