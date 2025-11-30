using UnityEngine;
using TMPro;
using System.Collections;

public class NotificationController : MonoBehaviour
{
    public static NotificationController instance;

    public TextMeshProUGUI notificationText;
    public CanvasGroup canvasGroup;

    private Coroutine fadeCoroutine;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
    void Start()
    {
        gameObject.SetActive(false);
    }

    public void ShowNotification(string message)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        notificationText.text = message;
        gameObject.SetActive(true);

        fadeCoroutine = StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(1.5f);

        float duration = 0.5f;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / duration);
            yield return null;
        }

        gameObject.SetActive(false);
    }
}