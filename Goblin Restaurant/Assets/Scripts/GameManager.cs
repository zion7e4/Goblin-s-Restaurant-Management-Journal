using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem; // V2에서 사용
using TMPro;
using UnityEngine.EventSystems; // V1에서 사용
using System.Collections; // V1에서 사용
using System.Linq;

// 역할: 게임의 시간, 명성, 돈, 게임 상태 등 전반적인 상태를 관리합니다.
public class GameManager : MonoBehaviour
{
    // [공통] 싱글톤 인스턴스
    public static GameManager instance;

    // [V1] RestaurantManager 참조
    [Header("Restaurant Management")]
    [Tooltip("Restaurant Manager 스크립트를 가진 오브젝트를 연결하세요.")]
    public RestaurantManager restaurantManager;

    // --- [V2의 정교한 게임 상태] ---
    public enum GameState { Preparing, Open, Closing, Settlement }
    public GameState _currentState;
    public GameState currentState
    {
        get { return _currentState; }
        set
        {
            _currentState = value;
            UpdateButtonUI(); // 상태 변경 시 UI 즉시 업데이트

            if (_currentState == GameState.Settlement)
            {
                ShowSettlementPanal();
            }
        }
    }

    // --- [공통] 게임 시간 설정 ---
    public float dayDurationInSeconds = 600f;
    private float currentTimeOfDay;
    private float timeScale;
    private int speedState = 0;

    // --- [병합] 게임 상태 변수 ---
    [Header("게임 상태 변수 (병합됨)")]
    public int totalGoldAmount = 0;
    private int todaysGold = 0;
    private int todaysCustomers = 0;
    [SerializeField] private int DayCount = 1;
    private Camera mainCamera;

    // --- [V1] 주인공 및 직원 테스트 설정 ---
    [Header("주인공 설정 (V1)")]
    [Tooltip("주인공으로 사용할 직원의 설계도(EmployeeData 에셋)")]
    public EmployeeData mainCharacterTemplate;

    [Header("TEST: 단일 종족 & 색상별 프리팹 (V1)")]
    [Tooltip("테스트에 사용할 EmployeeData 에셋을 연결하세요 (예: Dwarf)")]
    public EmployeeData testSpeciesTemplate;
    [Tooltip("초록색 프리팹 (주인공)")]
    public GameObject greenPrefab;
    [Tooltip("빨간색 프리팹 (테스트 1)")]
    public GameObject redPrefab;
    [Tooltip("파란색 프리팹 (테스트 2)")]
    public GameObject bluePrefab;

    // --- [병합] UI 및 프리팹 참조 ---
    [Header("UI 및 프리팹 (병합됨)")]
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

    [Header("직원 UI (V1)")]
    [Tooltip("PreparePanel에서 열릴 '직원 서브 메뉴' 패널을 연결하세요.")]
    public GameObject employeeSubMenuPanel;

    // [V2] Input System
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

    // [V1] 주인공 생성 함수
    void CreateMainCharacter()
    {
        if (mainCharacterTemplate != null && EmployeeManager.Instance != null)
        {
            if (!EmployeeManager.Instance.hiredEmployees.Any(e => e.isProtagonist))
            {
                EmployeeInstance mainCharacter = new EmployeeInstance(mainCharacterTemplate);
                EmployeeManager.Instance.hiredEmployees.Add(mainCharacter);
                Debug.Log($"주인공 '{mainCharacter.firstName}'이(가) 식당에 합류했습니다! (데이터 추가)");
            }
        }
    }

    void Start()
    {
        /* Employee[] existingWorkers = FindObjectsByType<Employee>(FindObjectsSortMode.None);
         foreach (Employee worker in existingWorkers)
         {
             Destroy(worker.gameObject);
         }
 */
        // [공통] 초기화
        currentState = GameState.Preparing; // <-- 이 시점에 UpdateButtonUI()가 호출됨
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
        if (mainWorker != null && greenPrefab != null)
        {
            workersToSpawn.Add((mainWorker, greenPrefab));
        }

        restaurantManager.SpawnWorkersWithPrefabs(workersToSpawn);

        if (ShopManager.Instance != null)
        {
            // (참고: currentFame은 GameManager의 인스펙터 값입니다.)
            ShopManager.Instance.GenerateTodayItems(FameManager.instance.CurrentFamePoints);
            Debug.Log("[GameManager] 테스트를 위해 '오늘의 상품'을 즉시 생성합니다.");
        }
        else
        {
            Debug.LogError("[GameManager] ShopManager.Instance가 null이라 '오늘의 상품'을 생성할 수 없습니다!");
        }
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
                    Debug.Log("완판 후 모든 손님 퇴장 및 테이블 청소 완료. 영업을 종료합니다.");
                    currentState = GameState.Closing;
                }
            }

            if (currentTimeOfDay >= 18 * 3600)
            {
                currentState = GameState.Closing;
                Debug.Log("영업 시간 종료! 남은 손님과 청소를 처리합니다.");
            }
        }

        if (currentState == GameState.Closing)
        {
            bool hasCustomers = restaurantManager.customers.Count > 0;
            bool hasDirtyTables = restaurantManager.tables.Any(t => t.isDirty);

            if (!hasCustomers && !hasDirtyTables)
            {
                currentState = GameState.Settlement;
                Debug.Log("모든 손님 퇴장 및 청소 완료. 정산을 시작합니다.");
            }
        }
    }

    // [병합] MoveToNextDay (Script 2의 "식탐" 특성 로직 포함)
    public void MoveToNextDay()
    {
        if (currentState == GameState.Settlement)
        {
            // ▼▼▼ [기능 1] "식탐" 특성 로직 (Script 2) ▼▼▼
            if (EmployeeManager.Instance != null && InventoryManager.instance != null)
            {
                foreach (EmployeeInstance emp in EmployeeManager.Instance.hiredEmployees)
                {
                    // (TODO: EmployeeInstance.cs에 GetTraitStealChance() 함수 구현 필요)
                    // float stealChance = emp.GetTraitStealChance();
                    float stealChance = 0f; // 임시 0
                    if (stealChance > 0 && UnityEngine.Random.Range(0f, 1f) < stealChance)
                    {
                        // (TODO: InventoryManager.cs에 StealRandomIngredient() 함수 구현 필요)
                        // InventoryManager.instance.StealRandomIngredient(emp.firstName);
                    }
                }
            }
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

            // V2의 리셋 로직
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
            Debug.Log("다음 날 준비를 시작합니다.");

            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.GenerateTodayItems(FameManager.instance.CurrentFamePoints);
            }

            if (DayCount >= 1)
            {
                float currentFamePoints = FameManager.instance.CurrentFamePoints;

                EmployeeManager.Instance.GenerateApplicants((int)currentFamePoints);

                Debug.Log($"[GameManager] {DayCount}일차 아침, 새로운 지원자들을 생성합니다. (현재 명성: {(int)currentFamePoints})");
            }
        }
    }


    // [병합] UpdateButtonUI (Script 1의 "OpenButton" 제어 로직 포함)
    private void UpdateButtonUI()
    {
        PreparePanel.SetActive(currentState == GameState.Preparing);
        NextDayButton.SetActive(currentState == GameState.Settlement);

        // ▼▼▼ [기능 2] OpenButton 제어 로직 (Script 1) ▼▼▼
        if (OpenButton != null)
        {
            OpenButton.gameObject.SetActive(currentState == GameState.Preparing);
        }
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

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

    // [V2의 OpenTheStore]
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
        totalGold.text = totalGoldAmount.ToString();
    }

    public void SpendGold(int amount)
    {
        totalGoldAmount -= amount;
        totalGold.text = totalGoldAmount.ToString();
    }

    // ▼▼▼ [기능 3] 재료 반환 함수 (Script 2) ▼▼▼
    /// <summary>
    /// (Employee.cs가 호출) 식재료 절약 성공 시 재료 1개를 반환합니다.
    /// </summary>
    public void RefundIngredients(RecipeData recipe)
    {
        if (recipe == null || recipe.requiredIngredients == null || !recipe.requiredIngredients.Any())
        {
            Debug.LogWarning("RefundIngredients: 반환할 재료 데이터를 찾을 수 없습니다.");
            return;
        }

        var ingredientsList = recipe.requiredIngredients;
        IngredientRequirement itemToRefund = ingredientsList[UnityEngine.Random.Range(0, ingredientsList.Count)];

        if (itemToRefund == null || string.IsNullOrEmpty(itemToRefund.ingredientID))
        {
            Debug.LogWarning("RefundIngredients: 선택된 반환 아이템이 유효하지 않습니다.");
            return;
        }

        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.AddIngredient(itemToRefund.ingredientID, 1);
            Debug.Log($"[재료 반환!] {itemToRefund.ingredientID} 1개를 돌려받았습니다!");
        }
        else
        {
            Debug.LogError("RefundIngredients: InventoryManager.instance를 찾을 수 없습니다!");
        }
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

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

    // ---------------------------------------------------
    // [병합] 패널 여닫기 함수 (Script 2의 Open/Close RecipeSelection 포함)
    // ---------------------------------------------------

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

        //if (panelBlocker != null) panelBlocker.SetActive(true);
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

        //if (panelBlocker != null) panelBlocker.SetActive(false);
        if (PopupManager != null) PopupManager.SetActive(false);
    }

    private IEnumerator ClearSelectedObjectDeferred()
    {
        yield return null;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            Debug.Log("EventSystem 포커스 초기화 완료.");
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