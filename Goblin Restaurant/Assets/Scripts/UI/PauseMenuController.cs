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
        if (resumeButton != null) resumeButton.onClick.AddListener(() => GameManager.instance.ClosePauseMenu());
        if (toTitleButton != null) toTitleButton.onClick.AddListener(GoToTitle);
        if (toDesktopButton != null) toDesktopButton.onClick.AddListener(QuitGame);
    }

    public void GoToTitle()
    {
        Time.timeScale = 1f; // 시간 정상화
        SceneManager.LoadScene(titleSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("게임 종료");
        Application.Quit();
    }
}