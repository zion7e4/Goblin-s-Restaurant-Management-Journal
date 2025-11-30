using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Buttons")]
    public Button resumeButton;
    public Button toTitleButton;
    public Button toDesktopButton;

    [Header("Scene Name")]
    public string titleSceneName = "TitleScene"; 

    void Awake()
    {
        // '계속하기' 버튼을 누르면 GameManager를 통해 닫기 명령을 수행
        if (resumeButton != null) resumeButton.onClick.AddListener(() => GameManager.instance.ClosePauseMenu());
        if (toTitleButton != null) toTitleButton.onClick.AddListener(GoToTitle);
        if (toDesktopButton != null) toDesktopButton.onClick.AddListener(QuitGame);
    }

    // ▼▼▼ [추가] GameManager가 호출할 함수들 ▼▼▼
    public void OpenPauseMenu()
    {
        gameObject.SetActive(true); // 패널 켜기
        Time.timeScale = 0f;        // 시간 정지
    }

    public void ClosePauseMenu()
    {
        Time.timeScale = 1f;        // 시간 정상화
        gameObject.SetActive(false); // 패널 끄기
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    public void GoToTitle()
    {
        Time.timeScale = 1f; // 시간 정상화 (필수)
        SceneManager.LoadScene(titleSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("게임 종료");
        Application.Quit();
    }
}