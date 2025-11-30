using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button continueButton; // 이어하기 버튼
    public Button newGameButton;  // 새 게임 버튼
    public Button exitButton;     // 종료 버튼

    [Header("Scene Name")]
    public string gameSceneName = "MainScene"; // 이동할 게임 씬 이름

    void Start()
    {
        // 이어하기 버튼 활성화/비활성화 (저장 파일 없으면 비활성화)
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
            // 1. 데이터를 로드합니다.
            if (SaveManager.Instance.LoadGame())
            {
                // 2. 로드 성공 시 게임 씬으로 이동합니다.
                SceneManager.LoadScene(gameSceneName);
            }
        }
    }

    void OnNewGameClick()
    {
        if (InventoryManager.instance != null) InventoryManager.instance.playerIngredients.Clear();
        if (RecipeManager.instance != null) 
        {
            // 기본 레시피 외에는 잠그는 로직 등이 필요할 수 있음
            // RecipeManager.instance.ResetData(); 
        }
        if (FameManager.instance != null) FameManager.instance.CurrentFamePoints = 0; // 초기값

        // 2. 씬 로드 (저장된 파일을 로드하지 않고 씬만 엽니다)
        SceneManager.LoadScene(gameSceneName);
        
        Debug.Log("새 게임 시작");
    }

    void OnExitClick()
    {
        Debug.Log("게임 종료");
        Application.Quit();
    }
}