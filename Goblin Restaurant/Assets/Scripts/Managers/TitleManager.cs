using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button continueButton; // 이어하기
    public Button newGameButton;  // 새 게임
    public Button exitButton;     // 종료

    [Header("Scene Name")]
    public string gameSceneName = "GameScene"; // 실제 게임 씬 이름

    void Start()
    {
        if (continueButton != null)
        {
            bool hasSave = SaveManager.Instance != null && SaveManager.Instance.HasSaveFile();
            continueButton.interactable = hasSave;
            continueButton.onClick.AddListener(OnContinueClick);
        }

        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(OnNewGameClick);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitClick);
        }
    }

    void OnContinueClick()
    {
        if (SaveManager.Instance != null)
        {
            if (SaveManager.Instance.LoadGame())
            {
                SceneManager.LoadScene(gameSceneName);
            }
        }
    }

    void OnNewGameClick()
    {
        // ▼▼▼ [수정] 모든 매니저 데이터 초기화 ▼▼▼
        
        // 1. 인벤토리 초기화
        if (InventoryManager.instance != null) 
        {
            InventoryManager.instance.playerIngredients.Clear();
            InventoryManager.instance.discoveredIngredients.Clear();
        }

        // 2. 게임 상태(골드, 날짜) 초기화
        if (GameManager.instance != null)
        {
            GameManager.instance.totalGoldAmount = 0; // 초기 자금
            GameManager.instance.DayCount = 1;
            // 필요한 변수들 초기화
        }

        // 3. 명성 초기화
        if (FameManager.instance != null)
        {
            FameManager.instance.CurrentFamePoints = 0; // 초기값
            // 레벨 업데이트 필요시 호출
        }
        
        // 4. 직원 초기화 (주인공 제외하고 삭제 등 기획에 맞게)
        // if (EmployeeManager.Instance != null) ...

        // 5. 레시피 초기화
        // if (RecipeManager.instance != null) ...

        SceneManager.LoadScene(gameSceneName);
        Debug.Log("새 게임 시작 (데이터 초기화 완료)");
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
    }
    void OnExitClick()
    {
        Application.Quit();
    }
}