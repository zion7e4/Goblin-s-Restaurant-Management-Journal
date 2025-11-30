using System;
using System.Collections.Generic;
using System.Linq;

public enum QuestType { Main, Daily, VIP }
public enum QuestTargetType { Collect, Count, Level, None }

[Serializable]
public class QuestData
{
    public int id;
    public QuestType type;
    public string title;
    public string description;
    public QuestTargetType targetType;
    public string target;      
    public int targetFigure;     
    public string triggerCondition;
    public string reward;

    public bool isUnlocked = false;
    public bool isCompleted = false;
    public bool isRewardClaimed = false;
    
    public Dictionary<string, int> progressDict = new Dictionary<string, int>();

    public QuestData(string[] values)
    {
        id = int.Parse(values[0]);
        type = ParseQuestType(values[1]);
        title = CleanString(values[2]);
        description = CleanString(values[3]);
        targetType = ParseTargetType(values[4]);
        target = CleanString(values[5]);
        targetFigure = int.Parse(values[6]);
        triggerCondition = CleanString(values[7]);
        reward = CleanString(values[8]);

        InitializeProgressDict();
    }

    private string CleanString(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "";
        return raw.Trim().Replace("\"", "");
    }

    public void InitializeProgressDict()
    {
        progressDict.Clear();
        string[] targets = target.Split(',');
        foreach (var t in targets)
        {
            string key = t.Trim();
            if (!string.IsNullOrEmpty(key))
            {
                progressDict[key] = 0; 
            }
        }
    }

    public bool CheckCompletion()
    {
        if (progressDict.Count == 0) return false;

        foreach (var kvp in progressDict)
        {
            if (kvp.Value < targetFigure) return false;
        }
        return true;
    }

    public string GetProgressString()
    {
        if (targetType == QuestTargetType.Count)
            return $"{progressDict.Values.Sum()} / {targetFigure}";
            
        List<string> status = new List<string>();
        foreach(var kvp in progressDict)
        {
            string displayName = kvp.Key; 
            
            status.Add($"{displayName}: {kvp.Value}/{targetFigure}");
        }
        return string.Join("\n", status);
    }

    QuestType ParseQuestType(string s)
    {
        s = s.Trim().ToLower();
        if (s == "main") return QuestType.Main;
        if (s == "daily") return QuestType.Daily;
        if (s == "vip") return QuestType.VIP;
        return QuestType.Main;
    }

    QuestTargetType ParseTargetType(string s)
    {
        s = s.Trim().ToLower();
        if (s == "collect") return QuestTargetType.Collect;
        if (s == "count") return QuestTargetType.Count;
        if (s == "level") return QuestTargetType.Level;
        return QuestTargetType.None;
    }
}