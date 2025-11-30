using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Buttons")]
    public Button resumeButton;      // 계속하기
    public Button toTitleButton;     // 타이틀로
    public Button toDesktopButton;   // 바탕화면으로 (종료)

    [Header("Scene Name")]
    public string titleSceneName = "TitleScene"; // 타이틀 씬 이름

    private void Awake()
    {
        // 버튼 리스너 연결
        if (resumeButton != null) resumeButton.onClick.AddListener(ClosePauseMenu);
        if (toTitleButton != null) toTitleButton.onClick.AddListener(GoToTitle);
        if (toDesktopButton != null) toDesktopButton.onClick.AddListener(QuitGame);
    }

    /// <summary>
    /// 메뉴 열기 (게임 일시정지)
    /// </summary>
    public void OpenPauseMenu()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f; // 시간 정지
    }

    /// <summary>
    /// 메뉴 닫기 (게임 재개)
    /// </summary>
    public void ClosePauseMenu()
    {
        Time.timeScale = 1f; // 시간 정상화
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 타이틀 화면으로 이동
    /// </summary>
    public void GoToTitle()
    {
        Time.timeScale = 1f; // 씬 이동 전 시간 정상화 필수
        
        // (선택 사항) 만약 타이틀로 갈 때 데이터를 초기화해야 한다면 여기서 매니저 리셋 호출
        // 예: InventoryManager.instance.ResetData();
        
        SceneManager.LoadScene(titleSceneName);
    }

    /// <summary>
    /// 게임 종료
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("게임 종료 (에디터에서는 동작하지 않음)");
        Application.Quit();
    }
}