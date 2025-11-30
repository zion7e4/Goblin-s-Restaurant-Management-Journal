using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Quest Data File")]
    public TextAsset questCsvFile;

    [Header("Quest Lists")]
    public List<QuestData> allQuests = new List<QuestData>();
    public List<QuestData> activeQuests = new List<QuestData>();
    
    public event System.Action OnQuestUpdated;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        LoadQuestData();
        CheckUnlockConditions(); 
    }

    void LoadQuestData()
    {
        if (questCsvFile == null) return;
        allQuests.Clear();
        string[] lines = questCsvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            string[] values = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

            if (values.Length >= 9)
            {
                try { allQuests.Add(new QuestData(values)); }
                catch { Debug.LogWarning($"퀘스트 파싱 에러 라인: {i}"); }
            }
        }
        Debug.Log($"[QuestManager] 퀘스트 {allQuests.Count}개 로드 완료.");
    }

    public void UpdateProgress(QuestTargetType type, string targetKeyword, int amount)
    {
        bool updated = false;
        foreach (var quest in activeQuests.ToList())
        {
            if (quest.isCompleted) continue;
            if (quest.targetType != type) continue;

            string key = targetKeyword.Trim();
            
            if (type == QuestTargetType.Count && quest.target.Trim() == key)
            {
                if (!quest.progressDict.ContainsKey(key)) quest.progressDict[key] = 0;
                quest.progressDict[key] += amount;
                updated = true;
            }
            else if (quest.progressDict.ContainsKey(key))
            {
                quest.progressDict[key] += amount;
                updated = true;
            }

            if (updated)
            {
                Debug.Log($"[Quest] '{quest.title}' 진행: {key} -> {quest.progressDict[key]}/{quest.targetFigure}");
                
                if (quest.CheckCompletion())
                {
                    CompleteQuest(quest);
                }
            }
        }
        if (updated) OnQuestUpdated?.Invoke();
    }

    public void SetProgress(QuestTargetType type, string targetKeyword, int currentValue)
    {
        bool updated = false;
        foreach (var quest in activeQuests.ToList())
        {
            if (quest.isCompleted) continue;
            if (quest.targetType != type) continue;

            string key = targetKeyword.Trim();

            if (quest.progressDict.ContainsKey(key) || quest.target.Trim() == key)
            {
                if (!quest.progressDict.ContainsKey(key)) quest.progressDict[key] = 0;

                quest.progressDict[key] = currentValue;
                updated = true;

                Debug.Log($"[Quest] '{quest.title}' 상태 갱신: {currentValue} / {quest.targetFigure}");

                if (quest.CheckCompletion())
                {
                    CompleteQuest(quest);
                }
            }
        }
        if (updated) OnQuestUpdated?.Invoke();
    }

    void CompleteQuest(QuestData quest)
    {
        quest.isCompleted = true;
        
        Debug.Log($"★★ [QuestManager] 퀘스트 달성! : {quest.title} (보상 대기중)");
        
        if (NotificationController.instance != null)
            NotificationController.instance.ShowNotification($"퀘스트 달성!\n{quest.title}");
            
        OnQuestUpdated?.Invoke();
        
        CheckUnlockConditions(); 
    }

    public void ClaimReward(QuestData quest)
    {
        if (!quest.isCompleted || quest.isRewardClaimed) return;

        quest.isRewardClaimed = true;
        GiveReward(quest.reward);
        
        CheckUnlockConditions();
        
        OnQuestUpdated?.Invoke();

    }

    void GiveReward(string rewardStr)
    {
        Debug.Log($"[Quest] 보상 지급: {rewardStr}");
        
        string numberStr = Regex.Match(rewardStr, @"\d+").Value;
        int.TryParse(numberStr, out int amount);

        if (rewardStr.Contains("골드")) 
        {
            GameManager.instance.AddGold(amount);
        }
        else if (rewardStr.Contains("명성도")) 
        {
            FameManager.instance.AddFame(amount);
        }
        else if (rewardStr.Contains("rcp_id")) 
        {
            RecipeManager.instance.UnlockRecipe(amount);
        }
        else if (rewardStr.Contains("레시피 도감 활성화"))
        {
            GameManager.instance.UnlockRecipeSystem();
        }
        else if (rewardStr.Contains("직원 메뉴 활성화"))
        {
            GameManager.instance.UnlockEmployeeSystem();
        }
    }

    public void CheckUnlockConditions()
    {
        foreach (var quest in allQuests)
        {
            if (quest.isUnlocked) continue;

            bool conditionMet = false;
            string cond = quest.triggerCondition.Replace(" ", ""); 

            if (cond.Contains("게임처음시작시")) conditionMet = true;
            else if (cond.Contains("qst_") && cond.Contains("완료시"))
            {
                string idStr = Regex.Match(cond, @"\d+").Value;
                if (int.TryParse(idStr, out int reqId))
                {
                    var prevQuest = allQuests.FirstOrDefault(q => q.id == reqId);
                    if (prevQuest != null && prevQuest.isRewardClaimed) 
                    {
                        conditionMet = true;
                    }
                }
            }
            else if (cond.Contains("명성도") && cond.Contains("레벨"))
            {
                string levelStr = Regex.Match(cond, @"\d+").Value;
                if (int.TryParse(levelStr, out int reqLevel))
                {
                    if (FameManager.instance != null && FameManager.instance.CurrentFameLevel >= reqLevel)
                        conditionMet = true;
                }
            }

            if (conditionMet)
            {
                quest.isUnlocked = true;
                quest.InitializeProgressDict();
                activeQuests.Add(quest);
                
                Debug.Log($"★ [QuestManager] 새 퀘스트 해금: {quest.title}");
                CheckImmediateCompletion(quest);
                
                if (NotificationController.instance != null)
                    NotificationController.instance.ShowNotification($"새 임무!\n{quest.title}");
                    
                OnQuestUpdated?.Invoke();
            }
        }
    }

    void CheckImmediateCompletion(QuestData quest)
    {
        if (quest.targetType == QuestTargetType.Collect && quest.target.Contains("골드 보유량"))
        {
            if (GameManager.instance != null)
            {
                int currentGold = GameManager.instance.totalGoldAmount;
                SetProgress(QuestTargetType.Collect, "골드 보유량", currentGold);
            }
        }
        else if (quest.targetType == QuestTargetType.Count && quest.target.Contains("메뉴 편성 개수"))
        {
            if (MenuPlanner.instance != null)
            {
                int count = MenuPlanner.instance.dailyMenu.Count(r => r != null);
                SetProgress(QuestTargetType.Count, "메뉴 편성 개수", count);
            }
        }
        else if (quest.targetType == QuestTargetType.Collect && quest.target.Contains("고용한 직원 수"))
        {
            if (EmployeeManager.Instance != null)
            {
                int count = EmployeeManager.Instance.hiredEmployees.Count(e => !e.isProtagonist);
                SetProgress(QuestTargetType.Collect, "고용한 직원 수", count);
            }
        }
        else if (quest.targetType == QuestTargetType.Count && quest.target.Contains("직원 배치"))
        {
             if (EmployeeManager.Instance != null)
             {
                 int count = EmployeeManager.Instance.hiredEmployees
                    .Count(e => e.assignedRole != EmployeeRole.Unassigned);
                 SetProgress(QuestTargetType.Count, "배치된 직원 수", count);
             }
        }
        else if (quest.targetType == QuestTargetType.Level && quest.target.Contains("명성도 레벨"))
        {
             if (FameManager.instance != null)
             {
                 SetProgress(QuestTargetType.Level, "식당 명성도 레벨", FameManager.instance.CurrentFameLevel);
             }
        }
    }
}