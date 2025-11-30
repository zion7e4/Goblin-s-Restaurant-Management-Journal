using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class QuestHUDController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI questDescText;
    public GameObject incompleteIcon; 
    public GameObject completeIcon; 
    public GameObject contentGroup;

    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated += UpdateHUD;
            UpdateHUD(); 
        }
    }

    void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated -= UpdateHUD;
        }
    }

    void Update()
    {
        if (GameManager.instance != null && canvasGroup != null)
        {
            bool shouldShow = (GameManager.instance.currentState == GameManager.GameState.Preparing || 
                               GameManager.instance.currentState == GameManager.GameState.Settlement);
            
            canvasGroup.alpha = shouldShow ? 1f : 0f;
            canvasGroup.interactable = shouldShow;
            canvasGroup.blocksRaycasts = shouldShow;
        }
    }
    
    public void UpdateHUD()
    {
        if (QuestManager.Instance == null) return;

        var currentQuest = QuestManager.Instance.activeQuests
            .Where(q => !q.isRewardClaimed && q.type == QuestType.Main)
            .OrderBy(q => q.id)
            .FirstOrDefault();

        if (currentQuest != null)
        {
            if (contentGroup != null) contentGroup.SetActive(true);
            
            if (questTitleText != null) questTitleText.text = currentQuest.title;
            if (questDescText != null) questDescText.text = currentQuest.description;

            bool isDone = currentQuest.isCompleted;

            if (incompleteIcon != null) incompleteIcon.SetActive(!isDone);
            if (completeIcon != null) completeIcon.SetActive(isDone);    
        }
        else
        {
            if (contentGroup != null) contentGroup.SetActive(false);
        }
    }
}