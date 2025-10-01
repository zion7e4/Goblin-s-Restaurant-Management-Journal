using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

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
                ShowSettlementPanal(); // 일일 정산 패널 표시
            }
        }
    }

    public float dayDurationInSeconds = 600f; // 실제 하루 길이 (10분)
    public int totalGoldAmount = 0; // 총 골드 변수 추가
    private int todaysGold = 0; // 오늘 번 골드
    private int todaysCustomers = 0; // 오늘 방문한 고객 수
    private float currentTimeOfDay;
    private int DayCount = 1; // 며칠째인지 세는 변수 추가
    private float timeScale; // 게임 내 시간 흐름 속도
    private int speedState = 0; // 시간 배속 상태 변수 추가
    private bool hasPlacedTable = false; // 테이블 배치 여부 변수 추가s
    private Camera mainCamera;

    public TextMeshProUGUI timeText; // 화면에 시간을 표시할 UI 텍스트
    public TextMeshProUGUI dayText; // 화면에 날짜를 표시할 UI 텍스트
    public TextMeshProUGUI totalGold; // 화면에 총 골드를 표시할 UI 텍스트

    public GameObject OpenButton; // 오픈 버튼 ui
    public GameObject NextDayButton; // 다음 날 버튼 ui
    public GameObject TablePrefab; // 테이블 프리팹
    public GameObject settlementPanel; // 일일 정산 패널
    public GameObject CheckButton; // 확인 버튼
    public TextMeshProUGUI todaysGoldText;
    public TextMeshProUGUI totalGoldText;
    public TextMeshProUGUI customerCountText;
    public GameObject MenuPlanner; // 메뉴 기획 패널
    public GameObject ShowMenuPlanner; // 메뉴 기획 패널 오픈 버튼
    public GameObject UpgradeTableButton; // 테이블 업그레이드 버튼
    public GameObject UpgradeTablePannal; // 테이블 업그레이드 패널
    public TextMeshProUGUI TimeScaleButtonText;

    private InputSystem_Actions inputActions; // 생성된 Input Action C# 클래스

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

    void Start()
    {
        currentState = GameState.Preparing;
        // 게임 내 9시간(09~18시)을 실제 10분으로 계산
        timeScale = (9 * 60 * 60) / dayDurationInSeconds;
        currentTimeOfDay = 9 * 3600; // 오전 9시에서 시작 (초 단위)
        timeText.text = "09:00";
        dayText.text = "Day " + DayCount;
        totalGold.text = "Gold: " + totalGoldAmount; // 총 골드 초기화
        Time.timeScale = 1; // 초기 시간 배속
        TimeScaleButtonText.text = "X1";
        Debug.Log("오픈 준비 시간입니다.");
        mainCamera = Camera.main;
    }

    void Update()
    {
        // 가게 운영 상태일 때만 시간이 흐름
        if (currentState == GameState.Open)
        {
            currentTimeOfDay += Time.deltaTime * timeScale;

            // 시간 UI 업데이트 (예: 13:30)
            int hours = (int)(currentTimeOfDay / 3600);
            int minutes = (int)((currentTimeOfDay % 3600) / 60);
            timeText.text = string.Format("{0:D2}:{1:D2}", hours, minutes);
            dayText.text = "Day " + DayCount;

            // 18시가 되면 마감 상태로 변경
            if (currentTimeOfDay >= 18 * 3600)
            {
                currentState = GameState.Closing;
                Debug.Log("영업 종료");

            }
        }
    }

    private void UpdateButtonUI()
    {
        // 각 상태에 맞는 버튼만 활성화(true)하고 나머지는 비활성화(false)
        OpenButton.SetActive(currentState == GameState.Preparing);
        ShowMenuPlanner.SetActive(currentState == GameState.Preparing);
        UpgradeTableButton.SetActive(currentState == GameState.Preparing && totalGoldAmount >= 100 && !hasPlacedTable);
        NextDayButton.SetActive(currentState == GameState.Closing);
    }

    public void OpenTheStore()
    {
        if (currentState == GameState.Preparing)
        {
            currentState = GameState.Open;
            Debug.Log("영업 시작");
        }
    }

    public void MoveToNextDay()
    {
        if (currentState == GameState.Closing)
        {
            timeText.text = "09:00";
            todaysGold = 0; // 오늘 번 골드 초기화
            todaysCustomers = 0; // 오늘 방문객 수 초기화

            currentState = GameState.Preparing;
            currentTimeOfDay = 9 * 3600; // 다음 날 오전 9시로 초기화
            DayCount += 1; // 며칠째인지 증가
            dayText.text = "Day " + DayCount;
            Debug.Log("다음 날 준비를 시작합니다.");
        }
    }

    public void closeSettlementPanal()
    {
        settlementPanel.SetActive(false); // 일일 정산 패널 닫기
        CheckButton.SetActive(false); // 확인 버튼 닫기
    }

    private void ShowSettlementPanal()
    {
        todaysGoldText.text = $"오늘 확득한 골드량: {todaysGold}";
        totalGoldText.text = $"총 보유 골드: {totalGoldAmount}";
        customerCountText.text = $"금일 방문객 수: {todaysCustomers}";

        settlementPanel.SetActive(true); // 일일 정산 패널 열기
        CheckButton.SetActive(true); // 확인 버튼 열기
    }

    public void AddGold(int amount)
    {
        totalGoldAmount += amount; // 총 골드에 추가
        todaysGold += amount; // 오늘 번 골드에 추가
        totalGold.text = "Gold: " + totalGoldAmount; // UI 업데이트
    }

    public void AddCustomerCount()
    {
        todaysCustomers += 1; // 오늘 방문한 고객 수 증가
    }

    public void ChangeTimeScale()
    {
        speedState = (speedState + 1) % 3;

        switch (speedState)
        {
            case 0:
                Time.timeScale = 1;
                TimeScaleButtonText.text = "X1";
                break;
            case 1:
                Time.timeScale = 2;
                TimeScaleButtonText.text = "X2";
                break;
            case 2:
                Time.timeScale = 0;
                TimeScaleButtonText.text = "||";
                break;
        }
    }

    public void OpenMenuPlanner()
    {
        MenuPlanner.SetActive(true);
        ShowMenuPlanner.SetActive(false);
    }

    public void CloseMenuPlanner()
    {
        MenuPlanner.SetActive(false);
        ShowMenuPlanner.SetActive(true);
    }

    public void AddTable(Transform buttonTransform, int price)
    {
        if (TablePrefab == null)
        {
            Debug.LogError("GameManager에 Table Prefab이 연결되지 않았습니다!");
            return;
        }

        // 골드 차감 로직
        totalGoldAmount -= price;
        totalGold.text = "Gold: " + totalGoldAmount;

        // 스크린 좌표를 월드 좌표로 변환
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(buttonTransform.position);
        worldPosition.z = 0f;

        // 변환된 위치에 테이블 생성 및 리스트에 추가
        GameObject newTableObject = Instantiate(TablePrefab, worldPosition, Quaternion.identity);
        Table newTableComponent = newTableObject.GetComponent<Table>();

        if (newTableComponent != null && RestaurantManager.instance != null)
        {
            RestaurantManager.instance.tables.Add(newTableComponent);
        }

        hasPlacedTable = true; // 테이블이 배치되었음을 표시

        Debug.Log($"테이블을 {worldPosition} 위치에 생성했습니다.");
    }

    public void OpenUpgradeTablePannal()
    {
        UpgradeTablePannal.SetActive(true);
    }

    public void CloseUpgradeTablePannal()
    {
        UpgradeTablePannal.SetActive(false);
    }
}
