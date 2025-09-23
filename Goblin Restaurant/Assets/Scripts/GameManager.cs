using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum GameState { Preparing, Open, Closing }
    public GameState currentState;

    public float dayDurationInSeconds = 600f; // 실제 하루 길이 (10분)
    public int totalGoldAmount = 0; // 총 골드 변수 추가
    private float currentTimeOfDay;
    private int DayCount = 1; // 며칠째인지 세는 변수 추가
    private float timeScale; // 게임 내 시간 흐름 속도

    public TextMeshProUGUI timeText; // 화면에 시간을 표시할 UI 텍스트
    public TextMeshProUGUI dayText; // 화면에 날짜를 표시할 UI 텍스트
    public TextMeshProUGUI totalGold; // 화면에 총 골드를 표시할 UI 텍스트

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
        inputActions.GameManager.OpenStore.performed += OpenTheStore;
        inputActions.GameManager.NextDay.performed += MoveToNextDay; //왜 씨발 넥스트데이를 처 추가해쑈는데 이병신같은 유니티는 인식을 못하는거니???? 저장까지 잘했잖아 씨봉방거 좆같네
    }

    private void OnDisable()
    {
        inputActions.GameManager.OpenStore.performed -= OpenTheStore;
        inputActions.GameManager.NextDay.performed -= MoveToNextDay;
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
        Debug.Log("오픈 준비 시간입니다.");
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

        if (currentState == GameState.Preparing)
        {
            timeText.text = "09:00";
            dayText.text = "Day " + DayCount;
        }
    }

    private void OpenTheStore(InputAction.CallbackContext context)
    {
        if (currentState == GameState.Preparing)
        {
            currentState = GameState.Open;
            Debug.Log("영업 시작");
        }
    }

    private void MoveToNextDay(InputAction.CallbackContext context)
    {
        if (currentState == GameState.Closing)
        {
            currentState = GameState.Preparing;
            currentTimeOfDay = 9 * 3600; // 다음 날 오전 9시로 초기화
            DayCount += 1; // 며칠째인지 증가
            Debug.Log("다음 날 준비를 시작합니다.");
        }
    }

    public void AddGold(int amount)
    {
        totalGoldAmount += amount;
        totalGold.text = "Gold: " + totalGoldAmount; // UI 업데이트
    }
}
