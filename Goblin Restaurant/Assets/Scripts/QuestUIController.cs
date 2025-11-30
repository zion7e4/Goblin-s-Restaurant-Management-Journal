using UnityEngine;
using System.Collections.Generic;

public class QuestUIController : MonoBehaviour
{
    public GameObject questPanel;
    public Transform questContentParent;
    public GameObject questSlotPrefab;

    void Start()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated += RefreshUI;
        }
        RefreshUI();
    }

    void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated -= RefreshUI;
        }
    }

    public void OpenPanel()
    {
        questPanel.SetActive(true);
        RefreshUI();
    }

    public void ClosePanel()
    {
        questPanel.SetActive(false);
    }

    void RefreshUI()
    {
        if (questContentParent == null || questSlotPrefab == null) return;

        foreach (Transform child in questContentParent) Destroy(child.gameObject);

        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            GameObject go = Instantiate(questSlotPrefab, questContentParent);
            go.GetComponent<QuestSlotUI>().Setup(quest);
        }
    }
}