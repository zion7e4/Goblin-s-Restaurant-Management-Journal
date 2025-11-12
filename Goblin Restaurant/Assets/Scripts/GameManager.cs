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

    // [V1] RestaurantManager 참조 (V2는 instance로 접근했으나, V1의 명시적 연결 사용)
    [Header("Restaurant Management")]
    [Tooltip("Restaurant Manager 스크립트를 가진 오브젝트를 연결하세요.")]
    public RestaurantManager restaurantManager;

    // --- [V2의 정교한 게임 상태] ---
    public enum GameState { Preparing, Open, Closing, Settlement } // V2의 'Settlement' 상태 포함
    public GameState _currentState;
    public GameState currentState
    {
        get { return _currentState; }
        set
        {
            _currentState = value;
            UpdateButtonUI();

            if (_currentState == GameState.Settlement) // V2의 로직
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
    [Tooltip("현재 식당의 명성도 (직원 생성에 사용)")]
    public int currentFame = 100; // [V1]
    public int totalGoldAmount = 0; // [공통]
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
    // ---

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

    // [V2] UI 추가 필드
    public Button TimeScaleButton; // V2
    public GameObject panelBlocker; // V2
    public GameObject PopupManager; // V2
    public GameObject UpgradeTablePanel; // V2

    // [V1] 직원 UI 추가 필드
    [Header("직원 UI (V1)")]
    [Tooltip("PreparePanel에서 열릴 '직원 서브 메뉴' 패널을 연결하세요.")]
    public GameObject employeeSubMenuPanel; // V1 (EmployeeUI_Controller가 제어)

    // [V2] Input System
    private InputSystem_Actions inputActions;

    // [V2의 Awake] (Input System 초기화 포함)
    private void Awake()
    {
        inputActions = new InputSystem_Actions(); // V2

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

    // [V2] Input System 활성화/비활성화
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

    // [V1의 Start] (직원 스폰 로직 포함)
    void Start()
    {
        // 1. [클린 스폰] 이전에 스폰되었던 Employee 오브젝트를 모두 제거합니다.
       /* Employee[] existingWorkers = FindObjectsByType<Employee>(FindObjectsSortMode.None);
        foreach (Employee worker in existingWorkers)
        {
            Destroy(worker.gameObject);
        }
*/
        // [공통] 초기화
        currentState = GameState.Preparing;
        timeScale = (9 * 60 * 60) / dayDurationInSeconds;
        currentTimeOfDay = 9 * 3600;
        timeText.text = "09:00";
        dayText.text = "Day " + DayCount;
        totalGold.text = totalGoldAmount.ToString(); // V1의 포맷

        Time.timeScale = 1;
        TimeScaleButtonText.text = "X1";
        Debug.Log("오픈 준비 시간입니다.");
        mainCamera = Camera.main;

        // [V1] 주인공 데이터 생성 및 고용 리스트에 추가
        CreateMainCharacter();

        // [V1] 테스트 스폰 로직
        if (restaurantManager == null)
        {
            Debug.LogError("Restaurant Manager가 연결되지 않았습니다! 직원 스폰 불가.");
            return;
        }

        List<(EmployeeInstance data, GameObject prefab)> workersToSpawn = new List<(EmployeeInstance, GameObject)>();

        // 1. 주인공 (고블린 쉐프) 데이터 추가
        EmployeeInstance mainWorker = EmployeeManager.Instance.hiredEmployees.FirstOrDefault(e => e.isProtagonist);
        if (mainWorker != null && greenPrefab != null)
        {
            workersToSpawn.Add((mainWorker, greenPrefab));
        }

        // 2. ★★★ 고용된 직원들을 맵에 스폰
        // (참고: V1의 Start 로직은 기본 주인공만 스폰하고 있습니다. 
        // 빨간/파란 프리팹 스폰 로직은 HireAndSpawnEmployee 함수로 분리되었습니다.)
        restaurantManager.SpawnWorkersWithPrefabs(workersToSpawn);
    }

    // [V2의 Update] (정교한 상태 관리)
    void Update()
    {
        if (currentState == GameState.Open)
        {
            currentTimeOfDay += Time.deltaTime * timeScale;

            int hours = (int)(currentTimeOfDay / 3600);
            int minutes = (int)((currentTimeOfDay % 3600) / 60);
            timeText.text = string.Format("{0:D2}:{1:D2}", hours, minutes);
            dayText.text = "Day " + DayCount;

            // V2의 조기 마감 로직
            if (MenuPlanner.instance != null && MenuPlanner.instance.isSoldOut)
            {
                // V1의 restaurantManager 필드 사용 (V2의 instance 접근 수정)
                bool noCustomers = (restaurantManager.customers.Count == 0);
                bool anyDirtyTables = restaurantManager.tables.Any(t => t.isDirty);

                if (noCustomers && !anyDirtyTables)
                {
                    Debug.Log("완판 후 모든 손님 퇴장 및 테이블 청소 완료. 영업을 종료합니다.");
                    currentState = GameState.Closing; // 조기 마감
                }
            }

            // 정규 마감
            if (currentTimeOfDay >= 18 * 3600)
            {
                currentState = GameState.Closing;
                Debug.Log("영업 시간 종료! 남은 손님과 청소를 처리합니다.");
            }
        }

        if (currentState == GameState.Closing)
        {
            // V1의 restaurantManager 필드 사용 (V2의 instance 접근 수정)
            bool hasCustomers = restaurantManager.customers.Count > 0;
            bool hasDirtyTables = restaurantManager.tables.Any(t => t.isDirty);

            // 손님 없고, 더러운 테이블 없으면 '정산'으로
            if (!hasCustomers && !hasDirtyTables)
            {
                currentState = GameState.Settlement;
                Debug.Log("모든 손님 퇴장 및 청소 완료. 정산을 시작합니다.");
            }
        }
    }

    // [병합] MoveToNextDay (V2의 Settlement 체크 + V1의 지원자 생성 로직)
    public void MoveToNextDay()
    {
        if (currentState == GameState.Settlement) // V2의 상태 체크
        {
            if (EmployeeManager.Instance != null && InventoryManager.instance != null)
            {
                // "식탐" 특성을 가진 모든 직원을 찾습니다.
                foreach (EmployeeInstance emp in EmployeeManager.Instance.hiredEmployees)
                {
                    float stealChance = emp.GetTraitStealChance();
                    if (stealChance > 0 && UnityEngine.Random.Range(0f, 1f) < stealChance)
                    {
                        // 식탐 발동! 인벤토리에서 랜덤 재료 1개 훔치기
                        InventoryManager.instance.StealRandomIngredient(emp.firstName);
                    }
                }
            }

            // V2의 리셋 로직
            timeText.text = "09:00";
            todaysGold = 0;
            todaysCustomers = 0;
            if (MenuPlanner.instance != null)
            {
                MenuPlanner.instance.isSoldOut = false; // 완판 상태 초기화
                MenuPlanner.instance.ClearDailyMenu();
            }

            if (menuPlannerUI != null)
            {
                menuPlannerUI.UpdateAllSlotsUI();
            }

            // 공통 로직
            currentState = GameState.Preparing;
            currentTimeOfDay = 9 * 3600;
            DayCount += 1;
            dayText.text = "Day " + DayCount;
            Debug.Log("다음 날 준비를 시작합니다.");

            // [V1의 7일마다 지원자 생성 로직 추가]
            if ((DayCount - 1) % 7 == 0 && DayCount > 1)
            {
                EmployeeManager.Instance.GenerateApplicants(currentFame);
                Debug.Log($"[GameManager] {DayCount}일차 아침, 새로운 지원자들을 생성합니다.");
            }
        }
    }


    // [V2의 UpdateButtonUI] (TimeScaleButton 및 Settlement 상태 처리)
    private void UpdateButtonUI()
    {
        PreparePanel.SetActive(currentState == GameState.Preparing);
        NextDayButton.SetActive(currentState == GameState.Settlement); // V2

        bool isPreparing = (currentState == GameState.Preparing);

        if (TimeScaleButton != null) // V2
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

    // [V2의 OpenTheStore] (시간 리셋 및 StartDaySales 호출)
    public void OpenTheStore()
    {
        if (currentState == GameState.Preparing)
        {
            if (currentTimeOfDay > 9 * 3600) // V2
            {
                currentTimeOfDay = 9 * 3600;
                timeText.text = "09:00";
            }

            currentState = GameState.Open;
            if (menuPlanner != null) menuPlanner.SetActive(false);

            if (MenuPlanner.instance != null)
            {
                MenuPlanner.instance.ConsumeIngredientsForToday(); // V2
                MenuPlanner.instance.StartDaySales(); // V2
            }
            Debug.Log("영업 시작");
        }
    }

    // [공통] SetStartButtonInteractable
    public void SetStartButtonInteractable(bool isInteractable)
    {
        if (OpenButton != null)
        {
            OpenButton.interactable = isInteractable;
        }
    }

    // [공통] closeSettlementPanal
    public void closeSettlementPanal()
    {
        settlementPanel.SetActive(false);
        CheckButton.SetActive(false);
    }

    // [공통] ShowSettlementPanal
    private void ShowSettlementPanal()
    {
        todaysGoldText.text = $"오늘 확득한 골드량: {todaysGold}";
        totalGoldText.text = $"총 보유 골드: {totalGoldAmount}";
        customerCountText.text = $"금일 방문객 수: {todaysCustomers}";

        settlementPanel.SetActive(true);
        CheckButton.SetActive(true);
    }

    // [공통] AddGold (V1 포맷 사용)
    public void AddGold(int amount)
    {
        totalGoldAmount += amount;
        todaysGold += amount;
        totalGold.text = totalGoldAmount.ToString();
    }

    // [공통] SpendGold (V1 포맷 사용)
    public void SpendGold(int amount)
    {
        totalGoldAmount -= amount;
        totalGold.text = totalGoldAmount.ToString();
    }

    /// <summary>
    // (Employee.cs가 호출) 식재료 절약 성공 시 재료 1개를 반환합니다.
    /// <summary>
    // (Employee.cs가 호출) 식재료 절약 성공 시 재료 1개를 반환합니다.
    /// </summary>
    public void RefundIngredients(RecipeData recipe)
    {
        // 1. 레시피 데이터나 재료 목록이 비어있는지 확인합니다.
        if (recipe == null || recipe.requiredIngredients == null || !recipe.requiredIngredients.Any())
        {
            Debug.LogWarning("RefundIngredients: 반환할 재료 데이터를 찾을 수 없습니다.");
            return;
        }

        // 2. 레시피에 필요한 재료 목록(List<IngredientRequirement>)을 가져옵니다.
        var ingredientsList = recipe.requiredIngredients;

        // 3. 그 목록 중 1개를 랜덤으로 선택합니다. (예: 꼬치구이의 [ING01, ING02] 중 ING01 선택)
        IngredientRequirement itemToRefund = ingredientsList[UnityEngine.Random.Range(0, ingredientsList.Count)];

        if (itemToRefund == null || string.IsNullOrEmpty(itemToRefund.ingredientID))
        {
            Debug.LogWarning("RefundIngredients: 선택된 반환 아이템이 유효하지 않습니다.");
            return;
        }

        // 4. InventoryManager를 호출하여 재료 1개를 인벤토리에 다시 추가합니다.
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

    // [공통] AddCustomerCount
    public void AddCustomerCount()
    {
        todaysCustomers += 1;
    }

    // [공통] ChangeTimeScale
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
    // [병합] 패널 여닫기 함수 (V1의 패널 닫기 + V2의 팝업/블로커 관리)
    // ---------------------------------------------------

    public void OpenRecipeBook()
    {
        if (RecipeBook != null) RecipeBook.SetActive(true);
        if (panelBlocker != null) panelBlocker.SetActive(true);
        if (PopupManager != null) PopupManager.SetActive(true); // V2
        CloseRecipeIngredientsPanel();
    }

    public void CloseRecipeBook()
    {
        if (RecipeBook != null) RecipeBook.SetActive(false);
        if (panelBlocker != null) panelBlocker.SetActive(false);
        if (PopupManager != null) PopupManager.SetActive(false); // V2
    }

    // [V2]
    public void OpenUpgradeTablePanel()
    {
        if (UpgradeTablePanel != null) UpgradeTablePanel.SetActive(true);
        if (panelBlocker != null) panelBlocker.SetActive(true);
        if (PopupManager != null) PopupManager.SetActive(true);
    }

    // [병합]
    public void OpenMenuPlanner()
    {
        if (menuPlanner != null) menuPlanner.SetActive(true);
        if (PopupManager != null) PopupManager.SetActive(true); // V2
    }

    // [V2]
    public void CloseMenuPlanner()
    {
        if (menuPlanner != null) menuPlanner.SetActive(false);
        if (PopupManager != null) PopupManager.SetActive(false);
    }

    // [V2]
    public void OpenRecipeSelection()
    {
        if (RecipeSelection != null) RecipeSelection.SetActive(true);
        if (panelBlocker != null) panelBlocker.SetActive(true);
        if (PopupManager != null) PopupManager.SetActive(true);
    }

    // [V2]
    public void CloseRecipeSelection()
    {
        if (RecipeSelection != null) RecipeSelection.SetActive(false);
        if (panelBlocker != null) panelBlocker.SetActive(false);
    }

    // [V2]
    public void OpenRecipeIngredientsPanel()
    {
        if (recipeIngredientsPanel != null) recipeIngredientsPanel.SetActive(true);
        if (PopupManager != null) PopupManager.SetActive(true);
    }

    // [V2]
    public void CloseRecipeIngredientsPanel()
    {
        if (recipeIngredientsPanel != null) recipeIngredientsPanel.SetActive(false);
    }

    // [V2]
    public void OpenInventoryPanel()
    {
        if (inventoryUI != null) inventoryUI.OpenInventory();
        CloseRecipeIngredientsPanel();
        if (panelBlocker != null) panelBlocker.SetActive(true);
        if (PopupManager != null) PopupManager.SetActive(true);
    }

    // [V2]
    public void CloseInventoryPanel()
    {
        if (inventoryUI != null) inventoryUI.CloseInventory();
        if (panelBlocker != null) panelBlocker.SetActive(false);
        if (PopupManager != null) PopupManager.SetActive(false);
    }

    // [병합]
    public void OpenShopPanel()
    {
        if (shopPanel != null) shopPanel.SetActive(true);
        if (panelBlocker != null) panelBlocker.SetActive(true); // V2
        if (PopupManager != null) PopupManager.SetActive(true); // V2
    }

    // [V2]
    public void CloseShopPanel()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        if (panelBlocker != null) panelBlocker.SetActive(false);
        if (PopupManager != null) PopupManager.SetActive(false);
    }

    // [공통]
    public void OpenRecipeShopPanel()
    {
        if (recipeShopPanel != null) recipeShopPanel.SetActive(true);
        if (ingredientShopPanel != null) ingredientShopPanel.SetActive(false);
    }

    // [공통]
    public void OpenIngredientShopPanel()
    {
        if (ingredientShopPanel != null) ingredientShopPanel.SetActive(true);
        if (recipeShopPanel != null) recipeShopPanel.SetActive(false);
    }


    // --- [V1의 직원 서브메뉴 함수] + V2의 팝업/블로커 로직 추가 ---

    // [병합]
    public void OpenEmployeeSubMenu()
    {
        Debug.Log("GameManager가 EmployeeUI_Controller에게 패널 열기를 요청합니다.");

        // V1: 다른 패널 닫기
        if (menuPlanner != null) CloseMenuPlanner();
        if (shopPanel != null) CloseShopPanel();

        // V2: 팝업/블로커 활성화
        //if (panelBlocker != null) panelBlocker.SetActive(true);
        if (PopupManager != null) PopupManager.SetActive(true);

        // V1: EmployeeUI_Controller 호출
        if (EmployeeUI_Controller.Instance != null)
        {
            EmployeeUI_Controller.Instance.OpenPanel();
        }

        StartCoroutine(ClearSelectedObjectDeferred());
    }

    // [병합]
    public void CloseEmployeeSubMenu()
    {
        // V1
        if (EmployeeUI_Controller.Instance != null)
        {
            EmployeeUI_Controller.Instance.ClosePanel();
        }

        // V2
        //if (panelBlocker != null) panelBlocker.SetActive(false);
        if (PopupManager != null) PopupManager.SetActive(false);
    }

    // [V1]
    private IEnumerator ClearSelectedObjectDeferred()
    {
        yield return null;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            Debug.Log("EventSystem 포커스 초기화 완료.");
        }
    }

    // --- [V2의 AddTable] (RestaurantManager와 연동) ---
    public void AddTable(Transform buttonTransform)
    {
        if (TablePrefab == null)
        {
            Debug.LogError("GameManager에 Table Prefab이 연결되지 않았습니다!");
            return;
        }

        // V1 포맷 사용
        totalGoldAmount -= tablePrice;
        totalGold.text = totalGoldAmount.ToString();

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(buttonTransform.position);
        worldPosition.z = 0f;
        GameObject newTableObject = Instantiate(TablePrefab, worldPosition, Quaternion.identity);

        // V2의 핵심 로직 (instance -> V1의 restaurantManager 필드로 수정)
        Table newTableComponent = newTableObject.GetComponent<Table>();
        if (newTableComponent != null && restaurantManager != null)
        {
            restaurantManager.tables.Add(newTableComponent);
        }

        Debug.Log($"테이블을 {worldPosition} 위치에 생성했습니다.");
    }

    // --- [V1] 직원 고용 및 스폰 함수 ---
    public void HireAndSpawnEmployee(EmployeeData dataTemplate, GameObject prefabToSpawn)
    {
        // 1. 새로운 직원 인스턴스(데이터) 생성
        EmployeeInstance newEmployee = new EmployeeInstance(dataTemplate);

        // 2. EmployeeManager의 전체 직원 목록에 이 직원을 추가
        EmployeeManager.Instance.hiredEmployees.Add(newEmployee);
        Debug.Log($"{newEmployee.firstName}이(가) 고용되어 리스트에 추가되었습니다.");

        // 3. RestaurantManager의 스폰 함수를 즉시 호출
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