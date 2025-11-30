using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class QuestSlotUI : MonoBehaviour
{
    [Header("Text Fields")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI rewardText;
    
    [Header("Buttons")]
    public Button claimButton; 
    public TextMeshProUGUI claimButtonText; 

    [Header("Check Box")]
    [Tooltip("완료 시 켜질 체크 표시 아이콘")]
    public GameObject checkMarkObject; 

    private QuestData myQuest;

    public void Setup(QuestData quest)
    {
        myQuest = quest;

        titleText.text = quest.title;
        descText.text = quest.description;

        rewardText.text = $"보상: {GetReadableRewardText(quest.reward)}";

        if (checkMarkObject != null)
        {
            checkMarkObject.SetActive(quest.isCompleted);
        }

        claimButton.onClick.RemoveAllListeners();

        if (quest.isRewardClaimed)
        {
            claimButton.interactable = false;
            claimButtonText.text = "수령 완료";
        }
        else if (quest.isCompleted)
        {
            claimButton.interactable = true;
            claimButtonText.text = "보상 받기";
            claimButton.onClick.AddListener(() => QuestManager.Instance.ClaimReward(myQuest));
        }
        else
        {
            claimButton.interactable = false;
            claimButtonText.text = "진행 중";
        }
    }

    private string GetReadableRewardText(string rawReward)
    {
        if (rawReward.Contains("rcp_id"))
        {
            string idStr = Regex.Match(rawReward, @"\d+").Value;
            
            if (int.TryParse(idStr, out int id))
            {
                if (GameDataManager.instance != null)
                {
                    RecipeData data = GameDataManager.instance.GetRecipeDataById(id);
                    if (data != null)
                    {
                        return $"{data.recipeName} 레시피";
                    }
                }
            }
            return "알 수 없는 레시피";
        }
        return rawReward;
    }
}