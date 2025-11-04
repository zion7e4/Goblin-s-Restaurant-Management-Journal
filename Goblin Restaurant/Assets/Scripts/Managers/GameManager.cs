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
    // [친구 기능] 싱글톤 인스턴스
    public static GameManager instance;

    // [추가 필드]
    [Header("Restaurant Management")]
    [Tooltip("Restaurant Manager 스크립트를 가진 오브젝트를 연결하세요.")]
    public RestaurantManager restaurantManager; // RestaurantManager 참조 추가

    // --- [친구 기능] 게임 상태 관리 ---
    public enum GameState { Preparing, Open, Closing }
    public GameState _currentState;
    public GameState currentState
    {
        get { return _currentState; }
        set
        {
            _currentState = value;
            UpdateButtonUI();

            if (_currentState == GameState.Closing)
            {
                ShowSettlementPanal();
            }
        }
    }
    // --- [친구 기능] 게임 시간 설정 ---
    public float dayDurationInSeconds = 600f;
    private float currentTimeOfDay;
    private float timeScale;
    private int speedState = 0;

    // --- [병합] 게임 상태 변수 ---
    [Header("게임 상태 변수 (병합됨)")]
    [Tooltip("현재 식당의 명성도 (직원 생성에 사용)")]
    public int currentFame = 100; // [사용자 기능]

    public int totalGoldAmount = 0; // [친구 기능]
    private int todaysGold = 0;
    private int todaysCustomers = 0;
    [SerializeField] private int DayCount = 1; // [친구 기능]의 'DayCount'를 메인으로 사용
    private Camera mainCamera;

    // --- [사용자 기능] 주인공 설정 ---
    [Header("주인공 설정 (병합됨)")]
    [Tooltip("주인공으로 사용할 직원의 설계도(EmployeeData 에셋)")]
    public EmployeeData mainCharacterTemplate;

    // --- [수정된 테스트 필드] 종족 데이터를 하나만 사용하고 프리팹을 색상별로 분리 ---
    [Header("TEST: 단일 종족 & 색상별 프리팹")]
    [Tooltip("테스트에 사용할 EmployeeData 에셋을 연결하세요 (예: Dwarf)")]
    public EmployeeData testSpeciesTemplate;

    [Tooltip("초록색 프리팹 (주인공)")]
    public GameObject greenPrefab; // 고블린 쉐프의 프리팹 (기본)
    [Tooltip("빨간색 프리팹 (테스트 1)")]
    public GameObject redPrefab;
    [Tooltip("파란색 프리팹 (테스트 2)")]
    public GameObject bluePrefab;
    // ---

    // --- [친구 기능] UI 및 프리팹 참조 ---
    [Header("UI 및 프리팹 (친구 코드)")]
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
    public GameObject menuPlanner; // 메뉴 기획 패널
    public GameObject RecipeSelection;
    public GameObject UpgradeTableButton;
    public GameObject recipeIngredientsPanel;
    public TextMeshProUGUI TimeScaleButtonText;
    public MenuPlannerUI_Controller menuPlannerUI;
    public InventoryUIController inventoryUI;
    public GameObject shopPanel; // 상점 패널
    public GameObject recipeShopPanel;
    public GameObject ingredientShopPanel;

    // --- [사용자 기능 추가] 직원 서브메뉴 UI ---
    [Header("직원 UI (병합됨)")]
    [Tooltip("PreparePanel에서 열릴 '직원 서브 메뉴' 패널을 연결하세요.")]
    public GameObject employeeSubMenuPanel;

    // [친구 기능]의 Awake
    private void Awake()
    {
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

    // [사용자 기능] 주인공 생성 함수
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


    // [병합] Start 함수 (수정됨: 클린 스폰 로직 및 테스트 직원 추가)
    void Start()
    {
        // 1. [클린 스폰] 이전에 스폰되었던 Employee 오브젝트를 모두 제거합니다.
        Employee[] existingWorkers = FindObjectsByType<Employee>(FindObjectsSortMode.None);
        foreach (Employee worker in existingWorkers)
        {
            Destroy(worker.gameObject);
        }

        // [친구 기능] 초기화
        currentState = GameState.Preparing;
        timeScale = (9 * 60 * 60) / dayDurationInSeconds;
        currentTimeOfDay = 9 * 3600;
        timeText.text = "09:00";
        dayText.text = "Day " + DayCount;
        totalGold.text = "Gold: " + totalGoldAmount;

        Time.timeScale = 1;
        TimeScaleButtonText.text = "X1";
        Debug.Log("오픈 준비 시간입니다.");
        mainCamera = Camera.main;

        // [사용자 기능 추가] 주인공 데이터 생성 및 고용 리스트에 추가 (고블린 쉐프)
        CreateMainCharacter();

        // ---------------------------------------------------
        // ★★★ TEST CODE: 단일 종족 데이터로 3가지 색상별 프리팹 스폰을 요청합니다. ★★★
        // ---------------------------------------------------
        if (restaurantManager == null)
        {
            Debug.LogError("Restaurant Manager가 연결되지 않았습니다! 직원 스폰 불가.");
            return;
        }

        List<(EmployeeInstance data, GameObject prefab)> workersToSpawn = new List<(EmployeeInstance, GameObject)>();

        // 1. 주인공 (고블린 쉐프) 데이터 추가 (Green Prefab 사용)
        EmployeeInstance mainWorker = EmployeeManager.Instance.hiredEmployees.FirstOrDefault(e => e.isProtagonist);
        if (mainWorker != null && greenPrefab != null)
        {
            workersToSpawn.Add((mainWorker, greenPrefab));
        }

        // 2. ★★★ 고용된 직원들을 맵에 스폰
        restaurantManager.SpawnWorkersWithPrefabs(workersToSpawn);
    }

    // [친구 기능] Update 
    void Update()
    {
        if (currentState == GameState.Open)
        {
            currentTimeOfDay += Time.deltaTime * timeScale;

            int hours = (int)(currentTimeOfDay / 3600);
            int minutes = (int)((currentTimeOfDay % 3600) / 60);
            timeText.text = string.Format("{0:D2}:{1:D2}", hours, minutes);
            dayText.text = "Day " + DayCount;

            if (currentTimeOfDay >= 18 * 3600)
            {
                currentState = GameState.Closing;
                Debug.Log("영업 종료");
            }
        }
    }

    // [병합] MoveToNextDay 함수 
    public void MoveToNextDay()
    {
        if (currentState == GameState.Closing)
        {
            // [친구 기능]
            timeText.text = "09:00";
            todaysGold = 0;
            todaysCustomers = 0;

            if (MenuPlanner.instance != null)
            {
                // MenuPlanner.instance.ClearDailyMenu(); // Assuming logic
            }

            if (menuPlannerUI != null)
            {
                menuPlannerUI.UpdateAllSlotsUI();
            }

            currentState = GameState.Preparing;
            currentTimeOfDay = 9 * 3600;
            DayCount += 1;
            dayText.text = "Day " + DayCount;
            Debug.Log("다음 날 준비를 시작합니다.");

            // [사용자 기능 추가] 7일마다 지원자 생성
            if ((DayCount - 1) % 7 == 0 && DayCount > 1)
            {
                EmployeeManager.Instance.GenerateApplicants(currentFame);
                Debug.Log($"[GameManager] {DayCount}일차 아침, 새로운 지원자들을 생성합니다.");
            }
        }
    }


    // ---------------------------------------------------
    // [친구 기능] 골드, 시간 배속, 정산창 관련 함수들 
    // ---------------------------------------------------

    private void UpdateButtonUI()
    {
        PreparePanel.SetActive(currentState == GameState.Preparing);
        NextDayButton.SetActive(currentState == GameState.Closing);

        bool isPreparing = (currentState == GameState.Preparing);

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
            currentState = GameState.Open;
            menuPlanner.SetActive(false);
            // MenuPlanner.instance.ConsumeIngredientsForToday(); // Assuming logic
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
        totalGold.text = "Gold: " + totalGoldAmount;
    }

    public void SpendGold(int amount)
    {
        totalGoldAmount -= amount;
        totalGold.text = "Gold: " + totalGoldAmount;
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

    // ---------------------------------------------------
    // 패널 여닫기 함수
    // ---------------------------------------------------

    public void OpenMenuPlanner()
    {
        if (menuPlanner != null) menuPlanner.SetActive(true);
        // 다른 패널 닫기
        if (shopPanel != null) CloseShopPanel();
        if (employeeSubMenuPanel != null) CloseEmployeeSubMenu();
    }

    public void CloseMenuPlanner()
    {
        if (menuPlanner != null) menuPlanner.SetActive(false);
    }

    public void OpenShopPanel()
    {
        if (shopPanel != null) shopPanel.SetActive(true);
        // 다른 패널 닫기
        if (menuPlanner != null) CloseMenuPlanner();
        if (employeeSubMenuPanel != null) CloseEmployeeSubMenu();
    }

    public void CloseShopPanel()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
    }

    // (나머지 Open/Close 함수들은 변경 없음)
    public void OpenRecipeSelection() { RecipeSelection.SetActive(true); }
    public void CloseRecipeSelection() { RecipeSelection.SetActive(false); }
    public void OpenRecipeIngredientsPanel() { recipeIngredientsPanel.SetActive(true); }
    public void CloseRecipeIngredientsPanel() { recipeIngredientsPanel.SetActive(false); }
    public void OpenInventoryPanel() { if (inventoryUI != null) inventoryUI.OpenInventory(); CloseRecipeIngredientsPanel(); }
    public void CloseInventoryPanel() { if (inventoryUI != null) inventoryUI.CloseInventory(); }
    public void OpenRecipeShopPanel() { recipeShopPanel.SetActive(true); ingredientShopPanel.SetActive(false); }
    public void OpenIngredientShopPanel() { ingredientShopPanel.SetActive(true); recipeShopPanel.SetActive(false); }


    // --- [사용자 기능 추가] 직원 서브메뉴 여닫기 함수 ---

    public void OpenEmployeeSubMenu()
    {
        Debug.Log("GameManager가 EmployeeUI_Controller에게 패널 열기를 요청합니다.");

        // 다른 패널 닫기
        if (menuPlanner != null) CloseMenuPlanner();
        if (shopPanel != null) CloseShopPanel();

        // EmployeeUI_Controller의 함수를 호출
        if (EmployeeUI_Controller.Instance != null)
        {
            EmployeeUI_Controller.Instance.OpenPanel();
        }

        // 패널을 연 직후, EventSystem의 포커스(선택)를 초기화합니다.
        StartCoroutine(ClearSelectedObjectDeferred());
    }

    public void CloseEmployeeSubMenu()
    {
        // EmployeeUI_Controller의 함수를 호출
        if (EmployeeUI_Controller.Instance != null)
        {
            EmployeeUI_Controller.Instance.ClosePanel();
        }
    }

    /// <summary>
    /// 다음 프레임에 EventSystem의 선택된 오브젝트를 null로 설정하여,
    /// UI 활성화 시 발생할 수 있는 원치 않는 버튼 클릭을 방지합니다.
    /// </summary>
    private IEnumerator ClearSelectedObjectDeferred()
    {
        yield return null;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            Debug.Log("EventSystem 포커스 초기화 완료.");
        }
    }

    // --- [친구 기능] 테이블 추가 함수 ---
    public void AddTable(Transform buttonTransform)
    {
        if (TablePrefab == null) return;
        totalGoldAmount -= tablePrice;
        totalGold.text = "Gold: " + totalGoldAmount;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(buttonTransform.position);
        worldPosition.z = 0f;
        GameObject newTableObject = Instantiate(TablePrefab, worldPosition, Quaternion.identity);
    }
    /// <summary>
    /// (UI 등에서 호출) 새로운 직원을 고용하고, 리스트에 추가한 뒤, 맵에 스폰합니다.
    /// </summary>
    /// <param name="dataTemplate">직원의 기본 정보가 되는 ScriptableObject (예: testSpeciesTemplate)</param>
    /// <param name="prefabToSpawn">직원의 외형이 될 프리팹 (예: redPrefab, bluePrefab 등)</param>
    public void HireAndSpawnEmployee(EmployeeData dataTemplate, GameObject prefabToSpawn)
    {
        // 1. 새로운 직원 인스턴스(데이터) 생성
        EmployeeInstance newEmployee = new EmployeeInstance(dataTemplate);
        // (필요시 newEmployee.firstName = "임의의 이름" 등으로 설정)

        // 2. EmployeeManager의 전체 직원 목록에 이 직원을 추가
        EmployeeManager.Instance.hiredEmployees.Add(newEmployee);
        Debug.Log($"{newEmployee.firstName}이(가) 고용되어 리스트에 추가되었습니다.");

        // 3. (1단계에서 만든) RestaurantManager의 스폰 함수를 즉시 호출
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