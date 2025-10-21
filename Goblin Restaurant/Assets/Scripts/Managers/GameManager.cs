using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.EventSystems; // EventSystem 사용을 위해 추가
using System.Collections; // 코루틴을 사용하지 않지만, 이전 코드 흐름을 유지하기 위해 추가

// 역할: 게임의 시간, 명성, 돈, 게임 상태 등 전반적인 상태를 관리합니다.
public class GameManager : MonoBehaviour
{
    // [친구 기능] 싱글톤 인스턴스
    public static GameManager instance;

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
    public GameObject employeeSubMenuPanel; // ★★★ 1. 여기에 새 '직원 서브 메뉴' 패널 연결

    // --- [친구 기능] 입력 시스템 ---
    private InputSystem_Actions inputActions;

    // [친구 기능]의 Awake
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

    // --- [친구 기능] 입력 시스템 활성화/비활성화 ---
    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    // [병합] Start 함수
    void Start()
    {
        // [친구 기능]
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

        // [사용자 기능 추가]
        CreateMainCharacter();
    }

    // [사용자 기능] 주인공 생성 함수
    void CreateMainCharacter()
    {
        if (mainCharacterTemplate != null)
        {
            EmployeeInstance mainCharacter = new EmployeeInstance(mainCharacterTemplate);
            EmployeeManager.Instance.hiredEmployees.Add(mainCharacter);
            Debug.Log($"주인공 '{mainCharacter.firstName}'이(가) 식당에 합류했습니다!");
        }
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
                MenuPlanner.instance.ClearDailyMenu();
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
    // [친구 기능] 골드, 시간 배속, 정산창 관련 함수들 (수정 없이 유지)
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
            MenuPlanner.instance.ConsumeIngredientsForToday();
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
    // [친구 기능] + [병합] 패널 여닫기 함수
    // (서로 다른 패널이 겹치지 않게 닫는 로직 추가)
    // ---------------------------------------------------

    public void OpenMenuPlanner()
    {
        if (menuPlanner != null) menuPlanner.SetActive(true);
        // 다른 패널 닫기
        if (shopPanel != null) shopPanel.SetActive(false);
        if (employeeSubMenuPanel != null) employeeSubMenuPanel.SetActive(false); // [추가]
    }

    public void CloseMenuPlanner()
    {
        if (menuPlanner != null) menuPlanner.SetActive(false);
    }

    public void OpenShopPanel()
    {
        if (shopPanel != null) shopPanel.SetActive(true);
        // 다른 패널 닫기
        if (menuPlanner != null) menuPlanner.SetActive(false);
        if (employeeSubMenuPanel != null) employeeSubMenuPanel.SetActive(false); // [추가]
    }

    public void CloseShopPanel()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
    }

    // (친구의 다른 Open/Close 함수들도 여기에... 예: RecipeSelection, Inventory 등)
    // ...
    public void OpenRecipeSelection() { RecipeSelection.SetActive(true); }
    public void CloseRecipeSelection() { RecipeSelection.SetActive(false); }
    public void OpenRecipeIngredientsPanel() { recipeIngredientsPanel.SetActive(true); }
    public void CloseRecipeIngredientsPanel() { recipeIngredientsPanel.SetActive(false); }
    public void OpenInventoryPanel() { inventoryUI.OpenInventory(); CloseRecipeIngredientsPanel(); }
    public void CloseInventoryPanel() { inventoryUI.CloseInventory(); }
    public void OpenRecipeShopPanel() { recipeShopPanel.SetActive(true); ingredientShopPanel.SetActive(false); }
    public void OpenIngredientShopPanel() { ingredientShopPanel.SetActive(true); recipeShopPanel.SetActive(false); }


    // --- [사용자 기능 추가] 직원 서브메뉴 여닫기 함수 ---

    /// <summary>
    /// '직원 서브 메뉴' 패널을 엽니다. 
    /// (PreparePanel의 'Employee' 버튼 OnClick에 이 함수를 연결하세요)
    /// </summary>
    // public GameObject employeeSubMenuPanel; // <- 이 변수는 이제 EmployeeUI_Controller가 가짐 (삭제)

    public void OpenEmployeeSubMenu()
    {
        Debug.Log("GameManager가 EmployeeUI_Controller에게 패널 열기를 요청합니다.");

        // 다른 패널 닫기 (PreparePanel은 안 닫음)
        if (menuPlanner != null) menuPlanner.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);

        // EmployeeUI_Controller의 함수를 호출
        if (EmployeeUI_Controller.Instance != null)
        {
            EmployeeUI_Controller.Instance.OpenPanel();
        }

        // 패널을 연 직후, EventSystem의 포커스(선택)를 초기화합니다.
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void CloseEmployeeSubMenu()
    {
        // EmployeeUI_Controller의 함수를 호출
        if (EmployeeUI_Controller.Instance != null)
        {
            EmployeeUI_Controller.Instance.ClosePanel();
        }
    }


    // --- [친구 기능] 테이블 추가 함수 ---
    public void AddTable(Transform buttonTransform)
    {
        // ... (친구의 AddTable 코드 원본) ...
        if (TablePrefab == null) return;
        totalGoldAmount -= tablePrice;
        totalGold.text = "Gold: " + totalGoldAmount;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(buttonTransform.position);
        worldPosition.z = 0f;
        GameObject newTableObject = Instantiate(TablePrefab, worldPosition, Quaternion.identity);
        Table newTableComponent = newTableObject.GetComponent<Table>();
        if (newTableComponent != null && RestaurantManager.instance != null)
        {
            RestaurantManager.instance.tables.Add(newTableComponent);
        }
    }
}
