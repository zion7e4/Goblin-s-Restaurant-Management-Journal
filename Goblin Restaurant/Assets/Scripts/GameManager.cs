using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;

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
    [SerializeField] private int _dayCount = 1;
    public int DayCount
    {
        get { return _dayCount; }
        set { _dayCount = value; }
    }
    private Camera mainCamera;

    [Header("주인공 설정")]
    public EmployeeData mainCharacterTemplate;

    [Header("UI 및 프리팹")]
    public List<GameObject> upgradeTableButtons;
    public int tablePrice = 100;
    public int contertopPrice = 1000;
    public float savedGameSpeed = 1f;

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
    public GameObject UpgradeCountertop;

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
    public GameObject pausePanel;

    public Button TimeScaleButton;
    public GameObject panelBlocker;
    public GameObject PopupManager;
    public GameObject UpgradeTablePanel;
    public GameObject preparePanelToggleButton;
    public GameObject endingPanel; 
    public GameObject watchEndingButton;


    [Header("사이드 메뉴 버튼")]
    public Button btnRecipeBook;
    public Button btnEmployee;
    public Button btnSaveGame;
    public Button btnLoadGame;

    [Header("기능 해금 상태")]
    public bool isRecipeUnlocked = false;
    public bool isEmployeeUnlocked = false;
    public bool isPaused = false;

    private InputSystem_Actions inputActions;
    private ClosePopupInput closePopupInput;

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
        
        closePopupInput = FindObjectOfType<ClosePopupInput>(true);
    }

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (panelBlocker != null) panelBlocker.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = savedGameSpeed;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (panelBlocker != null) panelBlocker.SetActive(false);
    }

    public void SetGameSpeed(float speed)
    {
        savedGameSpeed = speed; // 변수에 저장

        // 일시 정지 상태가 아닐 때만 즉시 적용
        if (!isPaused)
        {
            Time.timeScale = savedGameSpeed;
        }
    }

    public void OpenTutorialFromPause()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OpenTutorial();
        }
    }

    public void ReturnToTitle()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("TitleScene");
    }

    public void UnlockEndingButton()
    {
        if (watchEndingButton != null)
        {
            watchEndingButton.SetActive(true);
            
            if (NotificationController.instance != null)
                NotificationController.instance.ShowNotification("엔딩을 볼 수 있습니다!");
        }
    }

    public void ShowEnding()
    {
        Time.timeScale = 0f; 

        if (endingPanel != null)
        {
            endingPanel.SetActive(true);
            endingPanel.transform.SetAsLastSibling(); 
        }
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.UI.ClosePopup.performed += OnGlobalClosePopup;
    }

    private void OnDisable()
    {
        inputActions.UI.ClosePopup.performed -= OnGlobalClosePopup;
        inputActions.Disable();
    }

    private void OnGlobalClosePopup(InputAction.CallbackContext context)
    {
        // 만약 팝업이 이미 열려있다면, ClosePopupInput 스크립트가 처리를 담당하므로 여기서는 무시.
        if (PopupManager != null && PopupManager.activeSelf)
        {
            return;
        }

        // 팝업이 없는 상태에서 입력을 받으면, 최상위 팝업 닫기 시도 (결과적으로 Pause가 호출될 것)
        if (closePopupInput != null)
        {
            closePopupInput.TryCloseTopPopup();
        }
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
        if (PreparePanel != null) PreparePanel.SetActive(false);
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
        NextDayButton.SetActive(currentState == GameState.Settlement);

        if (OpenButton != null) OpenButton.gameObject.SetActive(currentState == GameState.Preparing);
        if (preparePanelToggleButton != null) preparePanelToggleButton.SetActive(currentState == GameState.Preparing);

        bool isPreparing = (currentState == GameState.Preparing);

        if (TimeScaleButton != null) TimeScaleButton.gameObject.SetActive(currentState == GameState.Open);

        if (UpgradeCountertop != null)
        {
            bool canShowButton = isPreparing && totalGoldAmount >= tablePrice;
            UpgradeCountertop.SetActive(canShowButton);
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
            if (PreparePanel != null) PreparePanel.SetActive(false); 

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
        panelBlocker.SetActive(false);
        CheckButton.SetActive(false);
    }

    private void ShowSettlementPanal()
    {
        todaysGoldText.text = $"오늘 확득한 골드량: {todaysGold}";
        totalGoldText.text = $"총 보유 골드: {totalGoldAmount}";
        customerCountText.text = $"금일 방문객 수: {todaysCustomers}";

        settlementPanel.SetActive(true);
        panelBlocker.SetActive(true);
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
        // speedState: 0(1배속), 1(2배속), 2(일시정지)
        speedState = (speedState + 1) % 2;

        switch (speedState)
        {
            case 0: // [1배속으로 변경]
                TimeScaleButtonText.text = "X1";
                
                if (GameManager.instance != null)
                {
                    // 1. 매니저에 1배속으로 기억시킴
                    GameManager.instance.SetGameSpeed(1f);
                    
                    // 2. 만약 일시정지 상태였다면 게임 재개
                    if (GameManager.instance.isPaused) 
                        GameManager.instance.ResumeGame();
                }
                break;

            case 1: // [2배속으로 변경]
                TimeScaleButtonText.text = "X2";
                
                if (GameManager.instance != null)
                {
                    // 1. 매니저에 2배속으로 기억시킴
                    GameManager.instance.SetGameSpeed(2f);
                    
                    // 2. 만약 일시정지 상태였다면 게임 재개
                    if (GameManager.instance.isPaused) 
                        GameManager.instance.ResumeGame();
                }
                break;
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

    public void TogglePreparePanel()
    {
        if (PreparePanel != null)
        {
            PreparePanel.SetActive(!PreparePanel.activeSelf);
        }
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

    public void AddTable(Vector3 position)
    {
         if (TablePrefab == null) return;
         totalGold.text = totalGoldAmount.ToString();
         GameObject newTableObject = Instantiate(TablePrefab, position, Quaternion.identity);
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

    // ========== 저장/로드 기능 ==========

    /// <summary>
    /// 게임을 저장합니다. UI 버튼에서 호출할 수 있습니다.
    /// </summary>
    public void SaveGame()
    {
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SaveGame();
            Debug.Log("[GameManager] 저장 버튼 클릭 - 게임 저장 요청");
        }
        else
        {
            Debug.LogError("[GameManager] SaveLoadManager를 찾을 수 없습니다!");
            if (NotificationController.instance != null)
                NotificationController.instance.ShowNotification("저장에 실패했습니다!");
        }
    }

    /// <summary>
    /// 게임을 로드합니다. UI 버튼에서 호출할 수 있습니다.
    /// </summary>
    public void LoadGame()
    {
        if (SaveLoadManager.Instance != null)
        {
            if (SaveLoadManager.Instance.HasSaveFile())
            {
                SaveLoadManager.Instance.LoadGame();
                Debug.Log("[GameManager] 로드 버튼 클릭 - 게임 로드 요청");
            }
            else
            {
                Debug.LogWarning("[GameManager] 저장 파일이 없습니다.");
                if (NotificationController.instance != null)
                    NotificationController.instance.ShowNotification("저장 파일이 없습니다!");
            }
        }
        else
        {
            Debug.LogError("[GameManager] SaveLoadManager를 찾을 수 없습니다!");
            if (NotificationController.instance != null)
                NotificationController.instance.ShowNotification("로드에 실패했습니다!");
        }
    }
}
